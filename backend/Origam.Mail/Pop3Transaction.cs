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
using MailKit.Net.Pop3;


namespace Origam.Mail
{
	/// <summary>
	/// Summary description for Pop3Transaction.
	/// </summary>
	public class Pop3Transaction : OrigamTransaction
	{
        public Pop3Transaction(Pop3Client client)
        {
            PopClient = client;
        }

        public override void Commit()
        {
            CheckStatus();
            PopClient.Disconnect(true);
            PopClient.Dispose();
        }

        public override void Rollback()
        {
            CheckStatus();

            PopClient.Reset();
            PopClient.Disconnect(true);
            PopClient.Dispose();
        }

        public Pop3Client PopClient { get; } = null;

        private void CheckStatus()
        {
            if (PopClient == null)
            {
                throw new InvalidOperationException(ResourceUtils.GetString("ErrorTransactionNotStarted"));
            }

            if (!PopClient.IsConnected)
            {
                throw new InvalidOperationException(ResourceUtils.GetString("ErrorNotConnected"));
            }
        }
    }
}
