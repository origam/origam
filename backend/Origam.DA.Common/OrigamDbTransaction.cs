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
using System.Data;
using System.Data.SqlClient;

namespace Origam.DA;

/// <summary>
/// Summary description for OrigamDbTransaction.
/// </summary>
public class OrigamDbTransaction : OrigamTransaction
{
    IDbTransaction _transaction;
    IDbConnection _connection;

    public OrigamDbTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
        _connection = transaction.Connection;
    }

    public override void Commit()
    {
        IDbConnection connection = _transaction.Connection;
        _transaction.Commit();
        _transaction.Dispose();
        connection.Close();
        connection.Dispose();
    }

    public override void Rollback()
    {
        _transaction.Rollback();
        _transaction.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    public override void Rollback(string savePointName)
    {
        SqlTransaction tran = _transaction as SqlTransaction;
        if (tran == null)
        {
            throw new NotSupportedException(
                ResourceUtils.GetString("PartialRollbackNotSupported", _transaction.ToString())
            );
        }

        tran.Rollback(savePointName);
    }

    public override void Save(string savePointName)
    {
        SqlTransaction tran = _transaction as SqlTransaction;
        if (tran == null)
        {
            throw new NotSupportedException(
                ResourceUtils.GetString("SavingNotSupported", _transaction.ToString())
            );
        }

        tran.Save(savePointName);
    }

    public IDbTransaction Transaction
    {
        get { return _transaction; }
    }
}
