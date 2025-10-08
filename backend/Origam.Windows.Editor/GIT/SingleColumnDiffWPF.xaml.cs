using System.Windows.Controls;
using ICSharpCode.AvalonEdit;

namespace Origam.Windows.Editor.GIT;

/// <summary>
/// Interaction logic for SingleColumnDiffWPF.xaml
/// </summary>
public partial class SingleColumnDiffWPF : UserControl
{
    public SingleColumnDiffWPF()
    {
        InitializeComponent();
    }

    public TextEditor TextEditor => Editor;
}
