using Origam.Schema.EntityModel;
using System;
using System.Windows.Forms;

namespace Origam.UI.WizardForm;

public class LookupForm : AbstractWizardForm
{
    public IDataEntity Entity { get; set; }

    public string LookupName { get; set; }

    public IDataEntityColumn NameColumn { get; set; }
        
    public IDataEntityColumn IdColumn { get; private set; } = null;

    public EntityFilter IdFilter { get; set; }
        
    public EntityFilter ListFilter { get; set; }
        

    internal void SetUpForm(ComboBox cboIdFilter, ComboBox cboListFilter, ComboBox cboDisplayField, TextBox txtName)
    {
            cboIdFilter.Items.Clear();
            cboListFilter.Items.Clear();
            cboDisplayField.Items.Clear();
            IDataEntityColumn nameColumn = null;

            if (this.Entity == null) return;

            txtName.Text = this.Entity.Name;

            EntityFilter idFilter = null;

            foreach (EntityFilter filter in this.Entity.ChildItemsByType(EntityFilter.CategoryConst))
            {
                cboListFilter.Items.Add(filter);
                cboIdFilter.Items.Add(filter);

                if (filter.Name == "GetId") idFilter = filter;
            }

            if (idFilter != null) cboIdFilter.SelectedItem = idFilter;

            foreach (IDataEntityColumn column in this.Entity.EntityColumns)
            {
                if (string.IsNullOrEmpty(column.ToString())) continue;
                if (column.Name == "Name") nameColumn = column;
                if (column.IsPrimaryKey && !column.ExcludeFromAllFields) IdColumn = column;

                cboDisplayField.Items.Add(column);
            }

            cboDisplayField.SelectedItem = nameColumn;

            if (IdColumn == null) throw new Exception("Entity has no primary key defined. Cannot create lookup.");
        }
}