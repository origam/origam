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
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace Origam.Schema.DeploymentModel;

/// <summary>
/// Summary description for FileSelectionUIEditor.
/// </summary>
public class FileSelectionUITypeEditor : UITypeEditor
{
    private OpenFileDialog _dialog = new OpenFileDialog();

    public FileSelectionUITypeEditor() { }

    public override object EditValue(
        ITypeDescriptorContext context,
        IServiceProvider provider,
        object value
    )
    {
        _dialog.Filter = "All files (*.*)|*.*";
        _dialog.Title = ResourceUtils.GetString("LoadFile");

        if (_dialog.ShowDialog() == DialogResult.OK)
        {
            Crc32 crc = new Crc32();
            MemoryStream stream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(stream);
            zipStream.SetLevel(9);
            BinaryReader br = new BinaryReader(stream);
            byte[] byteArray;
            try
            {
                FileStream fs = File.OpenRead(_dialog.FileName);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                ZipEntry entry = new ZipEntry(@"file");
                entry.DateTime = File.GetCreationTimeUtc(_dialog.FileName);
                entry.Comment = _dialog.FileName;
                entry.ZipFileIndex = 1;
                entry.Size = fs.Length;
                fs.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                zipStream.PutNextEntry(entry);
                zipStream.Write(buffer, 0, buffer.Length);
                zipStream.Finish();
                if (stream.Length > 50000000)
                    throw new Exception(ResourceUtils.GetString("ErrorFileBig"));
                stream.Position = 0;
                byteArray = br.ReadBytes((int)stream.Length);
            }
            finally
            {
                zipStream.Close();
                br.Close();
                stream.Close();
                stream = null;
                br = null;
                zipStream = null;
            }
            value = byteArray;
            FileRestoreUpdateScriptActivity activity =
                context.Instance as FileRestoreUpdateScriptActivity;

            if (activity.TargetLocation == DeploymentFileLocation.Manual)
            {
                activity.FileName = _dialog.FileName;
            }
            else
            {
                activity.FileName = Path.GetFileName(_dialog.FileName);
            }
        }
        return value;
    }

    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
    {
        return false;
    }

    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
        return UITypeEditorEditStyle.Modal;
    }
}
