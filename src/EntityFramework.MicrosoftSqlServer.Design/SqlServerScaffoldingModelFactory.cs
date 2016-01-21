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
using Microsoft.Data.Entity.Storage.Internal;
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

        public SqlServerTypeMapper SqlServerTypeMapper => TypeMapper as SqlServerTypeMapper;

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
            else if (!HasTypeAlias(column))
            {
                var qualifiedColumnTypeAndMaxLength =
                    SqlServerTypeMapper.MaxLengthQualifiedDataType(column.DataType, column.MaxLength);
                propertyBuilder.HasColumnType(qualifiedColumnTypeAndMaxLength.Item1);
                propertyBuilder.Metadata.SetMaxLength(qualifiedColumnTypeAndMaxLength.Item2);
            }

            return propertyBuilder;
        }

        private bool HasTypeAlias(ColumnModel column)
            => column.Table.Database.SqlServer().TypeAliases?.ContainsKey(column.DataType) == true;

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
