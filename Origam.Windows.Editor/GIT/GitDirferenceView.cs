using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Origam.Git;
using Origam.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Windows.Editor.GIT
{
    public partial class GitDirferenceView : DockContent, IViewContent
    {
        public GitDirferenceView()
        {
            InitializeComponent();
        }

        public void ShowDiff(GitDiff diff)
        {
            oldFileLabel.Text = diff.OldFile.FullName;
            newFileLabel.Text = diff.NewFile.FullName;
            singleColumnDiff1.Show(diff.Text);
        }

        public IWorkbenchWindow WorkbenchWindow { get; set; }

        public string TabPageText => "Git Diff";

        public void SwitchedTo()
        {
        }

        public void Selected()
        {
        }

        public void Deselected()
        {
        }

        public void RedrawContent()
        {
        }

        public string UntitledName { get; set; }
        public string TitleName { get; set; }
        public string StatusText { get; set; }
        public string FileName { get; set; }
        public string HelpTopic { get; }
        public bool IsUntitled { get; }
        public bool IsDirty { get; set; }
        public bool CanRefreshContent { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsViewOnly { get; }
        public bool CreateAsSubViewContent { get; }
        public void RefreshContent()
        {
        }

        public void SaveObject()
        {
        }

        public void LoadObject(object objectToLoad)
        {
            throw new NotImplementedException();
        }

        public object LoadedObject { get; }
        public string Test() => "";

        public event EventHandler TitleNameChanged;
        public event EventHandler DirtyChanged;
        public event EventHandler Saving;
        public event SaveEventHandler Saved;
        public event EventHandler StatusTextChanged;
        public void Dispose()
        {
        }
    }
}
