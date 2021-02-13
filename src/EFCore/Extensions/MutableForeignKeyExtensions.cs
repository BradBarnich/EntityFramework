// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IMutableForeignKey" />.
    /// </summary>
    public static class MutableForeignKeyExtensions
    {
        /// <summary>
        ///     Gets the entity type related to the given one.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="entityType"> One of the entity types related by the foreign key. </param>
        /// <returns> The entity type related to the given one. </returns>
        public static IMutableEntityType GetRelatedEntityType(
            [NotNull] this IMutableForeignKey foreignKey,
            [NotNull] IMutableEntityType entityType)
            => (IMutableEntityType)((IForeignKey)foreignKey).GetRelatedEntityType(entityType);

        /// <summary>
        ///     Returns a navigation associated with this foreign key.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="pointsToPrincipal">
        ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
        /// </param>
        /// <returns>
        ///     A navigation associated with this foreign key or <see langword="null" />.
        /// </returns>
        public static IMutableNavigation GetNavigation([NotNull] this IMutableForeignKey foreignKey, bool pointsToPrincipal)
            => pointsToPrincipal ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        public static IMutableNavigation HasDependentToPrincipal([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] string name)
            => foreignKey.SetDependentToPrincipal(name);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        public static IMutableNavigation HasDependentToPrincipal([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] MemberInfo property)
            => foreignKey.SetDependentToPrincipal(property);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        public static IMutableNavigation HasPrincipalToDependent([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] string name)
            => foreignKey.SetPrincipalToDependent(name);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        public static IMutableNavigation HasPrincipalToDependent([NotNull] this IMutableForeignKey foreignKey, [CanBeNull] MemberInfo property)
            => foreignKey.SetPrincipalToDependent(property);
    }
}
