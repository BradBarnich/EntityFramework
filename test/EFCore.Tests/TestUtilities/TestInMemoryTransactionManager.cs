// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestInMemoryTransactionManager : InMemoryTransactionManager
    {
        private IDbContextTransaction _currentTransaction;
        private Transaction _enlistedTransaction;

        public TestInMemoryTransactionManager(
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger)
            : base(logger)
        {
        }

        public override IDbContextTransaction CurrentTransaction
            => _currentTransaction;

        public override Transaction EnlistedTransaction
            => _enlistedTransaction;

        public override IDbContextTransaction BeginTransaction()
            => _currentTransaction = new TestInMemoryTransaction(this);

        public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_currentTransaction = new TestInMemoryTransaction(this));

        public override void CommitTransaction()
            => CurrentTransaction.Commit();

        public override Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => CurrentTransaction.CommitAsync(cancellationToken);

        public override void RollbackTransaction()
            => CurrentTransaction.Rollback();

        public override Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => CurrentTransaction.RollbackAsync(cancellationToken);

        public override void EnlistTransaction(Transaction transaction)
            => _enlistedTransaction = transaction;

        private class TestInMemoryTransaction : IDbContextTransaction
        {
            public TestInMemoryTransaction(TestInMemoryTransactionManager transactionManager)
            {
                TransactionManager = transactionManager;
            }

            public Guid TransactionId { get; } = Guid.NewGuid();

            private TestInMemoryTransactionManager TransactionManager { get; }

            public bool SupportsSavepoints => false;

            public void Dispose()
            {
                TransactionManager._currentTransaction = null;
            }

            public void Commit()
            {
                TransactionManager._currentTransaction = null;
            }

            public void Rollback()
            {
                TransactionManager._currentTransaction = null;
            }

            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                TransactionManager._currentTransaction = null;

                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                TransactionManager._currentTransaction = null;

                return Task.CompletedTask;
            }

            public ValueTask DisposeAsync()
            {
                Dispose();

                return default;
            }

            public void CreateSavepoint([NotNull] string name)
                => throw new NotImplementedException();

            public Task CreateSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public void RollbackToSavepoint([NotNull] string name)
                => throw new NotImplementedException();

            public Task RollbackToSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
                => throw new NotImplementedException();

            public void ReleaseSavepoint([NotNull] string name)
                => throw new NotImplementedException();

            public Task ReleaseSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
                => throw new NotImplementedException();
        }
    }
}
