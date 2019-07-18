using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.UI.WizardForm
{
    public class MenuFromForm : AbstractWizardForm
    {
        public ISchemaItem Entity { get; set; }
        public string Role { get; set; }
        public string Caption { get; set; }
    }
}
