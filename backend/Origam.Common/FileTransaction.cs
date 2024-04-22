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

using System.Collections;
using System.IO;

namespace Origam;

/// <summary>
/// Summary description for FileTransaction.
/// </summary>
public class FileDeleteTransaction : OrigamTransaction
{
	Hashtable _files = new Hashtable();

	public FileDeleteTransaction(Hashtable files)
	{
			_files = files;
		}

	public override void Commit()
	{
			foreach(DictionaryEntry entry in _files)
			{
				FileInfo fi = new FileInfo((string)entry.Key);
				FileStream fs = (FileStream)entry.Value;

				fs.Close();
				fi.Delete();
			}
		}

	public override void Rollback()
	{
			foreach(DictionaryEntry entry in _files)
			{
				try
				{
					FileStream fs = (FileStream)entry.Value;

					fs.Close();
				}
				catch{}
			}

			_files.Clear();
		}
}