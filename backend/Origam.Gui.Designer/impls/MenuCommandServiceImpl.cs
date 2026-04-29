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
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Origam.Gui.Designer;

/// IMenuCommandService implementation.
public class MenuCommandServiceImpl : System.ComponentModel.Design.IMenuCommandService, IDisposable
{
    // the host
    private IDesignerHost host;

    // commandId-command mapping
    private IDictionary commands;

    // menuItem-verb mapping
    private IDictionary menuItemVerb;

    // the global verbs collection.
    private DesignerVerbCollection globalVerbs;

    // we use the same context menu over-and-over
    private ContextMenu contextMenu;

    // we keep the lastSelectedComponent around
    private IComponent lastSelectedComponent;

    public MenuCommandServiceImpl(IDesignerHost host)
    {
        this.host = host;
        commands = new Hashtable();
        globalVerbs = new DesignerVerbCollection();
        menuItemVerb = new Hashtable();
        contextMenu = new ContextMenu();
        lastSelectedComponent = null;
    }

    #region Implementation of IMenuCommandService
    /// called to add a MenuCommand
    public void AddCommand(System.ComponentModel.Design.MenuCommand command)
    {
        if (command == null)
        {
            throw new ArgumentException(message: "command");
        }
        // don't add commands twice
        if (FindCommand(commandID: command.CommandID) == null)
        {
            commands.Add(key: command.CommandID, value: command);
        }
    }

    /// called to remove a MenuCommand
    public void RemoveCommand(System.ComponentModel.Design.MenuCommand command)
    {
        if (command == null)
        {
            throw new ArgumentException(message: "command");
        }
        commands.Remove(key: command.CommandID);
    }

    /// called when to add a global verb
    public void AddVerb(System.ComponentModel.Design.DesignerVerb verb)
    {
        if (verb == null)
        {
            throw new ArgumentException(message: "verb");
        }
        globalVerbs.Add(value: verb);
        // create a menu item for the verb and add it to the context menu
        MenuItem menuItem = new MenuItem(text: verb.Text);
        menuItem.Click += new EventHandler(MenuItemClickHandler);
        menuItemVerb.Add(key: menuItem, value: verb);
        contextMenu.MenuItems.Add(item: menuItem);
    }

    /// called to remove global verb
    public void RemoveVerb(System.ComponentModel.Design.DesignerVerb verb)
    {
        if (verb == null)
        {
            throw new ArgumentException(message: "verb");
        }

        globalVerbs.Remove(value: verb);
        // find the menu item associated with the verb
        MenuItem associatedMenuItem = null;
        foreach (DictionaryEntry de in menuItemVerb)
        {
            if (de.Value == verb)
            {
                associatedMenuItem = de.Key as MenuItem;
                break;
            }
        }
        // if we found the verb's menu item, remove it
        if (associatedMenuItem != null)
        {
            menuItemVerb.Remove(key: associatedMenuItem);
        }
        // remove the verb from the context menu too
        contextMenu.MenuItems.Remove(item: associatedMenuItem);
    }

    /// returns the MenuCommand associated with the commandId.
    public System.ComponentModel.Design.MenuCommand FindCommand(
        System.ComponentModel.Design.CommandID commandID
    )
    {
        return commands[key: commandID] as MenuCommand;
    }

    /// called to invoke a command
    public bool GlobalInvoke(System.ComponentModel.Design.CommandID commandID)
    {
        bool result = false;
        MenuCommand command = FindCommand(commandID: commandID);
        if (command != null)
        {
            command.Invoke();
            result = true;
        }
        return result;
    }

    /// called to show the context menu for the selected component.
    public void ShowContextMenu(System.ComponentModel.Design.CommandID menuID, int x, int y)
    {
        ISelectionService selectionService =
            host.GetService(serviceType: typeof(ISelectionService)) as ISelectionService;
        // get the primary component
        IComponent primarySelection = selectionService.PrimarySelection as IComponent;
        // if the he clicked on the same component again then just show the context
        // menu. otherwise, we have to throw away the previous
        // set of local menu items and create new ones for the newly
        // selected component
        if (lastSelectedComponent != primarySelection)
        {
            // remove all non-global menu items from the context menu
            ResetContextMenu();
            // get the designer
            IDesigner designer = host.GetDesigner(component: primarySelection);
            // not all controls need a desinger
            if (designer != null)
            {
                // get designer's verbs
                DesignerVerbCollection verbs = this.Verbs; //designer.Verbs;
                foreach (DesignerVerb verb in verbs)
                {
                    // add new menu items to the context menu
                    CreateAndAddLocalVerb(verb: verb);
                }
            }
        }
        // we only show designer context menus for controls
        if (primarySelection is Control)
        {
            Control comp = primarySelection as Control;
            Point pt = comp.PointToScreen(p: new Point(x: 0, y: 0));
            contextMenu.Show(control: comp, pos: new Point(x: x - pt.X, y: y - pt.Y));
        }
        // keep the selected component for next time
        lastSelectedComponent = primarySelection;
    }

