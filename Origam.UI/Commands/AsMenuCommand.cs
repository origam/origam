#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Origam.Gui.UI;


namespace Origam.UI
{


	public class AsMenuCommand : ToolStripMenuItem, IStatusUpdate, IDisposable
	{
		private readonly object caller;

		public AsMenuCommand()
		{
		}

		public AsMenuCommand(string label) : base(label)
		{
			this.Description = label;
		}

		public AsMenuCommand(string label, object caller) : base(label)
		{
			this.Description = label;
			this.caller = caller;
		}

		public AsMenuCommand(string label, ICommand menuCommand) : base(label)
		{
			this.Description = label;
			this.Command = menuCommand;
		}

		public AsMenuCommand(AsMenuCommand other): base(other.Description)
		{
			this.Description = other.Description;
			this.caller = other.caller;
			this.Command = other.Command;
			this.SubItems = other.SubItems;
			this.Image = other.Image;
			this.ShortcutKeys = other.ShortcutKeys;
			
			ShareAllEventHandlers(other);
		}

		// This method causes this AsMenuCommand to share event handlers with
		// the other AsMenuCommand. When any handler is added or removed in this
		// the same happens in the other and vice versa.
		// https://stackoverflow.com/questions/6055038/how-to-clone-control-event-handlers-at-run-time
		private void ShareAllEventHandlers(AsMenuCommand other)
		{
			var eventsField = typeof(Component).GetField("events",
				BindingFlags.NonPublic | BindingFlags.Instance);
			var eventHandlerList = eventsField.GetValue(other);
			eventsField.SetValue(this, eventHandlerList);
		}

		public ArrayList SubItems { get; } = new ArrayList();

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
					PopulateMenu(item);
				}

				this.Enabled = (DropDownItems.Count != 0);
			}
		}

        protected override void OnDropDownShow(EventArgs e)
        {
            if (DropDownItems.Count == 1)
            {
	            if (DropDownItems[0] is SubmenuBuilderPlaceholder builderPlaceholder)
                {
                    this.DropDownItems.RemoveAt(0);
                    PopulateBuilder(builderPlaceholder.Builder);
                }
            }
            base.OnDropDownShow(e);
        }

        private void PopulateBuilder(ISubmenuBuilder builder)
        {
            ToolStripMenuItem[] submenu = builder.BuildSubmenu(caller);
            foreach (var subItem in submenu)
            {
                PopulateMenu(subItem);
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
                        DropDownItems.Add(new SubmenuBuilderPlaceholder(submenuBuilder));
                    }
                }
                else
                {
                    PopulateBuilder(submenuBuilder);
                }
            } 
			else 
			{
				if (item is IStatusUpdate update) 
				{
					update.UpdateItemsToDisplay();
				}
				DropDownItems.Add((ToolStripItem)item);
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
	
	public class AsButtonCommand : BigToolStripButton, IStatusUpdate, IDisposable
	{
	    private string description = string.Empty;

        public AsButtonCommand(string label)
		{
			this.Description = label;
		}
		
		public ICommand Command { get; set; }


		
		private string Description 
		{
			get => description;
			set 
			{
				description = value;
                this.ToolTipText = RemoveSingleAmpersands(value).Replace("&&","&");
			}
		}
        /// <summary>
        /// Removes isolated ampersands, double ampersands are not removed.
        /// Ampersands separated by only one character will also not be removed (for example "ab&d&ef" => "abd&ef") 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
	    private string RemoveSingleAmpersands(string str)
	    {
	        return Regex.Replace(str, @"(^|[^&])(&)($|[^&])", "$1$3");
        }


	    public bool IsEnabled
		{
			get
			{
				bool isEnabled = true; 
				if (Command != null && Command is IMenuCommand) 
				{
					isEnabled &= ((IMenuCommand)Command).IsEnabled;
				}
				return isEnabled;
			}
			set => this.Enabled = value;
		}

		#region IStatusUpdate Members

		public void UpdateItemsToDisplay()
		{
			if (Command != null && Command is IMenuCommand) 
			{
				bool isEnabled = IsEnabled & ((IMenuCommand)Command).IsEnabled;
				if (Enabled != isEnabled) 
				{
					Enabled = isEnabled;
				}
			}
			
			this.Text = this.Description;
		}

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			(Command as IDisposable)?.Dispose();
		}

		#endregion
	}
}
