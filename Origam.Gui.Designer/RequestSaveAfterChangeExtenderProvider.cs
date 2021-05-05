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

using System.ComponentModel;
using System.Windows.Forms;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Designer
{
    [ProvideProperty("RequestSaveAfterChange", typeof(Control))]
    public class RequestSaveAfterChangeExtenderProvider : IExtenderProvider
    {

		[Category("Behavior")]
        [Description("If set to true, client will attempt to send save request after each change, if there are no errors.")]
        [ExtenderProvidedProperty()]
		public bool GetRequestSaveAfterChange(Control acontrol)
		{
			ControlSetItem csi = acontrol.Tag as ControlSetItem;
			if(csi != null)
			{
				return csi.RequestSaveAfterChange;
			}
			else
			{
				return false;
			}
		}
		public void SetRequestSaveAfterChange(Control acontrol, bool value)
		{
			ControlSetItem csi = acontrol.Tag as ControlSetItem;
			if(csi != null)
			{
				csi.RequestSaveAfterChange = value;
			}
		}


		#region IExtenderProvider Members
		public bool CanExtend(object extendee) 
		{
            if (extendee is Control 
            && (extendee as Control).Tag is ControlSetItem) {
                return ((extendee as Control).Tag as ControlSetItem).ControlItem
                    .RequestSaveAfterChangeAllowed;
            }
            else
            {
                return false;
            }
		}
		#endregion
    }
}
