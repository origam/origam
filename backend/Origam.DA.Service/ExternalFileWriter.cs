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
using System.IO;
using System.Text;
using static Origam.DA.ObjectPersistence.ExternalFileExtension;

namespace Origam.DA.Service;

internal abstract class ExternalFileWriter
{
    public static ExternalFileWriter GetNew(ExternalFilePath filePath)
    {
            switch (filePath.Extension)
            {
                case Txt:
                case Xml:
                case Xslt:
                    return new TextFileWriter(filePath);
                case Bin:
                case Png:
                    return new BinaryFileWriter(filePath);
                default:    
                    throw new NotImplementedException();
            }
        }
        
    private readonly ExternalFilePath externalFilePath;

    protected ExternalFileWriter(ExternalFilePath externalFilePath)
    {
            this.externalFilePath = externalFilePath;
        }

    public void Write(object data)
    {
            if (!externalFilePath.Directory.Exists)
            {
                externalFilePath.Directory.Create();
            }
            WriteData(externalFilePath.Absolute, data);
        }

    public object Read()
    {
            return ReadData(externalFilePath.Absolute);
        }

    protected abstract void WriteData(string path,object dataToWrite);
    protected abstract object ReadData(string path);
}

internal class TextFileWriter: ExternalFileWriter 
{
    public TextFileWriter(ExternalFilePath externalFilePath) : base(externalFilePath)
    {
        }

    protected override void WriteData(string path, object text)
    {
            if (! (text is string))
            {
                throw new ArgumentOutOfRangeException("text must be a string.");
            }
            File.WriteAllText(path, text as string, Encoding.UTF8);
        }

    protected override object ReadData(string path)
    {
            return File.ReadAllText(path,Encoding.UTF8);
        }
}

internal class BinaryFileWriter: ExternalFileWriter
{
    public BinaryFileWriter(ExternalFilePath filePath) : base(filePath)
    {
        }

    protected override void WriteData(string path,object dataToWrite)
    {
            File.WriteAllBytes(path, (byte[])dataToWrite);
        }

    protected override object ReadData(string path)
    {
            return File.ReadAllBytes(path);
        }
}