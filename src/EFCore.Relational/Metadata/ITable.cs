// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a table in the database.
    /// </summary>
    public interface ITable : ITableBase
    {
        /// <summary>
        ///     Gets the entity type mappings.
        /// </summary>
        new IEnumerable<ITableMapping> EntityTypeMappings { get; }

        /// <summary>
        ///     Gets the columns defined for this table.
        /// </summary>
        new IEnumerable<IColumn> Columns { get; }

        /// <summary>
        ///     Gets the value indicating whether the table should be managed by migrations
        /// </summary>
        bool IsExcludedFromMigrations { get; }

        /// <summary>
        ///     Gets the foreing key constraints for this table.
        /// </summary>
        IEnumerable<IForeignKeyConstraint> ForeignKeyConstraints { get; }

        /// <summary>
        ///     Gets the unique constraints including the primary key for this table.
        /// </summary>
        IEnumerable<IUniqueConstraint> UniqueConstraints { get; }

        /// <summary>
        ///     Gets the primary key for this table.
        /// </summary>
        IPrimaryKeyConstraint PrimaryKey { get; }

        /// <summary>
        ///     Gets the indexes for this table.
        /// </summary>
        IEnumerable<ITableIndex> Indexes { get; }

        /// <summary>
        ///     Gets the check constraints for this table.
        /// </summary>
        IEnumerable<ICheckConstraint> CheckConstraints { get; }

        /// <summary>
        ///     Gets the comment for this table.
        /// </summary>
        string Comment { get; }

        /// <summary>
        ///     Gets the column with a given name. Returns <see langword="null" /> if no column with the given name is defined.
        /// </summary>
        new IColumn FindColumn([NotNull] string name);

        /// <summary>
        ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
        /// </summary>
        new IColumn FindColumn([NotNull] IProperty property);
    }
}