    /// returns the the current designer verbs
    public System.ComponentModel.Design.DesignerVerbCollection Verbs
    {
        get
        {
            // create a new collection
            DesignerVerbCollection availableVerbs = new DesignerVerbCollection();
            // add the global verbs
            if (globalVerbs != null && globalVerbs.Count > 0)
            {
                availableVerbs.AddRange(value: globalVerbs);
            }
            // now add the local verbs
            ISelectionService selectionService =
                host.GetService(serviceType: typeof(ISelectionService)) as ISelectionService;
            IComponent primaryComponent = selectionService.PrimarySelection as IComponent;
            if (primaryComponent != null)
            {
                IDesigner designer = host.GetDesigner(component: primaryComponent);
                if (designer != null && designer.Verbs != null && designer.Verbs.Count > 0)
                {
                    availableVerbs.AddRange(value: designer.Verbs);
                }
            }
            return availableVerbs;
        }
    }
    #endregion
    /// called to invoke menu item verbs
    private void MenuItemClickHandler(object sender, EventArgs e)
    {
        // get the menu item
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null)
        {
            // get and invoke the verb
            DesignerVerb verb = menuItemVerb[key: menuItem] as DesignerVerb;
            if (verb != null)
            {
                try
                {
                    verb.Invoke();
                }
                catch { }
            }
        }
    }

    /// removes all local verbs from the context menu
    private void ResetContextMenu()
    {
        if (contextMenu != null && contextMenu.MenuItems != null && contextMenu.MenuItems.Count > 0)
        {
            MenuItem[] menuItemArray = new MenuItem[contextMenu.MenuItems.Count];
            contextMenu.MenuItems.CopyTo(dest: menuItemArray, index: 0);
            foreach (MenuItem menuItem in menuItemArray)
            {
                // if its not in the global list, remove it
                if (!IsInGlobalList(verbText: menuItem.Text))
                {
                    contextMenu.MenuItems.Remove(item: menuItem);
                }
                // get rid of the menu item from the mapping
                menuItemVerb.Remove(key: menuItem);
            }
        }
    }

    /// removes a local verb
    private void RemoveLocalVerb(DesignerVerb verb)
    {
        if (verb == null)
        {
            throw new ArgumentException(message: "verb");
        }
        // get the associated menuItem
        MenuItem menuItem = GetMenuItemForVerb(verb: verb);
        if (menuItem != null)
        {
            // undo mapping
            menuItemVerb.Remove(key: menuItem);
            // remove from context menu
            contextMenu.MenuItems.Remove(item: menuItem);
        }
    }

    /// creats and adds a local verb
    private void CreateAndAddLocalVerb(DesignerVerb verb)
    {
        if (verb == null)
        {
            throw new ArgumentException(message: "verb");
        }
        VerifyVerb(verb: verb);
        // create a menu item for the verb
        MenuItem menuItem = new MenuItem(text: verb.Text);
        // attach the menu item click listener
        menuItem.Click += new EventHandler(MenuItemClickHandler);
        // do the menuItem-verb mapping
        menuItemVerb.Add(key: menuItem, value: verb);
        // add to context menu
        contextMenu.MenuItems.Add(item: menuItem);
    }

    /// returns the MenuItem associated with the verb
    private MenuItem GetMenuItemForVerb(DesignerVerb verb)
    {
        MenuItem menuItem = null;
        if (menuItemVerb != null && menuItemVerb.Count > 0)
        {
            foreach (DictionaryEntry de in menuItemVerb)
            {
                DesignerVerb dv = de.Value as DesignerVerb;
                if (dv == verb)
                {
                    menuItem = de.Key as MenuItem;
                    break;
                }
            }
        }
        return menuItem;
    }

    /// returns true if the verb is in the global verb collection
    private bool IsInGlobalList(string verbText)
    {
        bool found = false;
        if (globalVerbs != null && globalVerbs.Count > 0)
        {
            foreach (DesignerVerb dv in globalVerbs)
            {
                if (string.Compare(strA: dv.Text, strB: verbText, ignoreCase: true) == 0)
                {
                    found = true;
                    break;
                }
            }
        }
        return found;
    }

    /// we can't add the same verb twice
    private void VerifyVerb(DesignerVerb verb)
    {
        if (verb == null)
        {
            throw new ArgumentException(message: "verb");
        }
        // make sure the verb is not in the global list
        if (globalVerbs != null && globalVerbs.Count > 0)
        {
            foreach (DesignerVerb dv in globalVerbs)
            {
                if (string.Compare(strA: dv.Text, strB: verb.Text, ignoreCase: true) == 0)
                {
                    throw new Exception(message: "Cannot add the same verb twice.");
                }
            }
        }
        // now check the menuItemVerb mapping
        if (menuItemVerb != null && menuItemVerb.Count > 0)
        {
            foreach (DesignerVerb dv in menuItemVerb.Values)
            {
                if (string.Compare(strA: dv.Text, strB: verb.Text, ignoreCase: true) == 0)
                {
                    throw new Exception(message: "Cannot add the same verb twice.");
                }
            }
        }
    }

    #region IDisposable Members
    public void Dispose()
    {
        contextMenu.Dispose();
        commands.Clear();
        host = null;
        lastSelectedComponent = null;
    }
    #endregion
}
