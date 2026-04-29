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
using Directory = System.IO.Directory;

namespace Origam.Workflow;

/// <summary>
/// Summary description for FileSystemServiceAgent.
/// </summary>
public class FileSystemServiceAgent : AbstractServiceAgent
{
    public void LoadBlob(string path)
    {
        _result = File.ReadAllBytes(path: path);
    }

    public void LoadXml(string path)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(filename: path);
        _result = new XmlContainer(xmlDocument: xmlDocument);
    }

    public void LoadText(string path, string encodingName)
    {
        _result = File.ReadAllText(path: path, encoding: GetEncoding(encoding: encodingName));
    }

    public void SaveXml(string path, XmlDocument outXml, string encodingName, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(path: Path.GetDirectoryName(path: path));
        }
        Encoding encoding = GetEncoding(encoding: encodingName);
        using (XmlWriter xw = new XmlTextWriter(filename: path, encoding: encoding))
        {
            outXml.Save(w: xw);
        }
        _result = null;
    }

    public void SaveText(string path, string output, string encodingName, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(path: Path.GetDirectoryName(path: path));
        }
        Encoding encoding = GetEncoding(encoding: encodingName);
        using (
            StreamWriter outfile = new StreamWriter(path: path, append: false, encoding: encoding)
        )
        {
            outfile.Write(value: output);
        }
        _result = null;
    }

    public void SaveBlob(string path, byte[] blob, bool createDirectory)
    {
        if (createDirectory)
        {
            Directory.CreateDirectory(path: Path.GetDirectoryName(path: path));
        }
        using (
            FileStream stream = new FileStream(
                path: path,
                mode: FileMode.Create,
                access: FileAccess.Write,
                share: FileShare.Read
            )
        )
        {
            stream.Write(array: blob, offset: 0, count: blob.Length);
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
            Directory.CreateDirectory(path: Path.GetDirectoryName(path: destinationPath));
        }
        File.Copy(sourceFileName: sourcePath, destFileName: destinationPath, overwrite: overwrite);
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
            Directory.CreateDirectory(path: Path.GetDirectoryName(path: destinationPath));
        }
        if (overwrite && File.Exists(path: destinationPath))
        {
            File.Delete(path: destinationPath);
        }
        File.Move(sourceFileName: sourcePath, destFileName: destinationPath);
        _result = null;
    }

    public void DeleteFile(string path)
    {
        File.Delete(path: path);
        _result = null;
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path: path);
        _result = null;
    }

    public string[] GetFileList(string path, string searchPattern)
    {
        return Directory.GetFiles(path: path, searchPattern: searchPattern);
    }

    public static Encoding GetEncoding(string encoding)
    {
        if (encoding == null)
        {
            encoding = "utf-8";
        }
        return Encoding.GetEncoding(name: encoding);
    }

    private IDataDocument GetFileSystemInfo(string path, string mask, bool recursive)
    {
        if (
            this.OutputStructure.PrimaryKey[key: "Id"].ToString()
            != "2aaaed07-bd5e-41d3-858e-288eca4bbcfc"
        )
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorNotFileSystemInfoDataStructure")
            );
        }
        DatasetGenerator generator = new DatasetGenerator(userDefinedParameters: true);
        DataSet dataSet = generator.CreateDataSet(ds: this.OutputStructure as DataStructure);
        DataTable table = dataSet.Tables[name: "FileSystemInfo"];
        ProcessFileSystemInfoForFolder(
            parentDirectoryInfo: new DirectoryInfo(path: path),
            mask: mask,
            recursive: recursive,
            table: table,
            parentFolderId: Guid.Empty
        );
        return DataDocumentFactory.New(dataSet: dataSet);
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
        var fileInfos = parentDirectoryInfo.EnumerateFiles(searchPattern: mask);
        foreach (FileInfo fileInfo in fileInfos)
        {
            row = table.NewRow();
            row[columnName: "Id"] = Guid.NewGuid();
            if (parentFolderId != Guid.Empty)
            {
                row[columnName: "ParentDirectoryId"] = parentFolderId;
            }
            row[columnName: "IsDirectory"] = false;
            row[columnName: "Name"] = fileInfo.Name;
            row[columnName: "FullName"] = fileInfo.FullName;
            row[columnName: "CreationTime"] = fileInfo.CreationTime;
            row[columnName: "LastAccessTime"] = fileInfo.LastAccessTime;
            row[columnName: "LastWriteTime"] = fileInfo.LastWriteTime;
            row[columnName: "Length"] = fileInfo.Length;
            if (!string.IsNullOrEmpty(value: fileInfo.Extension))
            {
                row[columnName: "Extension"] = fileInfo.Extension.Replace(
                    oldValue: ".",
                    newValue: ""
                );
            }
            table.Rows.Add(row: row);
        }
        var directoryInfos = parentDirectoryInfo.EnumerateDirectories();
        foreach (DirectoryInfo directoryInfo in directoryInfos)
        {
            row = table.NewRow();
            Guid directoryId = Guid.NewGuid();
            row[columnName: "Id"] = directoryId;
            if (parentFolderId != Guid.Empty)
            {
                row[columnName: "ParentDirectoryId"] = parentFolderId;
            }
            row[columnName: "IsDirectory"] = true;
            row[columnName: "Name"] = directoryInfo.Name;
            row[columnName: "FullName"] = directoryInfo.FullName;
            row[columnName: "CreationTime"] = directoryInfo.CreationTime;
            row[columnName: "LastAccessTime"] = directoryInfo.LastAccessTime;
            row[columnName: "LastWriteTime"] = directoryInfo.LastWriteTime;
            table.Rows.Add(row: row);
            if (recursive)
            {
                ProcessFileSystemInfoForFolder(
                    parentDirectoryInfo: directoryInfo,
                    mask: mask,
                    recursive: true,
                    table: table,
                    parentFolderId: directoryId
                );
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
                if (Parameters.Contains(key: "Mask"))
                {
                    mask = Parameters[key: "Mask"] as string;
                }
                if (string.IsNullOrEmpty(value: mask))
                {
                    mask = "*.*";
                }
                if (!(Parameters[key: "Path"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                if (Parameters.Contains(key: "Recursive"))
                {
                    recursive = (bool)Parameters[key: "Recursive"];
                }
                _result = GetFileSystemInfo(
                    path: Parameters[key: "Path"] as string,
                    mask: mask,
                    recursive: recursive
                );
                break;
            }

            case "LoadBlob":
            {
                if (!(Parameters[key: "Path"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                LoadBlob(path: (string)Parameters[key: "Path"]);
                break;
            }

            case "LoadXml":
            {
                if (!(Parameters[key: "Path"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                LoadXml(path: (string)Parameters[key: "Path"]);
                break;
            }

            case "LoadText":
            {
                if (!(Parameters[key: "Path"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                encoding = Parameters[key: "Encoding"] as string;
                LoadText(path: (string)Parameters[key: "Path"], encodingName: encoding);
                break;
            }

            case "SaveXml":
            {
                outPath = Parameters[key: "Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                XmlContainer outXml = Parameters[key: "Data"] as XmlContainer;
                if (outXml == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotXmlContainer")
                    );
                }
                encoding = Parameters[key: "Encoding"] as string;
                if (Parameters.Contains(key: "CreateDirectory"))
                {
                    createDirectory = (bool)Parameters[key: "CreateDirectory"];
                }
                SaveXml(
                    path: outPath,
                    outXml: outXml.Xml,
                    encodingName: encoding,
                    createDirectory: createDirectory
                );
                break;
            }

            case "SaveText":
            {
                outPath = Parameters[key: "Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                string output = Parameters[key: "Data"] as string;
                if (output == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotString")
                    );
                }
                encoding = Parameters[key: "Encoding"] as string;
                if (Parameters.Contains(key: "CreateDirectory"))
                {
                    createDirectory = (bool)Parameters[key: "CreateDirectory"];
                }
                SaveText(
                    path: outPath,
                    output: output,
                    encodingName: encoding,
                    createDirectory: createDirectory
                );
                break;
            }
            case "SaveBlob":
            {
                outPath = Parameters[key: "Path"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                byte[] blob = Parameters[key: "Data"] as byte[];
                if (blob == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorNotBlob")
                    );
                }
                if (Parameters.Contains(key: "CreateDirectory"))
                {
                    createDirectory = (bool)Parameters[key: "CreateDirectory"];
                }
                SaveBlob(path: outPath, blob: blob, createDirectory: createDirectory);
                break;
            }
            case "DeleteFile":
            {
                string deletePath = Parameters[key: "Path"] as string;
                if (deletePath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                DeleteFile(path: deletePath);
                break;
            }
            case "CopyFile":
            {
                inPath = Parameters[key: "SourcePath"] as string;
                if (inPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorSourcePathNotString")
                    );
                }
                outPath = Parameters[key: "DestinationPath"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorDestinationPathNotString")
                    );
                }
                if (Parameters.Contains(key: "CreateDirectory"))
                {
                    createDirectory = (bool)Parameters[key: "CreateDirectory"];
                }
                if (Parameters.Contains(key: "Overwrite"))
                {
                    overwrite = (bool)Parameters[key: "Overwrite"];
                }
                CopyFile(
                    sourcePath: inPath,
                    destinationPath: outPath,
                    createDirectory: createDirectory,
                    overwrite: overwrite
                );
                break;
            }
            case "MoveFile":
            {
                inPath = Parameters[key: "SourcePath"] as string;
                if (inPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorSourcePathNotString")
                    );
                }
                outPath = Parameters[key: "DestinationPath"] as string;
                if (outPath == null)
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorDestinationPathNotString")
                    );
                }
                if (Parameters.Contains(key: "CreateDirectory"))
                {
                    createDirectory = (bool)Parameters[key: "CreateDirectory"];
                }
                if (Parameters.Contains(key: "Overwrite"))
                {
                    overwrite = (bool)Parameters[key: "Overwrite"];
                }
                MoveFile(
                    sourcePath: inPath,
                    destinationPath: outPath,
                    createDirectory: createDirectory,
                    overwrite: overwrite
                );
                break;
            }

            case "CreateDirectory":
            {
                if (!(Parameters[key: "Path"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorPathNotString")
                    );
                }
                CreateDirectory(path: (string)Parameters[key: "Path"]);
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }
    #endregion
}
