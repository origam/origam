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
                errorListBox.Items.Add(section);
                foreach (var message in section.ErrorMessages)
                {
                    errorListBox.Items.Add(message);
                }
                errorListBox.Items.Add(ErrorMessage.Empty);
            }
        }

    private void okButton_Click(object sender, EventArgs e)
    {
            Close();
        }

    private void errorListBox_DrawItem(object sender, DrawItemEventArgs e)
    {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e = new DrawItemEventArgs(e.Graphics,
                    e.Font,
                    e.Bounds,
                    e.Index,
                    DrawItemState.Default,
                    e.ForeColor,
                    Color.Transparent);
            
            ListBox listBox = sender as ListBox;
            var currentItem = listBox.Items[e.Index];
            string text = currentItem.ToString();
            
            if (currentItem is ModelErrorSection section || 
                currentItem is ErrorMessage message && message.Link == null )
            {
                e.DrawBackground();
                e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
                return;
            }
            if (currentItem is ErrorMessage errorMessage)
            {
                e.DrawBackground();
                int indexOfLinkStart = errorMessage.Text.IndexOf(errorMessage.Link);

                string part1 = new string(errorMessage.Text.Take(indexOfLinkStart).ToArray());
                e.Graphics.DrawString(part1, e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
                
                var part1Size = e.Graphics.MeasureString(part1, e.Font);
                var linkBounds = new Rectangle(
                    e.Bounds.X + (int)part1Size.Width, 
                    e.Bounds.Y,
                    e.Bounds.Width,
                    e.Bounds.Height);
                e.Graphics.DrawString(errorMessage.Link, e.Font, Brushes.Blue, linkBounds, StringFormat.GenericDefault);
                
                var linkSize = e.Graphics.MeasureString(errorMessage.Link, e.Font);
                var part2Bounds = new Rectangle(
                    e.Bounds.X + (int)part1Size.Width + (int)linkSize.Width,
                    e.Bounds.Y,
                    e.Bounds.Width,
                    e.Bounds.Height);
                string part2 = new string(errorMessage.Text
                    .Skip(indexOfLinkStart + errorMessage.Link.Length)
                    .ToArray()
                );
                e.Graphics.DrawString(part2, e.Font, Brushes.Black, part2Bounds, StringFormat.GenericDefault);
                return;
            }

            throw new NotImplementedException();
        }

    private void errorListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
            ListBox listBox = sender as ListBox;
            string link = (listBox?.SelectedItem as ErrorMessage)?.Link;
            if (!string.IsNullOrWhiteSpace(link))
            {
                if (File.Exists(link))
                {
                    Process.Start(link);
                }
                else if (Directory.Exists(link))
                {
                    Process.Start("explorer.exe", link);
                }
            }
            listBox.SelectedItem = null;
        }
}