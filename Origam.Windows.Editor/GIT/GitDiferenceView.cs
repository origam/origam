using System;
using Origam.Git;
using Origam.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Windows.Editor.GIT
{
    public partial class GitDiferenceView : DockContent, IViewContent
    {
        public GitDiferenceView()
        {
            InitializeComponent();
        }

        public void ShowDiff(GitDiff gitDiff)
        {
            oldFileLabel.Text = gitDiff.OldFile.FullName;
            newFileLabel.Text = gitDiff.NewFile.FullName;
            
            DiffModelInfo diffInfo = new DiffParser(gitDiff.Text)
                .ParseToLines(maxLinesToReturn: 1000);

            noteLabel.Text = gitDiff.IsEmpty
                ? "No diferences found."
                : $"Showing {diffInfo.LinesReturned} of {diffInfo.LinesTotal} lines.";

            singleColumnDiff1.Show(diffInfo);
        }

        public void ShowDiff(string oldfile,string newfile, string text)
        {
            oldFileLabel.Text = oldfile;
            newFileLabel.Text = newfile;

            DiffModelInfo diffInfo = new DiffParser(text)
                .ParseToLines(maxLinesToReturn: 1000);

            noteLabel.Text = string.IsNullOrEmpty(text)
                ? "No diferences found."
                : $"Showing {diffInfo.LinesReturned} of {diffInfo.LinesTotal} lines.";

            singleColumnDiff1.Show(diffInfo);
        }

        public IWorkbenchWindow WorkbenchWindow { get; set; }

        public string TabPageText => "Diferences";

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
        public string TitleName { get; set; } = "Diferences";
        public string StatusText { get; set; }
        public Guid DisplayedItemId { get; set; }
        public string HelpTopic { get; }
        public bool IsUntitled { get; }
        public bool IsDirty { get; set; }
        public bool CanRefreshContent { get; set; }
        public bool IsReadOnly { get; set; } = true;
        public bool IsViewOnly { get; } = true;
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

        public event EventHandler TitleNameChanged
        {
            add { }
            remove { }
        }
        public event EventHandler DirtyChanged
        {
            add { }
            remove { }
        }

        public event EventHandler Saving
        {
            add { }
            remove { }
        }
        public event SaveEventHandler Saved
        {
            add { }
            remove { }
        }
        public event EventHandler StatusTextChanged
        {
            add { }
            remove { }
        }
    }
}
