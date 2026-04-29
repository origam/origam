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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Origam.DA.Service.FileSystemModeCheckers;

namespace Origam.UI;

public partial class ModelCheckResultWindow : Form
{
    public ModelCheckResultWindow(List<ModelErrorSection> modelErrorSections)
    {
        InitializeComponent();
        ShowIcon = false;

        errorListBox.DrawMode = DrawMode.OwnerDrawFixed;
        foreach (var section in modelErrorSections)
        {
            errorListBox.Items.Add(item: section);
            foreach (var message in section.ErrorMessages)
            {
                errorListBox.Items.Add(item: message);
            }
            errorListBox.Items.Add(item: ErrorMessage.Empty);
        }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void errorListBox_DrawItem(object sender, DrawItemEventArgs e)
    {
        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
        {
            e = new DrawItemEventArgs(
                graphics: e.Graphics,
                font: e.Font,
                rect: e.Bounds,
                index: e.Index,
                state: DrawItemState.Default,
                foreColor: e.ForeColor,
                backColor: Color.Transparent
            );
        }

        ListBox listBox = sender as ListBox;
        var currentItem = listBox.Items[index: e.Index];
        string text = currentItem.ToString();

        if (
            currentItem is ModelErrorSection section
            || (currentItem is ErrorMessage message && message.Link == null)
        )
        {
            e.DrawBackground();
            e.Graphics.DrawString(
                s: text,
                font: e.Font,
                brush: Brushes.Black,
                layoutRectangle: e.Bounds,
                format: StringFormat.GenericDefault
            );
            return;
        }
        if (currentItem is ErrorMessage errorMessage)
        {
            e.DrawBackground();
            int indexOfLinkStart = errorMessage.Text.IndexOf(value: errorMessage.Link);
            string part1 = new string(
                value: errorMessage.Text.Take(count: indexOfLinkStart).ToArray()
            );
            e.Graphics.DrawString(
                s: part1,
                font: e.Font,
                brush: Brushes.Black,
                layoutRectangle: e.Bounds,
                format: StringFormat.GenericDefault
            );

            var part1Size = e.Graphics.MeasureString(text: part1, font: e.Font);
            var linkBounds = new Rectangle(
                x: e.Bounds.X + (int)part1Size.Width,
                y: e.Bounds.Y,
                width: e.Bounds.Width,
                height: e.Bounds.Height
            );
            e.Graphics.DrawString(
                s: errorMessage.Link,
                font: e.Font,
                brush: Brushes.Blue,
                layoutRectangle: linkBounds,
                format: StringFormat.GenericDefault
            );

            var linkSize = e.Graphics.MeasureString(text: errorMessage.Link, font: e.Font);
            var part2Bounds = new Rectangle(
                x: e.Bounds.X + (int)part1Size.Width + (int)linkSize.Width,
                y: e.Bounds.Y,
                width: e.Bounds.Width,
                height: e.Bounds.Height
            );
            string part2 = new string(
                value: errorMessage
                    .Text.Skip(count: indexOfLinkStart + errorMessage.Link.Length)
                    .ToArray()
            );
            e.Graphics.DrawString(
                s: part2,
                font: e.Font,
                brush: Brushes.Black,
                layoutRectangle: part2Bounds,
                format: StringFormat.GenericDefault
            );
            return;
        }
        throw new NotImplementedException();
    }

    private void errorListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        ListBox listBox = sender as ListBox;
        string link = (listBox?.SelectedItem as ErrorMessage)?.Link;
        if (!string.IsNullOrWhiteSpace(value: link))
        {
            if (File.Exists(path: link))
            {
                Process.Start(fileName: link);
            }
            else if (Directory.Exists(path: link))
            {
                Process.Start(fileName: "explorer.exe", arguments: link);
            }
        }
        listBox.SelectedItem = null;
    }
}
