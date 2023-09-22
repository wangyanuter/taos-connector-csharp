﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace IoTSharp.EntityFrameworkCore.Taos.Query.Internal
{
    public class TaosBatchExecutor : IBatchExecutor //,BatchExecutor//
    {
        private const string SavepointName = "__EFSavePoint";
        public TaosBatchExecutor(
        ICurrentDbContext currentContext,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
        {
            CurrentContext = currentContext;
            UpdateLogger = updateLogger;
        }

        public ICurrentDbContext CurrentContext { get; }
        public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

        public virtual int Execute(IEnumerable<ModificationCommandBatch> commandBatches, IRelationalConnection connection)
        {
            using var batchEnumerator = commandBatches.GetEnumerator();

            if (!batchEnumerator.MoveNext())
            {
                return 0;
            }

            var batch = batchEnumerator.Current;

            var rowsAffected = 0;
            var transaction = connection.CurrentTransaction;
            var beganTransaction = false;
            var createdSavepoint = false;
            try
            {
                var transactionEnlistManager = connection as ITransactionEnlistmentManager;
                var autoTransactionBehavior = CurrentContext.Context.Database.AutoTransactionBehavior;
                if (transaction == null
                    && transactionEnlistManager?.EnlistedTransaction is null
                    && transactionEnlistManager?.CurrentAmbientTransaction is null
                    // Don't start a transaction if we have a single batch which doesn't require a transaction (single command), for perf.
                    && ((autoTransactionBehavior == AutoTransactionBehavior.WhenNeeded
                            && (batch.AreMoreBatchesExpected || batch.RequiresTransaction))
                        || autoTransactionBehavior == AutoTransactionBehavior.Always))
                {
                    transaction = connection.BeginTransaction();
                    beganTransaction = true;
                }
                else
                {
                    connection.Open();

                    if (transaction?.SupportsSavepoints == true
                        && CurrentContext.Context.Database.AutoSavepointsEnabled)
                    {
                        transaction.CreateSavepoint(SavepointName);
                        createdSavepoint = true;
                    }
                }

                do
                {
                    batch = batchEnumerator.Current;
                    batch.Execute(connection);
                    rowsAffected += batch.ModificationCommands.Count;
                }
                while (batchEnumerator.MoveNext());

                if (beganTransaction)
                {
                    transaction!.Commit();
                }
            }
            catch
            {
                if (createdSavepoint && connection.DbConnection.State == ConnectionState.Open)
                {
                    try
                    {
                        transaction!.RollbackToSavepoint(SavepointName);
                    }
                    catch (Exception e)
                    {
                        UpdateLogger.BatchExecutorFailedToRollbackToSavepoint(CurrentContext.GetType(), e);
                    }
                }

                throw;
            }
            finally
            {
                if (beganTransaction)
                {
                    transaction!.Dispose();
                }
                else
                {
                    if (createdSavepoint)
                    {
                        if (connection.DbConnection.State == ConnectionState.Open)
                        {
                            try
                            {
                                transaction!.ReleaseSavepoint(SavepointName);
                            }
                            catch (Exception e)
                            {
                                UpdateLogger.BatchExecutorFailedToReleaseSavepoint(CurrentContext.GetType(), e);
                            }
                        }
                    }

                    connection.Close();
                }
            }

            return rowsAffected;
        }



        public virtual async Task<int> ExecuteAsync(
            IEnumerable<ModificationCommandBatch> commandBatches,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            using var batchEnumerator = commandBatches.GetEnumerator();

            if (!batchEnumerator.MoveNext())
            {
                return 0;
            }

            var batch = batchEnumerator.Current;

            var rowsAffected = 0;
            var transaction = connection.CurrentTransaction;
            var beganTransaction = false;
            var createdSavepoint = false;
            try
            {
                var transactionEnlistManager = connection as ITransactionEnlistmentManager;
                var autoTransactionBehavior = CurrentContext.Context.Database.AutoTransactionBehavior;
                if (transaction == null
                    && transactionEnlistManager?.EnlistedTransaction is null
                    && transactionEnlistManager?.CurrentAmbientTransaction is null
                    // Don't start a transaction if we have a single batch which doesn't require a transaction (single command), for perf.
                    && ((autoTransactionBehavior == AutoTransactionBehavior.WhenNeeded
                            && (batch.AreMoreBatchesExpected || batch.RequiresTransaction))
                        || autoTransactionBehavior == AutoTransactionBehavior.Always))
                {
                    transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                    beganTransaction = true;
                }
                else
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    if (transaction?.SupportsSavepoints == true
                        && CurrentContext.Context.Database.AutoSavepointsEnabled)
                    {
                        await transaction.CreateSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                        createdSavepoint = true;
                    }
                }

                do
                {
                    batch = batchEnumerator.Current;
                    await batch.ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);
                    rowsAffected += batch.ModificationCommands.Count;
                }
                while (batchEnumerator.MoveNext());

                if (beganTransaction)
                {
                    await transaction!.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                if (createdSavepoint && connection.DbConnection.State == ConnectionState.Open)
                {
                    try
                    {
                        await transaction!.RollbackToSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        UpdateLogger.BatchExecutorFailedToRollbackToSavepoint(CurrentContext.GetType(), e);
                    }
                }

                throw;
            }
            finally
            {
                if (beganTransaction)
                {
                    await transaction!.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    if (createdSavepoint)
                    {
                        if (connection.DbConnection.State == ConnectionState.Open)
                        {
                            try
                            {
                                await transaction!.ReleaseSavepointAsync(SavepointName, cancellationToken).ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                UpdateLogger.BatchExecutorFailedToReleaseSavepoint(CurrentContext.GetType(), e);
                            }
                        }
                    }

                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }

            return rowsAffected;
        }
    }
}
