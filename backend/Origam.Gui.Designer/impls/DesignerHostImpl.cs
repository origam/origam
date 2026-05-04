#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Origam.DA;
using Origam.Gui.Win;
using Origam.Schema.GuiModel;
using Origam.Services;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.PropertyGrid;
using Origam.Workbench.Services;

namespace Origam.Gui.Designer;

/// <summary>
/// the host implementation
/// </summary>
public class DesignerHostImpl
    : IDesignerHost,
        IContainer,
        IServiceContainer,
        IComponentChangeService,
        IExtenderProviderService,
        IDesignerEventService,
        IExtenderListService,
        IDesignerOptionService
{
    //		private Guid ControlSet = new    Guid("727d9807-9697-4732-b8d8-e6abd6c2a318");
    //		private Guid CntrlSetItm = new   Guid ("42eb75c8-c939-459e-995e-2999729c8add");
    //		private Guid FormControl = new   Guid("00000000-0001-48f1-9da4-a130c04972d1");
    //		private Guid ButtonControl = new Guid("00000000-0002-4732-b8d8-e6abd6c2a318");
    private ISelectionService selectionService; // selection services
    private IMenuCommandService menuCommandService; // the menu command service
    private IHelpService helpService; // the help service - UNIMPLEMENTED
    private IReferenceService referenceService; // service to maintain references - UNIMPLEMENTED
    private IMenuEditorService menuEditorService; // Menu editor service - UNIMPLEMENTED
    private IToolboxService toolboxService; // toolbox service
    private AmbientProperties ambientPropertiesService;
    private IDesignerOptionService designerOptionService;
    private DesignerActionService designerActionService;
    public DataSet DataSet;
    public PanelControlSet PanelSet = null;
    public bool IsComplexControl = false;
    public bool IsFieldControl = false;
    public bool IsExternalControl = false;
    public string FieldName = "";

    //panel generator
    public Origam.Gui.Win.FormGenerator Generator = new FormGenerator();

    // root component
    private IComponent rootComponent;
    private IRootDesigner rootDesigner;

    // service container
    private IServiceContainer serviceContainer;

    // site-name to site mapping
    private IDictionary sites;

    // component to designer mapping
    private Hashtable designers;

    // extender provider list
    private List<IExtenderProvider> extenderProviders;

    // transaction list
    private int transactionCount; // >0 means we're doing a transaction
    private StringStack transactionDescriptions; // string descriptions of the current transactions

    public DesignerHostImpl(IServiceProvider parentProvider)
    {
        // append to the parentProvider...
        serviceContainer = new ServiceContainer(parentProvider: parentProvider);
        // site name to ISite mapping
        sites = new Hashtable(equalityComparer: StringComparer.InvariantCultureIgnoreCase);
        // component to designer mapping
        designers = new Hashtable();
        // list of extender providers
        extenderProviders = new List<IExtenderProvider>();
        // services
        ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);

        serviceContainer.AddService(serviceType: typeof(IDesignerHost), serviceInstance: this);
        serviceContainer.AddService(serviceType: typeof(IContainer), serviceInstance: this);
        serviceContainer.AddService(
            serviceType: typeof(IComponentChangeService),
            serviceInstance: this
        );
        serviceContainer.AddService(
            serviceType: typeof(IExtenderProviderService),
            serviceInstance: this
        );
        serviceContainer.AddService(
            serviceType: typeof(IExtenderListService),
            serviceInstance: this
        );
        serviceContainer.AddService(
            serviceType: typeof(IDesignerEventService),
            serviceInstance: this
        );
        serviceContainer.AddService(
            serviceType: typeof(IDesignerOptionService),
            serviceInstance: this
        );
        serviceContainer.AddService(
            serviceType: typeof(INameCreationService),
            serviceInstance: new NameCreationServiceImpl(host: this)
        );
        serviceContainer.AddService(serviceType: typeof(ISelectionService), callback: callback);
        serviceContainer.AddService(serviceType: typeof(IMenuCommandService), callback: callback);
        serviceContainer.AddService(
            serviceType: typeof(ITypeDescriptorFilterService),
            callback: callback
        );
        serviceContainer.AddService(serviceType: typeof(IToolboxService), callback: callback);
        serviceContainer.AddService(serviceType: typeof(AmbientProperties), callback: callback);
        serviceContainer.AddService(
            serviceType: typeof(IPropertyValueUIService),
            callback: callback
        );
        IExtenderProviderService extenderProvider = (IExtenderProviderService)GetService(
            serviceType: typeof(IExtenderProviderService)
        );
        extenderProvider.AddExtenderProvider(provider: new ControlExtenderProvider());
        extenderProvider.AddExtenderProvider(
            provider: new MultiColumnAdapterFieldExtenderProvider()
        );
        extenderProvider.AddExtenderProvider(
            provider: new RequestSaveAfterChangeExtenderProvider()
        );
    }

    #region IDesignerHost Members
    public IContainer Container
    {
        get { return this; }
    }
    public event System.EventHandler TransactionOpening;
    public event System.EventHandler TransactionOpened;
    public event System.EventHandler LoadComplete;

    public IDesigner GetDesigner(IComponent component)
    {
        return designers[key: component] as IDesigner;
    }

    public event System.EventHandler Activated;
    public event System.EventHandler Deactivated
    {
        add { }
        remove { }
    }
    public event System.ComponentModel.Design.DesignerTransactionCloseEventHandler TransactionClosed;
    public bool Loading
    {
        get { return false; }
    }

    public IComponent CreateComponent(Type componentClass, string name)
    {
        return CreateComponent(componentClass: componentClass, controlItem: null, name: name);
    }

    public IComponent CreateComponent(Type componentClass, ControlItem controlItem, string name)
    {
        try
        {
            return TryCreateComponent(
                componentClass: componentClass,
                controlItem: controlItem,
                name: name
            );
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: WorkbenchSingleton.Workbench as IWin32Window,
                text: ex.Message,
                caption: "Error creating component",
                exception: ex
            );
            throw;
        }
    }

    private IComponent TryCreateComponent(Type componentClass, ControlItem controlItem, string name)
    {
        var (newComponent, componentName) = CreateComponentInstance(
            type: componentClass,
            controlItem: controlItem
        );
        Add(component: newComponent, name: name ?? componentName);
        if (this.IsFieldControl && this.DataSet != null && newComponent is IAsControl)
        {
            string BindProp = (newComponent as IAsControl).DefaultBindableProperty;
            try
            {
                (newComponent as Control).DataBindings.Add(
                    propertyName: BindProp,
                    dataSource: this.DataSet,
                    dataMember: this.DataSet.Tables[index: 0].TableName + "." + this.FieldName
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(
                    message: $"{newComponent.GetType()} does not contain {FieldName}. Your Root model might be out of date.",
                    innerException: ex
                );
            }
            //If is it dataconsumer (valuelist editor)
            try
            {
                if (newComponent is Origam.UI.ILookupControl)
                {
                    Guid lookupId = (Guid)
                        this.DataSet
                            .Tables[index: 0]
                            .Columns[name: this.FieldName]
                            .ExtendedProperties[key: Const.DefaultLookupIdAttribute];
                    (newComponent as Origam.UI.ILookupControl).LookupId = lookupId;
                    (newComponent as Origam.UI.ILookupControl).CreateMappingItemsCollection();
                }
            }
            catch { }
        }
        this.IsFieldControl = false;
        this.PanelSet = null;
        this.IsComplexControl = false;
        return newComponent;
    }

    private (IComponent, string) CreateComponentInstance(Type type, ControlItem controlItem)
    {
        if (IsComplexControl)
        {
            Control instance = Generator.LoadControl(
                cntrlSet: FormTools.GetItemFromControlSet(controlSet: PanelSet)
            );
            return (instance, null);
        }
        var baseControl = ServiceManager
            .Services.GetService<ISchemaService>()
            .GetProvider<UserControlSchemaItemProvider>()
            .ChildItems.Cast<ControlItem>()
            .FirstOrDefault(predicate: item => item.ControlType == type.FullName);
        ControlItem refControl = controlItem;
        if (refControl == null)
        {
            if (
                ToolboxPane.DragAndDropControl != null
                && ToolboxPane.DragAndDropControl.ControlType == type.ToString()
            )
            {
                refControl = ToolboxPane.DragAndDropControl;
            }
            else
            {
                refControl = baseControl;
            }
        }

        var missingPropertyItems = refControl
            ?.ChildItemsByType<ControlPropertyItem>(itemType: ControlPropertyItem.CategoryConst)
            .Where(predicate: propItem => type.GetProperty(name: propItem.Name) == null)
            .ToList();

        if (missingPropertyItems == null || missingPropertyItems.Count == 0)
        {
            IComponent instance = Activator.CreateInstance(type: type) as IComponent;
            return (instance, refControl?.Name);
        }
        DynamicTypeFactory dynamicTypeFactory = new DynamicTypeFactory();
        Type newType = dynamicTypeFactory.CreateNewTypeWithDynamicProperties(
            parentType: type,
            inheritor: refControl,
            dynamicProperties: missingPropertyItems.Select(selector: propItem => new DynamicProperty
            {
                Name = propItem.Name,
                SystemType = propItem.SystemType,
                Category = "Data",
            })
        );
        IComponent component = Activator.CreateInstance(type: newType) as IComponent;
        if (component is Control control)
        {
            control.Name = type.Name;
            control.Text = type.Name;
        }
        return (component, refControl?.Name);
    }

    IComponent System.ComponentModel.Design.IDesignerHost.CreateComponent(Type componentClass)
    {
        return CreateComponent(componentClass: componentClass, controlItem: null, name: null);
    }

    // Gets a value indicating whether the designer host is currently in a transaction.
    public bool InTransaction
    {
        get { return transactionCount > 0; }
    }

    // Get descriptions of all of our transactions.
    internal StringStack TransactionDescriptions
    {
        get
        {
            if (transactionDescriptions == null)
            {
                transactionDescriptions = new StringStack();
            }
            return transactionDescriptions;
        }
    }

    // Gets the description of the current transaction.
    public string TransactionDescription
    {
        get
        {
            if (transactionDescriptions != null)
            {
                return transactionDescriptions.GetNonNull();
            }
            return "";
        }
    }

    // Get or set the number of transactions we have.
    internal int TransactionCount
    {
        get { return transactionCount; }
        set { transactionCount = value; }
    }

    public DesignerTransaction CreateTransaction(string description)
    {
        return new DesignerTransactionImpl(host: this, description: description);
    }

    DesignerTransaction System.ComponentModel.Design.IDesignerHost.CreateTransaction()
    {
        return CreateTransaction(description: "");
    }

    internal void OnTransactionOpened(EventArgs e)
    {
        if (TransactionOpened != null)
        {
            TransactionOpened(sender: this, e: e);
        }
    }

    internal void OnTransactionOpening(EventArgs e)
    {
        if (TransactionOpening != null)
        {
            TransactionOpening(sender: this, e: e);
        }
    }

    internal void OnTransactionClosed(DesignerTransactionCloseEventArgs e)
    {
        if (TransactionClosed != null)
        {
            TransactionClosed(sender: this, e: e);
        }
    }

    internal void OnTransactionClosing(DesignerTransactionCloseEventArgs e)
    {
        if (TransactionClosing != null)
        {
            TransactionClosing(sender: this, e: e);
        }
    }

    public void DestroyComponent(IComponent component)
    {
        string name;
        if (component == null)
        {
            throw new ArgumentNullException(paramName: "component");
        }
        if (component.Site != null && component.Site.Name != null)
        {
            name = component.Site.Name;
        }
        else
        {
            name = component.GetType().Name;
        }
        // Make sure the component is not being inherited -- we can't delete these!
        //
        //
        InheritanceAttribute ia = (InheritanceAttribute)
            TypeDescriptor.GetAttributes(component: component)[
                attributeType: typeof(InheritanceAttribute)
            ];
        if (ia != null && ia.InheritanceLevel != InheritanceLevel.NotInherited)
        {
            throw new InvalidOperationException(message: "Cannot remove inherited components.");
        }
        if (IsInherited(component: component))
        {
            return;
        }
        if (((IDesignerHost)this).InTransaction)
        {
            Remove(component: component);
        }
        else
        {
            DesignerTransaction t;
            using (
                t = ((IDesignerHost)this).CreateTransaction(
                    description: "Destroying Component " + name
                )
            )
            {
                // We need to signal changing and then perform the remove.  Remove must be done by us and not
                // by Dispose because (a) people need a chance to cancel through a Removing event, and (b)
                // Dispose removes from the container last and anything that would sync Removed would end up
                // with a dead component.
                //
                //
                Remove(component: component);
                //

                component.Dispose();
                t.Commit();
            }
        }
    }

    public void Activate()
    {
        try
        {
            if (Activated != null)
            {
                Activated(sender: this, e: EventArgs.Empty);
            }
        }
        catch { }
    }

    public void OnLoadComplete()
    {
        try
        {
            if (LoadComplete != null)
            {
                LoadComplete(sender: this, e: EventArgs.Empty);
            }
        }
        catch { }
    }

    public string RootComponentClassName
    {
        get { return rootComponent.GetType().Name; }
    }

    public Type GetType(string typeName)
    {
        Type type = null;
        ITypeResolutionService typeResolverService = (ITypeResolutionService)GetService(
            serviceType: typeof(ITypeResolutionService)
        );

        if (typeResolverService != null)
        {
            type = typeResolverService.GetType(name: typeName);
        }
        else
        {
            type = Type.GetType(typeName: typeName);
        }
        return type;
    }

    public event System.ComponentModel.Design.DesignerTransactionCloseEventHandler TransactionClosing;
    public IComponent RootComponent
    {
        get { return rootComponent; }
    }
    #endregion
    #region IServiceContainer Members
    public void RemoveService(Type serviceType, bool promote)
    {
        serviceContainer.RemoveService(serviceType: serviceType, promote: promote);
    }

    void System.ComponentModel.Design.IServiceContainer.RemoveService(Type serviceType)
    {
        serviceContainer.RemoveService(serviceType: serviceType);
    }

    public void AddService(
        Type serviceType,
        System.ComponentModel.Design.ServiceCreatorCallback callback,
        bool promote
    )
    {
        serviceContainer.AddService(serviceType: serviceType, callback: callback, promote: promote);
    }

    void System.ComponentModel.Design.IServiceContainer.AddService(
        Type serviceType,
        System.ComponentModel.Design.ServiceCreatorCallback callback
    )
    {
        serviceContainer.AddService(serviceType: serviceType, callback: callback);
    }

    void System.ComponentModel.Design.IServiceContainer.AddService(
        Type serviceType,
        object serviceInstance,
        bool promote
    )
    {
        serviceContainer.AddService(
            serviceType: serviceType,
            serviceInstance: serviceInstance,
            promote: promote
        );
    }

    void System.ComponentModel.Design.IServiceContainer.AddService(
        Type serviceType,
        object serviceInstance
    )
    {
        serviceContainer.AddService(serviceType: serviceType, serviceInstance: serviceInstance);
    }
    #endregion
    #region IServiceProvider Members
    public object GetService(Type serviceType)
    {
        object service = serviceContainer.GetService(serviceType: serviceType);
        if (service == null)
        {
            return null;
        }
        return serviceContainer.GetService(serviceType: serviceType);
    }
    #endregion
    #region IContainer Members
    public ComponentCollection Components
    {
        get { return new ComponentCollection(components: GetAllComponents()); }
    }

    public void Remove(IComponent component)
    {
        if (component == null)
        {
            throw new ArgumentException(message: "component");
        }
        IDesigner designer = designers[key: component] as IDesigner;
        // fire off changing and removing event
        ComponentEventArgs ce = new ComponentEventArgs(component: component);
        OnComponentChanging(component: component, member: null);
        try
        {
            if (ComponentRemoving != null)
            {
                ComponentRemoving(sender: this, e: ce);
            }
        }
        catch
        {
            // dont throw here
        }
        // make sure we own the component
        if (component.Site != null && component.Site.Container == this)
        {
            // remove from extender provider list
            if (component is IExtenderProvider)
            {
                IExtenderProviderService extenderProvider = (IExtenderProviderService)GetService(
                    serviceType: typeof(IExtenderProviderService)
                );
                if (extenderProvider != null)
                {
                    extenderProvider.RemoveExtenderProvider(provider: (IExtenderProvider)component);
                }
            }
            // remove the site
            sites.Remove(key: component.Site.Name);
            // dispose the designer
            if (designer != null)
            {
                designer.Dispose();
                // get rid of the designer from the list
                designers.Remove(key: component);
            }
            if (!_disposing)
            {
                // fire off removed event
                try
                {
                    if (ComponentRemoved != null)
                    {
                        ComponentRemoved(sender: this, e: ce);
                    }
                    this.OnComponentChanged(
                        component: component,
                        member: null,
                        oldValue: null,
                        newValue: null
                    );
                }
                catch
                {
                    // dont throw here
                }
                // breakdown component, container, and site relationship
                component.Site = null;
                // now dispose the of the component too
                component.Dispose();
            }
        }
    }

    private bool IsInherited(IComponent component)
    {
        IInheritanceService service = null;
        if (component != null)
        {
            service = (IInheritanceService)
                this.GetService(serviceType: typeof(IInheritanceService));
            if (
                (service != null)
                && service
                    .GetInheritanceAttribute(component: component)
                    .Equals(value: InheritanceAttribute.InheritedReadOnly)
            )
            {
                return true;
            }
        }
        return false;
    }

    public void Add(IComponent component, string name)
    {
        // we have to have a component
        if (component == null)
        {
            throw new ArgumentException(message: "component");
        }

        // if we dont have a name, create one
        if (name == null || name.Trim().Length == 0)
        {
            // we need the naming service
            INameCreationService nameCreationService =
                GetService(serviceType: typeof(INameCreationService)) as INameCreationService;
            if (nameCreationService == null)
            {
                throw new Exception(message: "Failed to get INameCreationService.");
            }
            Type nameType = DynamicTypeFactory.GetOriginalType(
                maybeDynamicType: component.GetType()
            );
            name = nameCreationService.CreateName(container: this, dataType: nameType);
        }
        else
        {
            name = ModifyNameIfPresent(componentName: name);
        }
        // if we own the component and the name has changed
        // we just rename the component
        if (
            component.Site != null
            && component.Site.Container == this
            && name != null
            && string.Compare(strA: name, strB: component.Site.Name, ignoreCase: true) != 0
        )
        {
            // name validation and component changing/changed events
            // are fired in the Site.Name property so we don't have
            // to do it here...
            component.Site.Name = name;
            // bail out
            return;
        }
        // create a site for the component
        ISite site = new SiteImpl(comp: component, name: name, host: this);
        // create component-site association

        component.Site = site;
        // the container-component association was established when
        // we created the site through site.host.
        // we need to fire adding/added events. create a component event args
        // for the component we are adding.
        ComponentEventArgs evtArgs = new ComponentEventArgs(component: component);
        // fire off adding event
        if (ComponentAdding != null)
        {
            try
            {
                ComponentAdding(sender: this, e: evtArgs);
            }
            catch { }
        }

        // if this is the root component
        IDesigner designer = null;
        if (rootComponent == null)
        {
            // set the root component
            rootComponent = component;
            // create the root designer
            designer = (IRootDesigner)
                TypeDescriptor.CreateDesigner(
                    component: component,
                    designerBaseType: typeof(IRootDesigner)
                );
            rootDesigner = (IRootDesigner)designer;
        }
        else
        {
            designer = TypeDescriptor.CreateDesigner(
                component: component,
                designerBaseType: typeof(IDesigner)
            );
        }
        if (designer != null)
        {
            // add the designer to the list
            designers.Add(key: component, value: designer);
            // initialize the designer
            designer.Initialize(component: component);
        }
        // add to container component list
        sites.Add(key: site.Name, value: site);
        // now fire off added event
        if (ComponentAdded != null)
        {
            try
            {
                ComponentAdded(sender: this, e: evtArgs);
            }
            catch { }
        }
    }

    private string ModifyNameIfPresent(string componentName)
    {
        if (!sites.Contains(key: componentName))
        {
            return componentName;
        }
        for (int i = 1; i < 1000; i++)
        {
            string candidateName = componentName + i;
            if (!sites.Contains(key: candidateName))
            {
                return candidateName;
            }
        }
        throw new Exception(message: "Could not create a valid name from " + componentName);
    }

    void System.ComponentModel.IContainer.Add(IComponent component)
    {
        Add(component: component, name: null);
    }

    /// Creates some of the more infrequently used services
    private object OnCreateService(IServiceContainer container, Type serviceType)
    {
        // Create SelectionService
        if (serviceType == typeof(ISelectionService))
        {
            if (selectionService == null)
            {
                selectionService = new SelectionServiceImpl(host: this);
            }
            return selectionService;
        }
        if (serviceType == typeof(ITypeDescriptorFilterService))
        {
            return new TypeDescriptorFilterServiceImpl(host: this);
        }

        if (serviceType == typeof(IToolboxService))
        {
            if (toolboxService == null)
            {
                toolboxService = new ToolboxServiceImpl(host: this);
            }
            return toolboxService;
        }

        if (serviceType == typeof(IMenuCommandService))
        {
            if (menuCommandService == null)
            {
                menuCommandService = new MenuCommandServiceImpl(host: this);
            }
            return menuCommandService;
        }
        if (serviceType == typeof(AmbientProperties))
        {
            if (ambientPropertiesService == null)
            {
                ambientPropertiesService = new AmbientProperties();
            }
            return ambientPropertiesService;
        }
        if (serviceType == typeof(IDesignerOptionService))
        {
            if (designerOptionService == null)
            {
                designerOptionService = new WindowsFormsDesignerOptionService();
            }
            return designerOptionService;
        }
        if (serviceType == typeof(DesignerActionService))
        {
            if (designerActionService == null)
            {
                designerActionService = new DesignerActionService(serviceProvider: this);
            }
            return designerActionService;
        }
        if (serviceType == typeof(IPropertyValueUIService))
        {
            return new PropertyValueServiceImpl();
        }
        return null;
    }
    #endregion
    #region IDisposable Members
    bool _disposing = false;

    public void Dispose()
    {
        _disposing = true;
        // Unload the document.
        UnloadDocument();
        // No services after this!
        serviceContainer = null;
        // Now tear down all of our services.
        if (menuEditorService != null)
        {
            IDisposable d = menuEditorService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            menuEditorService = null;
        }
        if (selectionService != null)
        {
            IDisposable d = selectionService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            selectionService = null;
        }
        if (menuCommandService != null)
        {
            IDisposable d = menuCommandService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            menuCommandService = null;
        }
        if (toolboxService != null)
        {
            IDisposable d = toolboxService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            toolboxService = null;
        }
        if (helpService != null)
        {
            IDisposable d = helpService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            helpService = null;
        }
        if (referenceService != null)
        {
            IDisposable d = referenceService as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }

            referenceService = null;
        }
        this.Generator.Dispose();
        this.Generator = null;
        this.DataSet = null;
    }
    #endregion
    #region IComponentChangeService Members
    public event System.ComponentModel.Design.ComponentEventHandler ComponentRemoving;

    public void OnComponentChanged(
        object component,
        MemberDescriptor member,
        object oldValue,
        object newValue
    )
    {
        if (ComponentChanged != null)
        {
            ComponentChangedEventArgs ce = new ComponentChangedEventArgs(
                component: component,
                member: member,
                oldValue: oldValue,
                newValue: newValue
            );
            try
            {
                ComponentChanged(sender: this, e: ce);
            }
            catch { }
        }
    }

    public void OnComponentChanging(object component, MemberDescriptor member)
    {
        if (ComponentChanging != null)
        {
            ComponentChangingEventArgs ce = new ComponentChangingEventArgs(
                component: component,
                member: member
            );
            try
            {
                ComponentChanging(sender: this, e: ce);
            }
            catch { }
        }
    }

    internal void OnComponentRename(object component, string oldName, string newName)
    {
        if (ComponentRename != null)
        {
            try
            {
                ComponentRename(
                    sender: this,
                    e: new ComponentRenameEventArgs(
                        component: component,
                        oldName: oldName,
                        newName: newName
                    )
                );
            }
            catch { }
        }
    }

    public event System.ComponentModel.Design.ComponentEventHandler ComponentAdded;
    public event System.ComponentModel.Design.ComponentRenameEventHandler ComponentRename;
    public event System.ComponentModel.Design.ComponentEventHandler ComponentAdding;
    public event System.ComponentModel.Design.ComponentEventHandler ComponentRemoved;
    public event System.ComponentModel.Design.ComponentChangingEventHandler ComponentChanging;
    public event System.ComponentModel.Design.ComponentChangedEventHandler ComponentChanged;
    #endregion
    #region Private Helper Methods
    private IComponent[] GetAllComponents()
    {
        IComponent[] components = new IComponent[sites.Count];
        // loop over the sites and get all the components
        IEnumerator ie = sites.Values.GetEnumerator();
        int count = 0;
        while (ie.MoveNext())
        {
            ISite site = ie.Current as ISite;
            components[count++] = site.Component;
        }
        return components;
    }

    // This is called during Dispose and Reload methods to unload the current designer.
    private void UnloadDocument()
    {
        if (helpService != null && rootDesigner != null)
        {
            helpService.RemoveContextAttribute(
                name: "Keyword",
                value: "Designer_" + rootDesigner.GetType().FullName
            );
        }

        // Note: Because this can be called during Dispose, we are very anal here
        // about checking for null references.
        // If we can get a selection service, clear the selection...
        // we don't want the property browser browsing disposed components...
        // or components who's designer has been destroyed.
        IServiceProvider sp = (IServiceProvider)this;
        ISelectionService selectionService = (ISelectionService)
            sp.GetService(serviceType: typeof(ISelectionService));
        System.Diagnostics.Debug.Assert(
            condition: selectionService != null,
            message: "ISelectionService not found"
        );
        if (selectionService != null)
        {
            selectionService.SetSelectedComponents(components: null);
        }
        // Stash off the base designer and component.  We are
        // going to be destroying these and we don't want them
        // to be accidently referenced after they're dead.
        //
        IDesigner rootDesignerHolder = rootDesigner;
        IComponent rootComponentHolder = rootComponent;
        rootDesigner = null;
        rootComponent = null;
        SiteImpl[] siteArray = new SiteImpl[sites.Values.Count];
        sites.Values.CopyTo(array: siteArray, index: 0);
        // Destroy all designers.  We save the base designer for last.
        //
        IDesigner[] d = new IDesigner[designers.Values.Count];
        designers.Values.CopyTo(array: d, index: 0);
        designers.Clear();
        // Loading, unloading, it's all the same.  It indicates that you
        // shouldn't dirty or otherwise mess with the buffer.  We also
        // create a transaction here to limit the effects of making
        // so many changes.
        //			loadingDesigner = true;
        DesignerTransaction trans = CreateTransaction(description: null);

        try
        {
            for (int i = 0; i < d.Length; i++)
            {
                if (designers[key: i] != rootDesignerHolder)
                {
                    try
                    {
                        d[i].Dispose();
                    }
                    catch
                    {
                        System.Diagnostics.Debug.Fail(
                            message: "Designer "
                                + designers[key: i].GetType().Name
                                + " threw an exception during Dispose."
                        );
                    }
                }
            }

            // Now destroy all components.
            for (int i = 0; i < siteArray.Length; i++)
            {
                SiteImpl site = siteArray[i];
                IComponent comp = site.Component;
                if (comp != null && comp != rootComponentHolder)
                {
                    try
                    {
                        bool disposed = false;
                        Control ctrl = comp as Control;
                        disposed = ctrl != null && ctrl.IsDisposed;
                        if (!disposed)
                        {
                            comp.Dispose();
                        }
                    }
                    catch
                    {
                        System.Diagnostics.Debug.Fail(
                            message: "Component "
                                + site.Name
                                + " threw during dispose.  Bad component!!"
                        );
                    }
                    if (comp.Site != null)
                    {
                        Remove(component: comp);
                    }
                }
            }

            // Finally, do the base designer and component.
            //
            if (rootDesignerHolder != null)
            {
                try
                {
                    rootDesignerHolder?.Dispose();
                }
                catch { }
            }

            if (rootComponentHolder != null)
            {
                try
                {
                    rootComponentHolder.Dispose();
                }
                catch
                {
                    System.Diagnostics.Debug.Fail(
                        message: "Component "
                            + rootComponentHolder.GetType().Name
                            + " threw during dispose.  Bad component!!"
                    );
                }

                if (rootComponentHolder.Site != null)
                {
                    Remove(component: rootComponentHolder);
                }
            }

            sites.Clear();
        }
        finally
        {
            trans.Commit();
        }
    }
    #endregion
    #region IExtenderProviderService Members
    public void RemoveExtenderProvider(IExtenderProvider provider)
    {
        extenderProviders.Remove(item: provider);
    }

    public void AddExtenderProvider(IExtenderProvider provider)
    {
        extenderProviders.Add(item: provider);
    }
    #endregion
    #region IDesignerEventService Members
    public DesignerCollection Designers
    {
        get
        {
            // we just have one designer
            IDesignerHost[] designers = new IDesignerHost[] { this };
            return new DesignerCollection(designers: designers);
        }
    }
    public event System.ComponentModel.Design.DesignerEventHandler DesignerDisposed
    {
        add { }
        remove { }
    }
    public IDesignerHost ActiveDesigner
    {
        get
        {
            // always this designer
            return this;
        }
    }
    public event System.ComponentModel.Design.DesignerEventHandler DesignerCreated
    {
        add { }
        remove { }
    }
    public event System.ComponentModel.Design.ActiveDesignerEventHandler ActiveDesignerChanged
    {
        add { }
        remove { }
    }
    public event System.EventHandler SelectionChanged
    {
        add { }
        remove { }
    }
    #endregion
    #region IExtenderListService Members
    public IExtenderProvider[] GetExtenderProviders()
    {
        IExtenderProvider[] e = new IExtenderProvider[extenderProviders.Count];
        extenderProviders.CopyTo(array: e, arrayIndex: 0);
        return e;
    }
    #endregion
    private Control _parentControl;
    public Control ParentControl
    {
        get { return _parentControl; }
        set { _parentControl = value; }
    }

    #region IDesignerOptionService Members
    public void SetOptionValue(string pageName, string valueName, object value)
    {
        // TODO:  Add DesignerHostImpl.SetOptionValue implementation
    }

    public object GetOptionValue(string pageName, string valueName)
    {
        switch (pageName)
        {
            case @"WindowsFormsDesigner\General":
            {
                switch (valueName)
                {
                    case "GridSize":
                    {
                        return new Size(width: 9, height: 9);
                    }
                    default:
                    {
                        return null;
                    }
                }
            }
            default:
            {
                return null;
            }
        }
    }
    #endregion
}
