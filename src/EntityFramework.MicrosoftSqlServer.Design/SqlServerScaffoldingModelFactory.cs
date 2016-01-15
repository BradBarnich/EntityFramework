// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqlServerScaffoldingModelFactory : RelationalScaffoldingModelFactory
    {
        private const int DefaultTimeTimePrecision = 7;

        private static readonly ISet<string> _dataTypesForbiddingLength =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image", "ntext", "text" };
        private static readonly ISet<string> _dataTypesAllowingMaxLength =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "varchar", "nvarchar", "varbinary" };

        public SqlServerScaffoldingModelFactory(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IDatabaseModelFactory databaseModelFactory)
            : base(loggerFactory, typeMapper, databaseModelFactory)
        {
        }

        public override IModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            var model = base.Create(connectionString, tableSelectionSet);
            model.Scaffolding().UseProviderMethodName = nameof(SqlServerDbContextOptionsExtensions.UseSqlServer);
            return model;
        }

        protected override PropertyBuilder VisitColumn([NotNull] EntityTypeBuilder builder, [NotNull] ColumnModel column)
        {
            var propertyBuilder = base.VisitColumn(builder, column);

            if (propertyBuilder == null)
            {
                return null;
            }

            VisitTypeMapping(propertyBuilder, column);

            VisitDefaultValue(column, propertyBuilder);

            return propertyBuilder;
        }

        protected override RelationalTypeMapping GetTypeMapping([NotNull] ColumnModel column)
        {
            RelationalTypeMapping mapping = null;
            if (column.DataType != null)
            {
                string underlyingDataType = null;
                var typeAliases = column.Table.Database.SqlServer().TypeAliases;
                if (typeAliases != null)
                {
                    typeAliases.TryGetValue(column.DataType, out underlyingDataType);
                }

                mapping = TypeMapper.FindMapping(underlyingDataType ?? column.DataType);
            }

            return mapping;
        }

        protected override KeyBuilder VisitPrimaryKey([NotNull] EntityTypeBuilder builder, [NotNull] TableModel table)
        {
            var keyBuilder = base.VisitPrimaryKey(builder, table);

            if (keyBuilder == null)
            {
                return null;
            }

            // If this property is the single integer primary key on the EntityType then
            // KeyConvention assumes ValueGeneratedOnAdd(). If the underlying column does
            // not have Identity set then we need to set to ValueGeneratedNever() to
            // override this behavior.

            // TODO use KeyConvention directly to detect when it will be applied
            var pkColumns = table.Columns.Where(c => c.PrimaryKeyOrdinal.HasValue).ToList();
            if (pkColumns.Count != 1
                || pkColumns[0].SqlServer().IsIdentity)
            {
                return keyBuilder;
            }

            // TODO 
            var property = builder.Metadata.FindProperty(GetPropertyName(pkColumns[0]));
            var propertyType = property?.ClrType?.UnwrapNullableType();

            if (propertyType?.IsInteger() == true
                || propertyType == typeof(Guid))
            {
                property.ValueGenerated = ValueGenerated.Never;
            }

            return keyBuilder;
        }

        protected override IndexBuilder VisitIndex(EntityTypeBuilder builder, IndexModel index)
        {
            var indexBuilder = base.VisitIndex(builder, index);

            if (index.SqlServer().IsClustered)
            {
                indexBuilder?.ForSqlServerIsClustered();
            }

            return indexBuilder;
        }

        private PropertyBuilder VisitTypeMapping(PropertyBuilder propertyBuilder, ColumnModel column)
        {
            if (column.SqlServer().IsIdentity)
            {
                if (typeof(byte) == propertyBuilder.Metadata.ClrType)
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.DataTypeDoesNotAllowSqlServerIdentityStrategy(
                            column.DisplayName, column.DataType));
                }
                else
                {
                    propertyBuilder
                        .ValueGeneratedOnAdd()
                        .UseSqlServerIdentityColumn();
                }
            }

            if (column.SqlServer().DateTimePrecision.HasValue
                && column.SqlServer().DateTimePrecision != DefaultTimeTimePrecision)
            {
                propertyBuilder.Metadata.SetMaxLength(null);
                propertyBuilder.HasColumnType($"{column.DataType}({column.SqlServer().DateTimePrecision.Value})");
            }

            if (!IsTypeAlias(column)
                && !_dataTypesForbiddingLength.Contains(propertyBuilder.Metadata.Relational().ColumnType))
            {
                var typeMapping = GetTypeMapping(column);
                if (typeof(string) == typeMapping.ClrType
                    || typeof(byte[]) == typeMapping.ClrType)
                {
                    if (typeMapping.DefaultTypeName == "nvarchar"
                        || typeMapping.DefaultTypeName == "varbinary")
                    {
                        // nvarchar is the default column type for string properties,
                        // so we don't need to define it using HasColumnType() and removing
                        // the column type allows the HasMaxLength() API to have effect.
                        propertyBuilder.Metadata.Relational().ColumnType = null;
                    }
                    else
                    {
                        // Override the column type to have the length in it.
                        if (_dataTypesAllowingMaxLength.Contains(typeMapping.DefaultTypeName))
                        {
                            propertyBuilder.Metadata.Relational().ColumnType =
                                column.DataType
                                + "("
                                + (column.MaxLength.HasValue ? column.MaxLength.Value.ToString() : "max")
                                + ")";
                        }
                        else
                        {
                            if (column.MaxLength.HasValue)
                            {
                                propertyBuilder.Metadata.Relational().ColumnType =
                                    column.DataType
                                    + "(" + column.MaxLength.Value + ")";
                            }
                        }

                        // Remove the MaxLength annotation so that it does not appear as
                        // fluent API or as an attribute (it would have no effect given
                        // what we did above).
                        propertyBuilder.Metadata.SetMaxLength(null);
                    }
                }
            }

            return propertyBuilder;
        }

        private bool IsTypeAlias(ColumnModel column)
        {
            var typeAliases = column.Table.Database.SqlServer().TypeAliases;
            if (typeAliases != null)
            {
                return typeAliases.ContainsKey(column.DataType);
            }

            return false;
        }

        private PropertyBuilder VisitDefaultValue(ColumnModel column, PropertyBuilder propertyBuilder)
        {
            if (column.DefaultValue != null)
            {
                ((Property)propertyBuilder.Metadata).SetValueGenerated(null, ConfigurationSource.Explicit);
                propertyBuilder.Metadata.Relational().GeneratedValueSql = null;

                var defaultExpression = ConvertSqlServerDefaultValue(column.DefaultValue);
                if (defaultExpression != null)
                {
                    if (!(defaultExpression == "NULL"
                          && propertyBuilder.Metadata.ClrType.IsNullableType()))
                    {
                        propertyBuilder.HasDefaultValueSql(defaultExpression);
                    }
                }
                else
                {
                    Logger.LogWarning(
                        SqlServerDesignStrings.CannotInterpretDefaultValue(
                            column.DisplayName,
                            column.DefaultValue,
                            propertyBuilder.Metadata.Name,
                            propertyBuilder.Metadata.DeclaringEntityType.Name));
                }
            }
            return propertyBuilder;
        }

        private string ConvertSqlServerDefaultValue(string sqlServerDefaultValue)
        {
            if (sqlServerDefaultValue.Length < 2)
            {
                return null;
            }

            while (sqlServerDefaultValue[0] == '('
                   && sqlServerDefaultValue[sqlServerDefaultValue.Length - 1] == ')')
            {
                sqlServerDefaultValue = sqlServerDefaultValue.Substring(1, sqlServerDefaultValue.Length - 2);
            }

            return sqlServerDefaultValue;
        }
    }
}
