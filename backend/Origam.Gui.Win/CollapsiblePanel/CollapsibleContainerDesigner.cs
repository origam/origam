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
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Origam.Gui.Win
{
    internal class CollapsibleContainerDesigner : ParentControlDesigner
    {
        // Fields
        private bool disableDrawGrid = false;
        private int persistedSelectedIndex = 0;
        private DesignerVerb removeVerb;
        private DesignerVerbCollection verbs;
        private ISelectionService m_SelectionService;

        // Methods
        public override bool CanParent(Control control)
        {
            return (control is TabPage);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ISelectionService service = (ISelectionService)
                    this.GetService(serviceType: typeof(ISelectionService));
                if (service != null)
                {
                    service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                IComponentChangeService service2 = (IComponentChangeService)
                    this.GetService(serviceType: typeof(IComponentChangeService));
                if (service2 != null)
                {
                    service2.ComponentChanging -= new ComponentChangingEventHandler(
                        this.OnComponentChanging
                    );
                    service2.ComponentChanged -= new ComponentChangedEventHandler(
                        this.OnComponentChanged
                    );
                }
            }
            base.Dispose(disposing: disposing);
        }

        private enum TabControlHitTest
        {
            TCHT_NOWHERE = 1,
            TCHT_ONITEMICON = 2,
            TCHT_ONITEMLABEL = 4,
            TCHT_ONITEM = TCHT_ONITEMICON | TCHT_ONITEMLABEL,
        }

        private const int TCM_HITTEST = 0x130D;

        private struct TCHITTESTINFO
        {
            public System.Drawing.Point pt;
            public TabControlHitTest flags;
        }

        public ISelectionService SelectionService
        {
            get
            {
                if (m_SelectionService == null)
                {
                    m_SelectionService = (ISelectionService)(
                        this.GetService(serviceType: typeof(ISelectionService))
                    );
                }

                return m_SelectionService;
            }
        }

        protected override bool GetHitTest(System.Drawing.Point point)
        {
            if (this.SelectionService.PrimarySelection == this.Control)
            {
                TCHITTESTINFO hti = new TCHITTESTINFO();

                hti.pt = this.Control.PointToClient(p: point);
                hti.flags = 0;

                System.Windows.Forms.Message m = new System.Windows.Forms.Message();
                m.HWnd = this.Control.Handle;
                m.Msg = TCM_HITTEST;

                IntPtr lparam = System.Runtime.InteropServices.Marshal.AllocHGlobal(
                    cb: System.Runtime.InteropServices.Marshal.SizeOf(structure: hti)
                );
                System.Runtime.InteropServices.Marshal.StructureToPtr(
                    structure: hti,
                    ptr: lparam,
                    fDeleteOld: false
                );
                m.LParam = lparam;

                base.WndProc(m: ref m);
                System.Runtime.InteropServices.Marshal.FreeHGlobal(hglobal: lparam);

                if (m.Result.ToInt32() != -1)
                {
                    return hti.flags != TabControlHitTest.TCHT_NOWHERE;
                }
            }

            return false;
        }

        internal static TabPage GetTabPageOfComponent(object comp)
        {
            if (!(comp is Control))
            {
                return null;
            }
            Control parent = (Control)comp;
            while ((parent != null) && !(parent is TabPage))
            {
                parent = parent.Parent;
            }
            return (TabPage)parent;
        }

        private void CheckVerbStatus()
        {
            if (this.removeVerb != null)
            {
                this.removeVerb.Enabled = this.Control.Controls.Count > 0;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component: component);
            ISelectionService service = (ISelectionService)
                this.GetService(serviceType: typeof(ISelectionService));
            if (service != null)
            {
                service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            IComponentChangeService service2 = (IComponentChangeService)
                this.GetService(serviceType: typeof(IComponentChangeService));
            if (service2 != null)
            {
                service2.ComponentChanging += new ComponentChangingEventHandler(
                    this.OnComponentChanging
                );
                service2.ComponentChanged += new ComponentChangedEventHandler(
                    this.OnComponentChanged
                );
            }
            ((TabControl)component).SelectedIndexChanged += new EventHandler(
                this.OnTabSelectedIndexChanged
            );
            ((TabControl)component).GotFocus += new EventHandler(this.OnGotFocus);
        }

        private void OnAdd(object sender, EventArgs eevent)
        {
            TabControl component = (TabControl)base.Component;
            MemberDescriptor member = TypeDescriptor.GetProperties(component: base.Component)[
                name: "Controls"
            ];
            IDesignerHost service = (IDesignerHost)
                this.GetService(serviceType: typeof(IDesignerHost));
            if (service != null)
            {
                DesignerTransaction transaction = null;
                try
                {
                    try
                    {
                        transaction = service.CreateTransaction(
                            description: "OnAddCollapsiblePanel: " + base.Component.Site.Name
                        );
                        base.RaiseComponentChanging(member: member);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw;
                        }
                        return;
                    }

                    CollapsiblePanel page = (CollapsiblePanel)
                        service.CreateComponent(componentClass: typeof(CollapsiblePanel));
                    string str = null;
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component: page)[
                        name: "Name"
                    ];
                    if ((descriptor2 != null) && (descriptor2.PropertyType == typeof(string)))
                    {
                        str = (string)descriptor2.GetValue(component: page);
                    }
                    if (str != null)
                    {
                        page.Text = str;
                    }
                    component.Controls.Add(value: page);
                    base.RaiseComponentChanged(member: member, oldValue: null, newValue: null);
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (
                ((e.Component == base.Component) && (e.Member != null))
                && (e.Member.Name == "TabPages")
            )
            {
                PropertyDescriptor member = TypeDescriptor.GetProperties(component: base.Component)[
                    name: "Controls"
                ];
                base.RaiseComponentChanging(member: member);
            }
            this.CheckVerbStatus();
        }

        private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            if (
                ((e.Component == base.Component) && (e.Member != null))
                && (e.Member.Name == "TabPages")
            )
            {
                PropertyDescriptor member = TypeDescriptor.GetProperties(component: base.Component)[
                    name: "Controls"
                ];
                base.RaiseComponentChanging(member: member);
            }
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            EventHandlerService service = (EventHandlerService)
                this.GetService(serviceType: typeof(EventHandlerService));
            if (service != null)
            {
                Control focusWindow = service.FocusWindow;
                if (focusWindow != null)
                {
                    focusWindow.Focus();
                }
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            try
            {
                this.disableDrawGrid = true;
                base.OnPaintAdornments(pe: pe);
            }
            finally
            {
                this.disableDrawGrid = false;
            }
        }

        private void OnRemove(object sender, EventArgs eevent)
        {
            TabControl component = (TabControl)base.Component;
            if ((component != null) && (component.TabPages.Count != 0))
            {
                MemberDescriptor member = TypeDescriptor.GetProperties(component: base.Component)[
                    name: "Controls"
                ];
                TabPage selectedTab = component.SelectedTab;
                IDesignerHost service = (IDesignerHost)
                    this.GetService(serviceType: typeof(IDesignerHost));
                if (service != null)
                {
                    DesignerTransaction transaction = null;
                    try
                    {
                        try
                        {
                            transaction = service.CreateTransaction(
                                description: "CollapsibleContainerRemovePanel: "
                                    + selectedTab.Site.Name
                                    + ", "
                                    + base.Component.Site.Name
                            );
                            base.RaiseComponentChanging(member: member);
                        }
                        catch (CheckoutException exception)
                        {
                            if (exception != CheckoutException.Canceled)
                            {
                                throw;
                            }
                            return;
                        }
                        service.DestroyComponent(component: selectedTab);
                        base.RaiseComponentChanged(member: member, oldValue: null, newValue: null);
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = (ISelectionService)
                this.GetService(serviceType: typeof(ISelectionService));
            if (service != null)
            {
                ICollection selectedComponents = service.GetSelectedComponents();
                TabControl component = (TabControl)base.Component;
                foreach (object obj2 in selectedComponents)
                {
                    TabPage tabPageOfComponent = GetTabPageOfComponent(comp: obj2);
                    if ((tabPageOfComponent != null) && (tabPageOfComponent.Parent == component))
                    {
                        component.SelectedTab = tabPageOfComponent;
                        break;
                    }
                }
            }
        }

        private void OnTabSelectedIndexChanged(object sender, EventArgs e)
        {
            ISelectionService service = (ISelectionService)
                this.GetService(serviceType: typeof(ISelectionService));
            if (service != null)
            {
                ICollection selectedComponents = service.GetSelectedComponents();
                TabControl component = (TabControl)base.Component;
                bool flag = false;
                foreach (object obj2 in selectedComponents)
                {
                    TabPage tabPageOfComponent = GetTabPageOfComponent(comp: obj2);
                    if (
                        ((tabPageOfComponent != null) && (tabPageOfComponent.Parent == component))
                        && (tabPageOfComponent == component.SelectedTab)
                    )
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    service.SetSelectedComponents(components: new object[] { base.Component });
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties: properties);
            string[] strArray = new string[] { "SelectedIndex" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor)
                    properties[key: strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[key: strArray[i]] = TypeDescriptor.CreateProperty(
                        componentType: typeof(CollapsibleContainerDesigner),
                        oldPropertyDescriptor: oldPropertyDescriptor,
                        attributes: attributes
                    );
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                base.WndProc(m: ref m);
                if (((int)m.Result) == -1)
                {
                    m.Result = (IntPtr)1;
                }
            }
            else
            {
                base.WndProc(m: ref m);
            }
        }

        // Properties
        protected override bool DrawGrid
        {
            get
            {
                if (this.disableDrawGrid)
                {
                    return false;
                }
                return base.DrawGrid;
            }
        }

        private int SelectedIndex
        {
            get { return this.persistedSelectedIndex; }
            set { this.persistedSelectedIndex = value; }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this.verbs == null)
                {
                    this.removeVerb = new DesignerVerb(
                        text: "Remove",
                        handler: new EventHandler(this.OnRemove)
                    );
                    this.verbs = new DesignerVerbCollection();
                    this.verbs.Add(
                        value: new DesignerVerb(text: "Add", handler: new EventHandler(this.OnAdd))
                    );
                    this.verbs.Add(value: this.removeVerb);
                }
                this.removeVerb.Enabled = this.Control.Controls.Count > 0;
                return this.verbs;
            }
        }
    }
}
