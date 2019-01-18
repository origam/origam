using Origam.Schema;
using Origam.Workbench.Commands;
using Origam.Workbench.Services;
using System;
using System.Windows.Forms;

namespace Origam.Workbench.Pads
{
    public static class ParentPackage
    {
        public static bool OpenParentPackage(AbstractSchemaItem item)
        {
            TreeNode treenode = (WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser).EbrSchemaBrowser.GetFirstNode();
            if (treenode != null)
            {
                if (treenode.Text != item.Package)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to change the Package from " + treenode.Text + " to " + item.Package + "?", "Package change", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            LoadSchema(treenode, item);
            return true;
        }

        private static void LoadSchema(TreeNode treenode, AbstractSchemaItem item)
        {
            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            if (treenode != null)
            {
                schema.UnloadSchema();
            }
            if (treenode==null || string.Compare(treenode.Text, item.Package, true) != 0)
            {
                foreach (SchemaExtension sch in schema.AllPackages)
                {
                    if (sch.PrimaryKey.Equals(item.SchemaExtension.PrimaryKey))
                    {
                        schema.LoadSchema(sch.Id, false, false);
                        ViewSchemaBrowserPad cmd = new ViewSchemaBrowserPad();
                        cmd.Run();
                    }
                }
            }
        }
    }
}
