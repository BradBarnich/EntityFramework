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
    ///     Extension methods for <see cref="IConventionForeignKey" />.
    /// </summary>
    public static class ConventionForeignKeyExtensions
    {
        /// <summary>
        ///     Gets the entity type related to the given one.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="entityType"> One of the entity types related by the foreign key. </param>
        /// <returns> The entity type related to the given one. </returns>
        public static IConventionEntityType GetRelatedEntityType(
            [NotNull] this IConventionForeignKey foreignKey,
            [NotNull] IConventionEntityType entityType)
            => (IConventionEntityType)((IForeignKey)foreignKey).GetRelatedEntityType(entityType);

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
        public static IConventionNavigation GetNavigation([NotNull] this IConventionForeignKey foreignKey, bool pointsToPrincipal)
            => pointsToPrincipal ? foreignKey.DependentToPrincipal : foreignKey.PrincipalToDependent;

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="name">
        ///     The name of the navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        public static IConventionNavigation HasDependentToPrincipal([NotNull] this IConventionForeignKey foreignKey, [CanBeNull] string name, bool fromDataAnnotation = false)
            => foreignKey.SetDependentToPrincipal(name, fromDataAnnotation);

        /// <summary>
        ///     Sets the navigation property on the dependent entity type that points to the principal entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="property">
        ///     The navigation property on the dependent type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetDependentToPrincipal")]
        public static IConventionNavigation HasDependentToPrincipal([NotNull] this IConventionForeignKey foreignKey, [CanBeNull] MemberInfo property, bool fromDataAnnotation = false)
            => foreignKey.SetDependentToPrincipal(property, fromDataAnnotation);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="name">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        public static IConventionNavigation HasPrincipalToDependent([NotNull] this IConventionForeignKey foreignKey, [CanBeNull] string name, bool fromDataAnnotation = false)
            => foreignKey.SetPrincipalToDependent(name, fromDataAnnotation);

        /// <summary>
        ///     Sets the navigation property on the principal entity type that points to the dependent entity.
        /// </summary>
        /// <param name="foreignKey"> The foreign key. </param>
        /// <param name="property">
        ///     The name of the navigation property on the principal type. Passing <see langword="null" /> will result in there being
        ///     no navigation property defined.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The newly created navigation property. </returns>
        [Obsolete("Use SetPrincipalToDependent")]
        public static IConventionNavigation HasPrincipalToDependent([NotNull] this IConventionForeignKey foreignKey,[CanBeNull] MemberInfo property, bool fromDataAnnotation = false)
            => foreignKey.SetPrincipalToDependent(property, fromDataAnnotation);
    }
}
