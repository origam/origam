#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using Origam.Extensions;

namespace Origam;

public class ResourceMonitor
{
    private static ConcurrentDictionary<string, OrderedDictionary> _transactionStore = new();
    private static ConcurrentDictionary<string, List<string>> _savePoints = new();
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public static void RegisterTransaction(
        string transactionId,
        string resourceManagerId,
        OrigamTransaction transaction
    )
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Registering transaction id " + transactionId);
        }
        OrderedDictionary transactions = Transactions(transactionId: transactionId);
        if (transactions.Contains(key: resourceManagerId))
        {
            throw new Exception(message: ResourceUtils.GetString(key: "TransactionRegistered"));
        }
        transactions.Add(key: resourceManagerId, value: transaction);
        List<string> savePoints = SavePoints(transactionId: transactionId);

        foreach (string savePointName in savePoints)
        {
            foreach (OrigamTransaction t in transactions.Values)
            {
                t.Save(savePointName: savePointName);
            }
        }
    }

    public static OrigamTransaction GetTransaction(string transactionId, string resourceManagerId)
    {
        OrderedDictionary transactions = Transactions(transactionId: transactionId);
        if (transactions.Contains(key: resourceManagerId))
        {
            return transactions[key: resourceManagerId] as OrigamTransaction;
        }

        return null;
    }

    public static void Commit(string transactionId)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Committing transaction id " + transactionId);
        }
        try
        {
            OrderedDictionary transactions = Transactions(transactionId: transactionId);
            // commit in reverse order (first transactions last)
            for (int i = transactions.Count - 1; i >= 0; i--)
            {
                OrigamTransaction tran = transactions[index: i] as OrigamTransaction;
                try
                {
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.FatalFormat(
                            format: "Failed committing transaction {0}",
                            arg0: transactionId,
                            arg1: ex
                        );
                    }
                    Rollback(transactionId: transactionId);
                    throw;
                }
            }
        }
        finally
        {
            _transactionStore.TryRemove(key: transactionId, value: out _);
            _savePoints.TryRemove(key: transactionId, value: out _);
        }
    }

    public static void Rollback(string transactionId)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Rolling back transaction id " + transactionId);
        }
        OrderedDictionary transactions = Transactions(transactionId: transactionId);
        string errorMessage = "";
        try
        {
            foreach (DictionaryEntry entry in transactions)
            {
                try
                {
                    (entry.Value as OrigamTransaction).Rollback();
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.FatalFormat(
                            format: "Failed rolling back transaction {0}",
                            arg0: transactionId,
                            arg1: ex
                        );
                    }
                    if (errorMessage != "")
                    {
                        errorMessage += Environment.NewLine;
                    }

                    errorMessage += ex.Message;
                }
            }
        }
        finally
        {
            _transactionStore.TryRemove(key: transactionId, value: out _);
            _savePoints.TryRemove(key: transactionId, value: out _);
            if (errorMessage != "")
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorsDuringRollback",
                        args: Environment.NewLine + Environment.NewLine + errorMessage
                    )
                );
            }
        }
    }

    public static void Rollback(string transactionId, string savePointName)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: "Rolling back transaction id "
                    + transactionId
                    + " to save point "
                    + savePointName
            );
        }
        OrderedDictionary transactions = Transactions(transactionId: transactionId);
        string errorMessage = "";
        try
        {
            foreach (DictionaryEntry entry in transactions)
            {
                OrigamTransaction t = entry.Value as OrigamTransaction;
                try
                {
                    t.Rollback(savePointName: savePointName);
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.LogOrigamError(message: ex.Message, ex: ex);
                    }
                    if (errorMessage != "")
                    {
                        errorMessage += Environment.NewLine;
                    }

                    errorMessage += ex.Message;
                }
            }
        }
        finally
        {
            List<string> savePoints = SavePoints(transactionId: transactionId);
            int min = savePoints.IndexOf(item: savePointName);
            savePoints.RemoveRange(index: min, count: savePoints.Count - min);
            if (errorMessage != "")
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorsDuringPartialRollback",
                        args: Environment.NewLine + Environment.NewLine + errorMessage
                    )
                );
            }
        }
    }

    public static void Save(string transactionId, string savePointName)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: "Saving transaction id " + transactionId + ", save point " + savePointName
            );
        }
        OrderedDictionary transactions = Transactions(transactionId: transactionId);
        SavePoints(transactionId: transactionId).Add(item: savePointName);
        foreach (OrigamTransaction transaction in transactions.Values)
        {
            try
            {
                transaction.Save(savePointName: savePointName);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ExceptionSavingTransaction"),
                    innerException: ex
                );
            }
        }
    }

    #region Private Methods
    private static OrderedDictionary Transactions(string transactionId)
    {
        return _transactionStore.GetOrAdd(
            key: transactionId,
            valueFactory: key => new OrderedDictionary()
        );
    }

    private static List<string> SavePoints(string transactionId)
    {
        return _savePoints.GetOrAdd(key: transactionId, valueFactory: key => new List<string>());
    }
    #endregion
}
