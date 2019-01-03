using System;
using System.Text.RegularExpressions;
using Origam.Gui.UI;

namespace Origam.UI
{
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