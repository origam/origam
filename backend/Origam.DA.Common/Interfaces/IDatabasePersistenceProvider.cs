#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using System.Data;

namespace Origam.DA.ObjectPersistence
{
    public interface IDatabasePersistenceProvider
    {
        /// <summary>
        /// Refreshes the internal dataset from the database.
        /// </summary>
        void Refresh(bool append, string transactionId);

        /// <summary>
        /// Updates the data source with changed data.
        /// </summary>
        void Update(string transactionId);

        DataSet EmptyData();

        /// <summary>
        /// Persists internal dataset to file.
        /// </summary>
        /// <param name="fileName"></param>
        void PersistToFile(string fileName, bool sort, List<IDatasetFormater> formaters=null);
    }
    public interface IDatasetFormater
    {
        DataSet Format(DataSet data);	
    }
}
