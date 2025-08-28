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
using System.IO;
using System.Text;
using System.Xml;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Services;
using Directory = System.IO.Directory;

namespace Origam.Workflow;

/// <summary>
/// Summary description for FileSystemServiceAgent.
/// </summary>
public class FileSystemServiceAgent : AbstractServiceAgent
{
    public void LoadBlob(string path)
    {
        _result = File.ReadAllBytes(path);
    }

    public void LoadXml(string path)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        _result = new XmlContainer(xmlDocument);
    }

    public void LoadText(string path, string encodingName)
    {
        _result = File.ReadAllText(path, GetEncoding(encodingName));
    }

    public void SaveXml(string path, XmlDocument outXml, string encodingName, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        Encoding encoding = GetEncoding(encodingName);
        using (XmlWriter xw = new XmlTextWriter(path, encoding))
        {
            outXml.Save(xw);
        }
        _result = null;
    }

    public void SaveText(string path, string output, string encodingName, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        Encoding encoding = GetEncoding(encodingName);
        using (StreamWriter outfile = new StreamWriter(path, false, encoding))
        {
            outfile.Write(output);
        }
        _result = null;
    }

    public void SaveBlob(string path, byte[] blob, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        using (
            FileStream stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read
            )
        )
        {
            stream.Write(blob, 0, blob.Length);
        }
        _result = null;
    }

    public void CopyFile(
        string sourcePath,
        string destinationPath,
        bool createDirectory,
        bool overwrite
    )
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
        }
        File.Copy(sourcePath, destinationPath, overwrite);
        _result = null;
    }

    public void MoveFile(
        string sourcePath,
        string destinationPath,
        bool createDirectory,
        bool overwrite
    )
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
        }
        if (overwrite && File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
        File.Move(sourcePath, destinationPath);
        _result = null;
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
        _result = null;
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
        _result = null;
    }

    public string[] GetFileList(string path, string searchPattern)
    {
        return Directory.GetFiles(path, searchPattern);
    }

    public static Encoding GetEncoding(string encoding)
    {
        if (encoding == null)
        {
            encoding = "utf-8";
        }
        return Encoding.GetEncoding(encoding);
    }

    private IDataDocument GetFileSystemInfo(string path, string mask, bool recursive)
    {
        if (
            this.OutputStructure.PrimaryKey["Id"].ToString()
            != "2aaaed07-bd5e-41d3-858e-288eca4bbcfc"
        )
        {
            throw new Exception(ResourceUtils.GetString("ErrorNotFileSystemInfoDataStructure"));
        }
        DatasetGenerator generator = new DatasetGenerator(true);
        DataSet dataSet = generator.CreateDataSet(this.OutputStructure as DataStructure);
        DataTable table = dataSet.Tables["FileSystemInfo"];
        ProcessFileSystemInfoForFolder(new DirectoryInfo(path), mask, recursive, table, Guid.Empty);
        return DataDocumentFactory.New(dataSet);
    }

    private void ProcessFileSystemInfoForFolder(
        DirectoryInfo parentDirectoryInfo,
        string mask,
        bool recursive,
        DataTable table,
        Guid parentFolderId
    )
    {
        DataRow row;
        var fileInfos = parentDirectoryInfo.EnumerateFiles(mask);
        foreach (FileInfo fileInfo in fileInfos)
        {
            row = table.NewRow();
            row["Id"] = Guid.NewGuid();
            if (parentFolderId != Guid.Empty)
            {
                row["ParentDirectoryId"] = parentFolderId;
            }
            row["IsDirectory"] = false;
            row["Name"] = fileInfo.Name;
            row["FullName"] = fileInfo.FullName;
            row["CreationTime"] = fileInfo.CreationTime;
            row["LastAccessTime"] = fileInfo.LastAccessTime;
            row["LastWriteTime"] = fileInfo.LastWriteTime;
            row["Length"] = fileInfo.Length;
            if (!string.IsNullOrEmpty(fileInfo.Extension))
            {
                row["Extension"] = fileInfo.Extension.Replace(".", "");
            }
            table.Rows.Add(row);
        }
        var directoryInfos = parentDirectoryInfo.EnumerateDirectories();
        foreach (DirectoryInfo directoryInfo in directoryInfos)
        {
            row = table.NewRow();
            Guid directoryId = Guid.NewGuid();
            row["Id"] = directoryId;
            if (parentFolderId != Guid.Empty)
            {
                row["ParentDirectoryId"] = parentFolderId;
            }
            row["IsDirectory"] = true;
            row["Name"] = directoryInfo.Name;
            row["FullName"] = directoryInfo.FullName;
            row["CreationTime"] = directoryInfo.CreationTime;
            row["LastAccessTime"] = directoryInfo.LastAccessTime;
            row["LastWriteTime"] = directoryInfo.LastWriteTime;
            table.Rows.Add(row);
            if (recursive)
            {
                ProcessFileSystemInfoForFolder(directoryInfo, mask, true, table, directoryId);
            }
        }
    }

    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get
        {
            object temp = _result;
            _result = null;

            return temp;
        }
    }

    public override void Run()
    {
        string encoding,
            outPath,
            inPath;
        bool createDirectory = false;
        bool overwrite = false;
        switch (MethodName)
        {
            case "GetFileSystemInfo":
            {
                string mask = "";
                bool recursive = false;
                if (Parameters.Contains("Mask"))
                {
                    mask = Parameters["Mask"] as string;
                }
                if (string.IsNullOrEmpty(mask))
                {
                    mask = "*.*";
                }
                if (!(Parameters["Path"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                if (Parameters.Contains("Recursive"))
                {
                    recursive = (bool)Parameters["Recursive"];
                }
                _result = GetFileSystemInfo(Parameters["Path"] as string, mask, recursive);
                break;
            }

            case "LoadBlob":
            {
                if (!(Parameters["Path"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                LoadBlob((string)Parameters["Path"]);
                break;
            }

            case "LoadXml":
            {
                if (!(Parameters["Path"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                LoadXml((string)Parameters["Path"]);
                break;
            }

            case "LoadText":
            {
                if (!(Parameters["Path"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                encoding = Parameters["Encoding"] as string;
                LoadText((string)Parameters["Path"], encoding);
                break;
            }

            case "SaveXml":
            {
                outPath = Parameters["Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                XmlContainer outXml = Parameters["Data"] as XmlContainer;
                if (outXml == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlContainer"));
                }
                encoding = Parameters["Encoding"] as string;
                if (Parameters.Contains("CreateDirectory"))
                {
                    createDirectory = (bool)Parameters["CreateDirectory"];
                }
                SaveXml(outPath, outXml.Xml, encoding, createDirectory);
                break;
            }

            case "SaveText":
            {
                outPath = Parameters["Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                string output = Parameters["Data"] as string;
                if (output == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNotString"));
                }
                encoding = Parameters["Encoding"] as string;
                if (Parameters.Contains("CreateDirectory"))
                {
                    createDirectory = (bool)Parameters["CreateDirectory"];
                }
                SaveText(outPath, output, encoding, createDirectory);
                break;
            }

            case "SaveBlob":
            {
                outPath = Parameters["Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                byte[] blob = Parameters["Data"] as byte[];
                if (blob == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorNotBlob"));
                }
                if (Parameters.Contains("CreateDirectory"))
                {
                    createDirectory = (bool)Parameters["CreateDirectory"];
                }
                SaveBlob(outPath, blob, createDirectory);
                break;
            }

            case "DeleteFile":
            {
                string deletePath = Parameters["Path"] as string;
                if (deletePath == null)
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                DeleteFile(deletePath);
                break;
            }

            case "CopyFile":
            {
                inPath = Parameters["SourcePath"] as string;
                if (inPath == null)
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorSourcePathNotString")
                    );
                }
                outPath = Parameters["DestinationPath"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorDestinationPathNotString")
                    );
                }
                if (Parameters.Contains("CreateDirectory"))
                {
                    createDirectory = (bool)Parameters["CreateDirectory"];
                }
                if (Parameters.Contains("Overwrite"))
                {
                    overwrite = (bool)Parameters["Overwrite"];
                }
                CopyFile(inPath, outPath, createDirectory, overwrite);
                break;
            }

            case "MoveFile":
            {
                inPath = Parameters["SourcePath"] as string;
                if (inPath == null)
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorSourcePathNotString")
                    );
                }
                outPath = Parameters["DestinationPath"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        ResourceUtils.GetString("ErrorDestinationPathNotString")
                    );
                }
                if (Parameters.Contains("CreateDirectory"))
                {
                    createDirectory = (bool)Parameters["CreateDirectory"];
                }
                if (Parameters.Contains("Overwrite"))
                {
                    overwrite = (bool)Parameters["Overwrite"];
                }
                MoveFile(inPath, outPath, createDirectory, overwrite);
                break;
            }

            case "CreateDirectory":
            {
                if (!(Parameters["Path"] is string))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorPathNotString"));
                }
                CreateDirectory((string)Parameters["Path"]);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    "MethodName",
                    MethodName,
                    ResourceUtils.GetString("InvalidMethodName")
                );
        }
    }
    #endregion
}
