
using System.Windows.Forms;

namespace Origam.UI
{
    public partial class LongMessageBox : Form
    {
        public static DialogResult ShowMsgBox(Form parent, string message, string title)
        {
            var messageBox = new LongMessageBox
            {
                Text = title,
                Visible = false
            };
            messageBox.messageTextBox.Text = message;
            return messageBox.ShowDialog(parent);
        }

        public LongMessageBox()
        {
            InitializeComponent();
        }
    }
}
