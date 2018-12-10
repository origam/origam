namespace Origam.Workbench
{
    public partial class XmlViewer : AbstractViewContent
    {
        public XmlViewer()
        {
            InitializeComponent();
        }

        public override object Content
        {
            get
            {
                return editor.Text; ;
            }

            set
            {
                editor.Text = value as string;
            }
        }

        protected override void ViewSpecificLoad(object objectToLoad)
        {
            this.Content = objectToLoad;
        }

        public override bool IsViewOnly
        {
            get
            {
                return true;
            }

            set
            {
                base.IsViewOnly = value;
            }
        }
    }
}
