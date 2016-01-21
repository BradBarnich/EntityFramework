// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class NotificationEntitiesSqliteTest
        : NotificationEntitiesTestBase<NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture>
    {
        public NotificationEntitiesSqliteTest(NotificationEntitiesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqliteFixture : NotificationEntitiesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlite(SqliteTestStore.CreateConnectionString("NotificationEntities"));
                _options = optionsBuilder.Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_serviceProvider, _options);
        }
    }
}
