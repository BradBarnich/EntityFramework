// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class Navigation : PropertyBase, IMutableNavigation, IConventionNavigation
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrCollectionAccessor _collectionAccessor;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Navigation(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] ForeignKey foreignKey)
            : base(name, propertyInfo, fieldInfo, ConfigurationSource.Convention)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            ForeignKey = foreignKey;

            Builder = new InternalNavigationBuilder(this, foreignKey.DeclaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type ClrType
            => this.GetIdentifyingMemberInfo()?.GetMemberType() ?? typeof(object);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey ForeignKey { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalNavigationBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => (EntityType)((INavigation)this).DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TypeBase DeclaringType
        {
            [DebuggerStepThrough]
            get => (EntityType)((INavigation)this).DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType TargetEntityType
        {
            [DebuggerStepThrough]
            get => (EntityType)((INavigationBase)this).TargetEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsOnDependent
        {
            [DebuggerStepThrough]
            get => ForeignKey.DependentToPrincipal == this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ConfigurationSource GetConfigurationSource()
            => (ConfigurationSource)(IsOnDependent
                ? ForeignKey.GetDependentToPrincipalConfigurationSource()
                : ForeignKey.GetPrincipalToDependentConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            if (IsOnDependent)
            {
                ForeignKey.UpdateDependentToPrincipalConfigurationSource(configurationSource);
            }
            else
            {
                ForeignKey.UpdatePrincipalToDependentConfigurationSource(configurationSource);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsEagerLoaded(bool? eagerLoaded, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] string navigationName,
            [CanBeNull] MemberInfo navigationProperty,
            [NotNull] EntityType sourceType,
            [NotNull] EntityType targetType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            var targetClrType = targetType.ClrType;
            if (targetClrType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationToShadowEntity(navigationName, sourceType.DisplayName(), targetType.DisplayName()));
                }

                return false;
            }

            return navigationProperty == null
                || IsCompatible(navigationProperty, sourceType.ClrType, targetClrType, shouldBeCollection, shouldThrow);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsCompatible(
            [NotNull] MemberInfo navigationProperty,
            [NotNull] Type sourceClrType,
            [NotNull] Type targetClrType,
            bool? shouldBeCollection,
            bool shouldThrow)
        {
            if (!navigationProperty.DeclaringType.IsAssignableFrom(sourceClrType))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NoClrNavigation(
                            navigationProperty.Name, sourceClrType.ShortDisplayName()));
                }

                return false;
            }

            var navigationTargetClrType = navigationProperty.GetMemberType().TryGetSequenceType();
            shouldBeCollection ??= navigationTargetClrType != null && navigationProperty.GetMemberType() != targetClrType;
            if (shouldBeCollection.Value
                && navigationTargetClrType?.IsAssignableFrom(targetClrType) != true)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationCollectionWrongClrType(
                            navigationProperty.Name,
                            sourceClrType.ShortDisplayName(),
                            navigationProperty.GetMemberType().ShortDisplayName(),
                            targetClrType.ShortDisplayName()));
                }

                return false;
            }

            if (!shouldBeCollection.Value
                && !navigationProperty.GetMemberType().IsAssignableFrom(targetClrType))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationSingleWrongClrType(
                            navigationProperty.Name,
                            sourceClrType.ShortDisplayName(),
                            navigationProperty.GetMemberType().ShortDisplayName(),
                            targetClrType.ShortDisplayName()));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation Inverse
        {
            [DebuggerStepThrough]
            get => (Navigation)((INavigationBase)this).Inverse;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation SetInverse([CanBeNull] string inverseName, ConfigurationSource configurationSource)
            => IsOnDependent
                ? ForeignKey.HasPrincipalToDependent(inverseName, configurationSource)
                : ForeignKey.SetDependentToPrincipal(inverseName, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation SetInverse([CanBeNull] MemberInfo inverse, ConfigurationSource configurationSource)
            => IsOnDependent
                ? ForeignKey.HasPrincipalToDependent(inverse, configurationSource)
                : ForeignKey.SetDependentToPrincipal(inverse, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetInverseConfigurationSource()
            => IsOnDependent
                ? ForeignKey.GetPrincipalToDependentConfigurationSource()
                : ForeignKey.GetDependentToPrincipalConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IClrCollectionAccessor CollectionAccessor
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _collectionAccessor, this, n => new ClrCollectionAccessorFactory().Create(n));

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation)
            => DeclaringType.Model.ConventionDispatcher.OnNavigationAnnotationChanged(
                ForeignKey.Builder, this, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        PropertyAccessMode IPropertyBase.GetPropertyAccessMode()
            => (PropertyAccessMode)(this[CoreAnnotationNames.PropertyAccessMode]
                ?? DeclaringType.GetNavigationAccessMode());

        IEntityType INavigationBase.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => ((INavigation)this).DeclaringEntityType;
        }

        IEntityType INavigationBase.TargetEntityType
        {
            [DebuggerStepThrough]
            get => ((INavigation)this).TargetEntityType;
        }

        INavigationBase INavigationBase.Inverse
        {
            [DebuggerStepThrough]
            get => ((INavigation)this).Inverse;
        }

        bool INavigationBase.IsCollection
        {
            [DebuggerStepThrough]
            get => ((INavigation)this).IsCollection;
        }

        [DebuggerStepThrough]
        IClrCollectionAccessor INavigationBase.GetCollectionAccessor()
            => CollectionAccessor;

        bool INavigationBase.IsEagerLoaded
            => (bool?)this[CoreAnnotationNames.EagerLoaded] ?? false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IForeignKey INavigation.ForeignKey
        {
            [DebuggerStepThrough] get => ForeignKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType INavigation.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType INavigation.TargetEntityType
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.PrincipalEntityType : ForeignKey.DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        INavigation INavigation.Inverse
        {
            [DebuggerStepThrough]
            get => IsOnDependent ? ForeignKey.PrincipalToDependent : ForeignKey.DependentToPrincipal;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool INavigation.IsCollection
        {
            [DebuggerStepThrough]
            get => !IsOnDependent && !ForeignKey.IsUnique;
        }

        IClrCollectionAccessor INavigation.GetCollectionAccessor()
            => CollectionAccessor;

        void IMutableNavigationBase.SetIsEagerLoaded(bool? eagerLoaded)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded);

        IMutableEntityType IMutableNavigation.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        IMutableEntityType IMutableNavigation.TargetEntityType
        {
            [DebuggerStepThrough] get => TargetEntityType;
        }

        IMutableForeignKey IMutableNavigation.ForeignKey
        {
            [DebuggerStepThrough] get => ForeignKey;
        }

        IMutableNavigation IMutableNavigation.Inverse
        {
            [DebuggerStepThrough] get => Inverse;
        }

        [DebuggerStepThrough]
        IMutableNavigation IMutableNavigation.SetInverse([CanBeNull] string inverseName)
            => SetInverse(inverseName, ConfigurationSource.Explicit);

        [DebuggerStepThrough]
        IMutableNavigation IMutableNavigation.SetInverse([CanBeNull] MemberInfo inverse)
            => SetInverse(inverse, ConfigurationSource.Explicit);

        [DebuggerStepThrough]
        bool? IConventionNavigationBase.SetIsEagerLoaded(bool? eagerLoaded, bool fromDataAnnotation)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.EagerLoaded, eagerLoaded, fromDataAnnotation);
            return eagerLoaded;
        }

        [DebuggerStepThrough]
        ConfigurationSource? IConventionNavigationBase.GetIsEagerLoadedConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.EagerLoaded)?.GetConfigurationSource();

        [DebuggerStepThrough]
        IConventionNavigation IConventionNavigation.SetInverse([CanBeNull] string inverseName, bool fromDataAnnotation)
            => SetInverse(inverseName, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        [DebuggerStepThrough]
        IConventionNavigation IConventionNavigation.SetInverse([CanBeNull] MemberInfo inverse, bool fromDataAnnotation)
            => SetInverse(inverse, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        IConventionNavigationBuilder IConventionNavigation.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }

        IConventionEntityType IConventionNavigation.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        IConventionEntityType IConventionNavigation.TargetEntityType
        {
            [DebuggerStepThrough] get => TargetEntityType;
        }

        IConventionForeignKey IConventionNavigation.ForeignKey
        {
            [DebuggerStepThrough] get => ForeignKey;
        }

        IConventionNavigation IConventionNavigation.Inverse
        {
            [DebuggerStepThrough] get => Inverse;
        }

        ConfigurationSource IConventionPropertyBase.GetConfigurationSource()
            => GetConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionAnnotatableBuilder IConventionAnnotatable.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }
    }
}
