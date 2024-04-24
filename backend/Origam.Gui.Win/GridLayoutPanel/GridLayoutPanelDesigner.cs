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
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Origam.Gui.Win
{
	internal class GridLayoutPanelDesigner : ParentControlDesigner
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
				ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
				if (service != null)
				{
					service.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
				}
				IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
				if (service2 != null)
				{
					service2.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
					service2.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
				}
			}
			base.Dispose(disposing);
		}

		private enum TabControlHitTest
		{
			TCHT_NOWHERE = 1,
			TCHT_ONITEMICON = 2,
			TCHT_ONITEMLABEL = 4,
			TCHT_ONITEM = TCHT_ONITEMICON | TCHT_ONITEMLABEL
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
					m_SelectionService =
						(ISelectionService)(this.GetService(typeof(ISelectionService)));
				return m_SelectionService;
			}
		}

		protected override bool GetHitTest(System.Drawing.Point point)
		{
			if (this.SelectionService.PrimarySelection == this.Control)
			{
				TCHITTESTINFO hti = new TCHITTESTINFO();

				hti.pt = this.Control.PointToClient(point);
				hti.flags = 0;

				System.Windows.Forms.Message m = new
					System.Windows.Forms.Message();
				m.HWnd = this.Control.Handle;
				m.Msg = TCM_HITTEST;

				IntPtr lparam =
					System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(hti));
				System.Runtime.InteropServices.Marshal.StructureToPtr(hti,	lparam, false);
				m.LParam = lparam;

				base.WndProc(ref m);
				System.Runtime.InteropServices.Marshal.FreeHGlobal (lparam);

				if (m.Result.ToInt32() != -1)
					return hti.flags != TabControlHitTest.TCHT_NOWHERE;

			}

			return false;
		}

		internal static TabPage GetTabPageOfComponent(object comp)
		{
			if (!(comp is Control))
			{
				return null;
			}
			Control parent = (Control) comp;
			while ((parent != null) && !(parent is TabPage))
			{
				parent = parent.Parent;
			}
			return (TabPage) parent;
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
			base.Initialize(component);
			ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
			if (service != null)
			{
				service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
			}
			IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
			if (service2 != null)
			{
				service2.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
				service2.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
			}
			((TabControl) component).SelectedIndexChanged += new EventHandler(this.OnTabSelectedIndexChanged);
			((TabControl) component).GotFocus += new EventHandler(this.OnGotFocus);
		}

		private void OnAdd(object sender, EventArgs eevent)
		{
			TabControl component = (TabControl) base.Component;
			MemberDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
			IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
			if (service != null)
			{
				DesignerTransaction transaction = null;
				try
				{
					try
					{
						transaction = service.CreateTransaction("OnAddGridLayoutPanelItem: " + base.Component.Site.Name);
						base.RaiseComponentChanging(member);
					}
					catch (CheckoutException exception)
					{
						if (exception != CheckoutException.Canceled)
						{
							throw;
						}
						return;
					}
				
					GridLayoutPanelItem page = (GridLayoutPanelItem) service.CreateComponent(typeof(GridLayoutPanelItem));
					string str = null;
					PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(page)["Name"];
					if ((descriptor2 != null) && (descriptor2.PropertyType == typeof(string)))
					{
						str = (string) descriptor2.GetValue(page);
					}
					if (str != null)
					{
						page.Text = str;
					}
					component.Controls.Add(page);
					base.RaiseComponentChanged(member, null, null);
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
			if (((e.Component == base.Component) && (e.Member != null)) && (e.Member.Name == "TabPages"))
			{
				PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
				base.RaiseComponentChanging(member);
			}
			this.CheckVerbStatus();
		}

		private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
		{
			if (((e.Component == base.Component) && (e.Member != null)) && (e.Member.Name == "TabPages"))
			{
				PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
				base.RaiseComponentChanging(member);
			}
		}

		private void OnGotFocus(object sender, EventArgs e)
		{
			EventHandlerService service = (EventHandlerService) this.GetService(typeof(EventHandlerService));
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
				base.OnPaintAdornments(pe);
			}
			finally
			{
				this.disableDrawGrid = false;
			}
		}

		private void OnRemove(object sender, EventArgs eevent)
		{
			TabControl component = (TabControl) base.Component;
			if ((component != null) && (component.TabPages.Count != 0))
			{
				MemberDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
				TabPage selectedTab = component.SelectedTab;
				IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
				if (service != null)
				{
					DesignerTransaction transaction = null;
					try
					{
						try
						{
							transaction = service.CreateTransaction("GridLayoutPanelRemoveItem: " + selectedTab.Site.Name + ", " + base.Component.Site.Name);
							base.RaiseComponentChanging(member);
						}
						catch (CheckoutException exception)
						{
							if (exception != CheckoutException.Canceled)
							{
								throw;
							}
							return;
						}
						service.DestroyComponent(selectedTab);
						base.RaiseComponentChanged(member, null, null);
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
			ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
			if (service != null)
			{
				ICollection selectedComponents = service.GetSelectedComponents();
				TabControl component = (TabControl) base.Component;
				foreach (object obj2 in selectedComponents)
				{
					TabPage tabPageOfComponent = GetTabPageOfComponent(obj2);
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
			ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
			if (service != null)
			{
				ICollection selectedComponents = service.GetSelectedComponents();
				TabControl component = (TabControl) base.Component;
				bool flag = false;
				foreach (object obj2 in selectedComponents)
				{
					TabPage tabPageOfComponent = GetTabPageOfComponent(obj2);
					if (((tabPageOfComponent != null) && (tabPageOfComponent.Parent == component)) && (tabPageOfComponent == component.SelectedTab))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					service.SetSelectedComponents(new object[] { base.Component });
				}
			}
		}

		protected override void PreFilterProperties(IDictionary properties)
		{
			base.PreFilterProperties(properties);
			string[] strArray = new string[] { "SelectedIndex" };
			Attribute[] attributes = new Attribute[0];
			for (int i = 0; i < strArray.Length; i++)
			{
				PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
				if (oldPropertyDescriptor != null)
				{
					properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(GridLayoutPanelDesigner), oldPropertyDescriptor, attributes);
				}
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 0x84)
			{
				base.WndProc(ref m);
				if (((int) m.Result) == -1)
				{
					m.Result = (IntPtr) 1;
				}
			}
			else
			{
				base.WndProc(ref m);
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
			get
			{
				return this.persistedSelectedIndex;
			}
			set
			{
				this.persistedSelectedIndex = value;
			}
		}

		public override DesignerVerbCollection Verbs
		{
			get
			{
				if (this.verbs == null)
				{
					this.removeVerb = new DesignerVerb("Remove", new EventHandler(this.OnRemove));
					this.verbs = new DesignerVerbCollection();
					this.verbs.Add(new DesignerVerb("Add", new EventHandler(this.OnAdd)));
					this.verbs.Add(this.removeVerb);
				}
				this.removeVerb.Enabled = this.Control.Controls.Count > 0;
				return this.verbs;
			}
		}
	}
}