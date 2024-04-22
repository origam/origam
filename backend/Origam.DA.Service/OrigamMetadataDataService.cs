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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Security.Principal;
using ICSharpCode.SharpZipLib.Zip;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service;

/// <summary>
/// Summary description for SerializerDataService.
/// </summary>
public class OrigamMetadataDataService : IDataService
{
	public OrigamMetadataDataService()
	{
		}

	#region IDataService Members
	public virtual string Info
	{
		get
		{
				return "Connection String: " + _connectionString;
			}
	}

	private bool _userDefinedParameters = false;
	public bool UserDefinedParameters
	{
		get
		{
				return _userDefinedParameters;
			}
		set
		{
				_userDefinedParameters = value;
			}
	}

	private string _connectionString = "";
	public string ConnectionString
	{
		get
		{
				return _connectionString;
			}
		set
		{
				_connectionString = value;
			}
	}

	public int BulkInsertThreshold { get; set; }
	public int UpdateBatchSize { get; set; }

	private IStateMachineService _stateMachine;
	public IStateMachineService StateMachine
	{
		get
		{
				return _stateMachine;
			}
		set
		{
				_stateMachine = value;
			}
	}

	private IAttachmentService _attachmentService;
	public IAttachmentService AttachmentService
	{
		get
		{
                return _attachmentService;
            }
		set
		{
                _attachmentService = value;
            }
	}

	public string DbUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	public string DBPassword { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

	public string Xsd(Guid dataStructureId)
	{
			throw new NotImplementedException("Not implemented");
		}

	public DataSet GetEmptyDataSet(Guid dataStructureId)
	{
			return new DataSet();
		}

	public DataSet GetEmptyDataSet(Guid dataStructureId, CultureInfo culture)
	{
			return new DataSet();
		}

	public DataSet LoadDataSet(DataStructureQuery dataStructureQuery, IPrincipal userProfile, DataSet dataset, string transactionId)
	{
			dataset.Merge(LoadDataSet(dataStructureQuery, userProfile, transactionId));
			return dataset;
		}

	public object GetScalarValue(DataStructureQuery query, ColumnsInfo columnsInfo, IPrincipal userProfile, string transactionId)
	{
			throw new NotImplementedException("Not implemented");
		}

	public DataSet LoadDataSet(DataStructureQuery dataStructureQuery, IPrincipal userProfile, string transactionId)
	{
			System.Reflection.Assembly thisExe;
			thisExe = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream file = 
				thisExe.GetManifestResourceStream("Origam.DA.Service.origamMetadata.zip");
			
			ZipFile zipFile = new ZipFile(file);
			
			ZipEntry entry = zipFile.GetEntry("origamMetadata.xml");

			string fileName = entry.Name;
			System.IO.Stream stream = zipFile.GetInputStream(entry);

			System.Data.DataSet dataset = new System.Data.DataSet();
			dataset.ReadXml(stream, XmlReadMode.ReadSchema);

			file.Close();
			stream.Close();

			return dataset;
		}

	public int UpdateData(DataStructureQuery dataStructureQuery, IPrincipal userProfile, DataSet dataset, string transactionid)
	{
			throw new InvalidOperationException("Cannot update embedded metadata.");
		}
	public int UpdateData(
		DataStructureQuery dataStructureQuery, IPrincipal userProfile, 
		DataSet dataset, string transactionid, bool forceBulkInsert)
	{
			throw new InvalidOperationException("Cannot update embedded metadata.");
		}

	public DataSet ExecuteProcedure(string name, string entityOrder, DataStructureQuery query, string transactionid)
	{
			throw new NotSupportedException("ExecuteProcedure is not supported by OrigamMetadataDataService");
		}

	public ArrayList CompareSchema(IPersistenceProvider provider)
	{
			throw new NotSupportedException("CompareSchema is not supported by OrigamMetadataDataService");
		}

	public string ExecuteUpdate(string command, string transactionId)
	{
			throw new NotImplementedException("ExecuteUpdate() is not implemented by this data service");
		}

	public virtual string DatabaseSchemaVersion()
	{
			throw new NotImplementedException("DatabaseSchemaVersion() is not implemented by this data service");
		}

	public virtual void UpdateDatabaseSchemaVersion(string version, string transactionId)
	{
			throw new NotImplementedException("UpdateDatabaseSchemaVersion() is not implemented by this data service");
		}

	public virtual int UpdateField(Guid entityId, Guid fieldId, object oldValue, object newValue, IPrincipal userProfile, string transactionId)
	{
			throw new NotImplementedException("UpdateField() is not implemented by this data service");
		}
	public virtual int ReferenceCount(Guid entityId, Guid fieldId, object value, IPrincipal userProfile, string transactionId)
	{
			throw new NotImplementedException("ReferenceCount() is not implemented by this data service");
		}
	public string BuildConnectionString(string serverName, int port , string databaseName, string userName, string password, bool integratedAuthentication, bool pooling)
	{
            throw new NotImplementedException();
        }
	public void CreateDatabase(string name)
	{
            throw new NotImplementedException();
        }
	public void DeleteDatabase(string name)
	{
            throw new NotImplementedException();
        }
	public string EntityDdl(Guid entity)
	{
            throw new NotImplementedException();
        }
	public string[] FieldDdl(Guid field)
	{
            throw new NotImplementedException();
        }
	public IDataReader ExecuteDataReader(DataStructureQuery query,
		IPrincipal principal, string transactionId)
	{ 
            throw new NotImplementedException();
        }

	public IEnumerable<IEnumerable<object>> ExecuteDataReader(DataStructureQuery dataStructureQuery)
	{
	        throw new NotImplementedException();
        }

	public IEnumerable<Dictionary<string, object>> ExecuteDataReaderReturnPairs(DataStructureQuery query)
	{
	        throw new NotImplementedException();
        }

	#endregion

	#region IDisposable Members

	public void Dispose()
	{
			
		}

	public string[] DatabaseSpecificDatatypes()
	{
            throw new NotImplementedException();
        }

	#endregion

}