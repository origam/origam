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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Origam.UI;

public class AsMenuCommand : ToolStripMenuItem, IStatusUpdate, IDisposable
{
    private readonly object caller;

    public AsMenuCommand()
    {
        DefaultImageScaling();
    }

    public AsMenuCommand(string label)
        : base(text: label)
    {
        this.Description = label;
        DefaultImageScaling();
    }

    public AsMenuCommand(string label, object caller)
        : base(text: label)
    {
        this.Description = label;
        this.caller = caller;
        DefaultImageScaling();
    }

    public AsMenuCommand(string label, ICommand menuCommand)
        : base(text: label)
    {
        this.Description = label;
        this.Command = menuCommand;
        DefaultImageScaling();
    }

    public AsMenuCommand(AsMenuCommand other)
        : base(text: other.Description)
    {
        this.Description = other.Description;
        this.caller = other.caller;
        this.Command = other.Command;
        this.SubItems = other.SubItems;
        this.Image = other.Image;
        this.ShortcutKeys = other.ShortcutKeys;
        DefaultImageScaling();
        ShareAllEventHandlers(other: other);
    }

    private void DefaultImageScaling()
    {
        this.ImageScaling = ToolStripItemImageScaling.None;
    }

    // This method causes this AsMenuCommand to share event handlers with
    // the other AsMenuCommand. When any handler is added or removed in this
    // the same happens in the other and vice versa.
    // https://stackoverflow.com/questions/6055038/how-to-clone-control-event-handlers-at-run-time
    private void ShareAllEventHandlers(AsMenuCommand other)
    {
        var eventsField = typeof(Component).GetField(
            name: "events",
            bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance
        );
        var eventHandlerList = eventsField.GetValue(obj: other);
        eventsField.SetValue(obj: this, value: eventHandlerList);
    }

    public List<object> SubItems { get; } = new();
    public ICommand Command { get; set; }
    public string Description { get; } = string.Empty;
    public bool IsEnabled
    {
        get
        {
            bool isEnabled = true;
            if (Command is IMenuCommand command)
            {
                isEnabled &= command.IsEnabled;
            }
            return isEnabled;
        }
        set => this.Enabled = value;
    }

    #region IStatusUpdate Members
    public void UpdateItemsToDisplay()
    {
        this.Text = this.Description;
        if (Command is IMenuCommand command)
        {
            bool isEnabled = IsEnabled & command.IsEnabled;
            if (Enabled != isEnabled)
            {
                Enabled = isEnabled;
            }
        }
        else
        {
            this.DropDownItems.Clear();
            foreach (object item in SubItems)
            {
                PopulateMenu(item: item);
            }
            this.Enabled = (DropDownItems.Count != 0);
        }
    }

    protected override void OnDropDownShow(EventArgs e)
    {
        if (DropDownItems.Count == 1)
        {
            if (DropDownItems[index: 0] is SubmenuBuilderPlaceholder builderPlaceholder)
            {
                this.DropDownItems.RemoveAt(index: 0);
                PopulateBuilder(builder: builderPlaceholder.Builder);
            }
        }
        base.OnDropDownShow(e: e);
    }

    private void PopulateBuilder(ISubmenuBuilder builder)
    {
        ToolStripMenuItem[] submenu = builder.BuildSubmenu(owner: caller);
        foreach (var subItem in submenu)
        {
            PopulateMenu(item: subItem);
        }
    }

    public void PopulateMenu(object item)
    {
        if (item is ISubmenuBuilder submenuBuilder)
        {
            if (submenuBuilder.LateBound)
            {
                if (submenuBuilder.HasItems())
                {
                    DropDownItems.Add(
                        value: new SubmenuBuilderPlaceholder(builder: submenuBuilder)
                    );
                }
            }
            else
            {
                PopulateBuilder(builder: submenuBuilder);
            }
        }
        else
        {
            if (item is IStatusUpdate update)
            {
                update.UpdateItemsToDisplay();
            }
            DropDownItems.Add(value: (ToolStripItem)item);
        }
    }
    #endregion
    #region IDisposable Members
    void IDisposable.Dispose()
    {
        (Command as IDisposable)?.Dispose();
    }
    #endregion
}
