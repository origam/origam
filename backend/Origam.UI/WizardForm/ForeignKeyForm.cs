using System.Windows.Forms;
using Origam.Schema.EntityModel;

namespace Origam.UI.WizardForm;
public class ForeignKeyForm : AbstractWizardForm
{
    public IDataEntity MasterEntity;
    public string ForeignKeyName { get; set; }
    public string Caption { get; set; }
    public bool AllowNulls { get; set; }
    public IDataEntity ForeignEntity { get; set; }
    public IDataEntityColumn ForeignField { get; set; }
    public IDataLookup Lookup { get; set; }
    public string SelectForeignEntity { get; set; }
    public string ForeignKeyWiz { get; set; }
    public string SelectForeignField { get; set; }
    public string EnterKeyName { get; set; }
    internal void SetUpForm(TextBox txtFieldName, ComboBox cboEntity, ComboBox cboLookup, ComboBox cboField, CheckBox chkAllowNulls)
    {
        if (cboEntity.Items.Count == 0)
        {
            txtFieldName.Text = "";
            cboEntity.Items.Clear();
            cboLookup.Items.Clear();
            cboField.Items.Clear();
            chkAllowNulls.Checked = true;
            try
            {
                cboEntity.BeginUpdate();
                cboLookup.BeginUpdate();
                foreach (IDataEntity entity in this.MasterEntity.RootProvider.ChildItems)
                {
                    cboEntity.Items.Add(entity);
                }
                Workbench.Services.SchemaService schema = Workbench.Services.ServiceManager.Services.GetService(typeof(Workbench.Services.SchemaService)) as Workbench.Services.SchemaService;
                IDataLookupSchemaItemProvider lookups = schema.GetProvider(typeof(IDataLookupSchemaItemProvider)) as IDataLookupSchemaItemProvider;
                foreach (object lookup in lookups.ChildItems)
                {
                    cboLookup.Items.Add(lookup);
                }
            }
            finally
            {
                cboEntity.EndUpdate();
                cboLookup.EndUpdate();
            }
        }
    }
}
