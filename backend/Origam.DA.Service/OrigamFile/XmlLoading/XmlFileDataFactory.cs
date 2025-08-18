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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;

namespace Origam.DA.Service;
public class XmlFileDataFactory
{
    public Result<XmlFileData, XmlLoadError> Create(FileInfo fileInfo)
    {
        Result<OrigamXmlDocument> documentResult = LoadXmlDoc(fileInfo);
        if (documentResult.IsFailure)
        {
            return Result.Failure<XmlFileData, XmlLoadError>(
                new XmlLoadError(documentResult.Error));
        }
        return Result.Success<XmlFileData, XmlLoadError>(new XmlFileData(documentResult.Value, fileInfo));
    }
    private Result<OrigamXmlDocument> LoadXmlDoc(FileInfo fileInfo)
    {
        OrigamXmlDocument xmlDocument = new OrigamXmlDocument();
        try
        {
            xmlDocument.Load(fileInfo.FullName);
        } catch (XmlException ex)
        {
            return Result.Failure<OrigamXmlDocument>(
                $"Could not read file: {fileInfo.FullName}{Environment.NewLine}{ex.Message}");
        }
        return Result.Success(xmlDocument);
    }
}
