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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema;
using Origam.Workbench.Commands;

namespace Origam.Workbench.PropertyGrid;

class PropertyGridEx : System.Windows.Forms.PropertyGrid
{
    private static readonly PropertyValueUIItemInvokeHandler UIItemNullHandler = delegate { };
    private static readonly Image UIItemErrorImage = Properties.Resources.Exclamation_8x;
    private static readonly Image UIItemNavigateImage = Properties.Resources.Search_8x;
    private static readonly Image UIItemEditImage = Properties.Resources.Editor_8x;
    public event EventHandler LinkClicked;

    public PropertyGridEx()
    {
        Site = new SimpleSiteImpl();
        PropertyValueServiceImpl svc = new PropertyValueServiceImpl();
        svc.QueryPropertyUIValueItems += VerifyDataErrorInfo;
        ((SimpleSiteImpl)Site).AddService<IPropertyValueUIService>(service: svc);
    }

    void VerifyDataErrorInfo(
        ITypeDescriptorContext context,
        PropertyDescriptor propDesc,
        ArrayList valueUIItemList
    )
    {
        foreach (var item in propDesc.Attributes)
        {
            IModelElementRule rule = item as IModelElementRule;
            if (rule != null)
            {
                Exception ex = rule.CheckRule(
                    instance: context.Instance,
                    memberName: propDesc.Name
                );
                if (ex != null)
                {
                    valueUIItemList.Add(
                        value: new PropertyValueUIItem(
                            uiItemImage: UIItemErrorImage,
                            handler: new PropertyValueUIItemInvokeHandler(UIItemNullHandler),
                            tooltip: ex.Message
                        )
                    );
                }
            }
        }
        var element = propDesc.GetValue(component: context.Instance) as ISchemaItem;
        if (element != null)
        {
            var editHandler = new ModelElementEditHandler();
            editHandler.LinkClicked += (sender, args) =>
                LinkClicked?.Invoke(sender: this, e: EventArgs.Empty);
            valueUIItemList.Add(
                value: new PropertyValueUIItem(
                    uiItemImage: UIItemEditImage,
                    handler: editHandler.Run,
                    tooltip: "Double click to open " + element.Path
                )
            );
        }
    }

    class ModelElementEditHandler
    {
        public event EventHandler LinkClicked;

        public void Run(
            ITypeDescriptorContext context,
            PropertyDescriptor descriptor,
            PropertyValueUIItem invokedItem
        )
        {
            try
            {
                // navigate in model browser
                var schemaBrowser =
                    WorkbenchSingleton.Workbench.GetPad(type: typeof(SchemaBrowser))
                    as SchemaBrowser;
                schemaBrowser.EbrSchemaBrowser.SelectItem(
                    item: descriptor.GetValue(component: context.Instance) as ISchemaItem
                );
                ViewSchemaBrowserPad cmd = new ViewSchemaBrowserPad();
                cmd.Run();
                // edit
                EditSchemaItem cmdEdit = new Commands.EditSchemaItem();
                cmdEdit.Owner = descriptor.GetValue(component: context.Instance);
                cmdEdit.Run();
                LinkClicked?.Invoke(sender: this, e: EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Origam.UI.AsMessageBox.ShowError(
                    owner: null,
                    text: ex.Message,
                    caption: "Error",
                    exception: ex
                );
            }
        }
    }

    internal void SetSplitter()
    {
        var flags =
            BindingFlags.GetField
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.InvokeMethod
            | BindingFlags.Public;
        FieldInfo View = this.GetType().BaseType.GetField(name: "gridView", bindingAttr: flags);
        Control controll = (Control)View.GetValue(obj: this);
        MethodInfo methodInfo = controll
            .GetType()
            .GetMethod(name: "MoveSplitterTo", bindingAttr: flags);
        GridItemCollection gridItemCollection = (GridItemCollection)
            controll
                .GetType()
                .InvokeMember(
                    name: "GetAllGridEntries",
                    invokeAttr: flags,
                    binder: null,
                    target: controll,
                    args: null
                );
        int maxwidth =
            gridItemCollection
                .OfType<GridItem>()
                .OrderByDescending(keySelector: gridItem => gridItem.Label.Width(font: Font))
                .First()
                .Label.Width(font: Font) + 50;
        if (methodInfo != null)
        {
            methodInfo.Invoke(obj: controll, parameters: new object[] { maxwidth });
        }
    }
}
