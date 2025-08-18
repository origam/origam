#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Origam.UI.Commands;
using Origam.UI.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.UI.WizardForm;
public class AbstractWizardForm : IWizardForm
{
    public List<ListViewItem> ItemTypeList { get ; set ; }
    public  Stack Pages { get; set; }
    public  string Description { get ; set ; }
    public  List<string> StructureList { get ; set ; }
    public  string NameOfEntity { get ; set ; }
    public ImageList ImageList { get; set ; }
    public IRunCommand Command { get; set; }
    public string Title { get; set; }
    public string PageTitle { get; set; }
    public bool IsExistsNameInDataStructure(string name)
    {
        return StructureList.Contains(name);
    }
    public void ListView(ListView listView)
    {
        if (listView.Items.Count == 0)
        {
            foreach (ListViewItem item in ItemTypeList)
            {
                item.ImageIndex = ImageList.ImageIndex(item.ImageKey);
                listView.Items.Add(item);
            }
        }
    }
}
public enum PagesList
{
    StartPage,
    StructureNamePage,
    ScreenForm,
    LookupForm,
    FieldLookup,
    FieldEntity,
    MenuPage,
    ChildEntity,
    ForeignForm,
    SummaryPage,
    Finish
}
