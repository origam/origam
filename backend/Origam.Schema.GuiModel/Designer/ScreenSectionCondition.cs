using System;
using System.Collections;
using System.ComponentModel;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.GuiModel
{
    [SchemaItemDescription("Screen Section Condition", "Screen Section Condition", 
        "icon_parameter-mapping.png")]
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class ScreenSectionCondition: AbstractSchemaItem
    {
        public const string CategoryConst = "ScreenSectionCondition";
   
        public override string ItemType => CategoryConst;

        public Guid ScreenSectionId;
        
        [TypeConverter(typeof(PanelControlSetConverter))]
        [XmlReference("screenSection", "ScreenSectionId")]
        public PanelControlSet ScreenSection
        {
            get => (PanelControlSet)PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(ScreenSectionId));
            set
            {
                ScreenSectionId = value?.Id ?? Guid.Empty;
                if (ScreenSectionId != null && ScreenSectionId != Guid.Empty)
                {
                    var panelControl = (PanelControlSet)PersistenceProvider.
                                       RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(ScreenSectionId));
                    Name = panelControl.Name;
                }
            }
        }
        public ScreenSectionCondition(Guid extensionId) : base(extensionId)
        {
        }

        public ScreenSectionCondition(Key primaryKey) : base(primaryKey)
        {
        }

        public ScreenSectionCondition()
        {
        }
        
        public override void GetExtraDependencies(ArrayList dependencies)
        {
            dependencies.Add(ScreenSection);
        }
    }
}