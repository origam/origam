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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.VisualStyles;
using Origam.Schema.GuiModel;
using Origam.Gui.Win;
using Origam.DA;
using Origam.Services;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.PropertyGrid;
using Origam.Workbench.Services;

namespace Origam.Gui.Designer;
/// <summary>
/// the host implementation
/// </summary>
public class DesignerHostImpl:IDesignerHost,IContainer,IServiceContainer,IComponentChangeService,IExtenderProviderService,IDesignerEventService, IExtenderListService, IDesignerOptionService
{

	//		private Guid ControlSet = new    Guid("727d9807-9697-4732-b8d8-e6abd6c2a318");
	//		private Guid CntrlSetItm = new   Guid ("42eb75c8-c939-459e-995e-2999729c8add");
	//		private Guid FormControl = new   Guid("00000000-0001-48f1-9da4-a130c04972d1");
	//		private Guid ButtonControl = new Guid("00000000-0002-4732-b8d8-e6abd6c2a318");
	private ISelectionService              selectionService;       // selection services
	private IMenuCommandService            menuCommandService;     // the menu command service
	private IHelpService                   helpService;            // the help service - UNIMPLEMENTED
	private IReferenceService              referenceService;       // service to maintain references - UNIMPLEMENTED
	private IMenuEditorService             menuEditorService;      // Menu editor service - UNIMPLEMENTED
	private IToolboxService	               toolboxService;         // toolbox service
	private AmbientProperties			   ambientPropertiesService;
    private IDesignerOptionService         designerOptionService;
    private DesignerActionService          designerActionService;
	public DataSet DataSet;
	public PanelControlSet PanelSet=null;
	public bool IsComplexControl=false;
	public bool IsFieldControl=false;
	public bool IsExternalControl = false;
	public string FieldName="";
	//panel generator
	public Origam.Gui.Win.FormGenerator Generator = new FormGenerator();		
	// root component
	private IComponent						rootComponent;
	private IRootDesigner					rootDesigner;
	// service container
	private IServiceContainer               serviceContainer;
	// site-name to site mapping
	private IDictionary						sites;
	// component to designer mapping
	private Hashtable						designers;
	// extender provider list
	private List<IExtenderProvider>							extenderProviders;
	// transaction list
	private int                            transactionCount;          // >0 means we're doing a transaction
	private StringStack                    transactionDescriptions;   // string descriptions of the current transactions
	public DesignerHostImpl(IServiceProvider parentProvider)
	{
		// append to the parentProvider...
		serviceContainer = new ServiceContainer(parentProvider);
		// site name to ISite mapping
		sites = new Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
		// component to designer mapping
		designers = new Hashtable();
		// list of extender providers
		extenderProviders = new List<IExtenderProvider>();
		// services
		ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
		
		serviceContainer.AddService(typeof(IDesignerHost), this);
		serviceContainer.AddService(typeof(IContainer), this);
		serviceContainer.AddService(typeof(IComponentChangeService), this);
		serviceContainer.AddService(typeof(IExtenderProviderService), this);
		serviceContainer.AddService(typeof(IExtenderListService), this);
		serviceContainer.AddService(typeof(IDesignerEventService), this);
		serviceContainer.AddService(typeof(IDesignerOptionService), this);
		serviceContainer.AddService(typeof(INameCreationService), new NameCreationServiceImpl(this));
		serviceContainer.AddService(typeof(ISelectionService), callback);
		serviceContainer.AddService(typeof(IMenuCommandService), callback);
		serviceContainer.AddService(typeof(ITypeDescriptorFilterService), callback);
		serviceContainer.AddService(typeof(IToolboxService), callback);
		serviceContainer.AddService(typeof(AmbientProperties), callback);
        serviceContainer.AddService(typeof(IPropertyValueUIService), callback);
        IExtenderProviderService extenderProvider = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
		extenderProvider.AddExtenderProvider(new ControlExtenderProvider());
		extenderProvider.AddExtenderProvider(new MultiColumnAdapterFieldExtenderProvider());
        extenderProvider.AddExtenderProvider(new RequestSaveAfterChangeExtenderProvider());
	}
	#region IDesignerHost Members
	public IContainer Container
	{
		get
		{
			return this;
		}
	}
	public event System.EventHandler TransactionOpening;
	public event System.EventHandler TransactionOpened;
	public event System.EventHandler LoadComplete;
	public IDesigner GetDesigner(IComponent component)
	{
		return designers[component] as IDesigner;
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
		get
		{
			return false;
		}
	}
	public IComponent CreateComponent(Type componentClass, string name)
	{
		return CreateComponent(componentClass, null, name);
	}
	public IComponent CreateComponent(Type componentClass, ControlItem controlItem, string name)
	{
		try
		{
			return TryCreateComponent(componentClass, controlItem, name);
		} catch (Exception ex)
		{
			AsMessageBox.ShowError(WorkbenchSingleton.Workbench as IWin32Window,
				ex.Message, "Error creating component", ex);
			throw;
		}
	}
	private IComponent TryCreateComponent(Type componentClass, 
		ControlItem controlItem, string name)
	{
		var( newComponent, componentName) = CreateComponentInstance(componentClass, controlItem);
		Add(newComponent, name ?? componentName);
		if(this.IsFieldControl && this.DataSet !=null && newComponent is IAsControl)
		{
			string BindProp = (newComponent as IAsControl).DefaultBindableProperty;
			try
			{
				(newComponent as Control).DataBindings.Add(BindProp, this.DataSet,
					this.DataSet.Tables[0].TableName + "." + this.FieldName);
			} catch (ArgumentException ex)
			{
				throw new ArgumentException(
					$"{newComponent.GetType()} does not contain {FieldName}. Your Root model might be out of date.",
					ex);
			}
			//If is it dataconsumer (valuelist editor)
			try
			{
				if (newComponent is Origam.UI.ILookupControl)
				{
					Guid lookupId = (Guid)this.DataSet.Tables[0].Columns[this.FieldName].ExtendedProperties[Const.DefaultLookupIdAttribute];
					(newComponent as Origam.UI.ILookupControl).LookupId = lookupId;
					(newComponent as Origam.UI.ILookupControl).CreateMappingItemsCollection();
				}
			}
			catch{} 
			
		}
        this.IsFieldControl = false;
		this.PanelSet =null;
		this.IsComplexControl =false;
		return newComponent;
	}
	private (IComponent, string) CreateComponentInstance(Type type, ControlItem controlItem)
	{
		if (IsComplexControl)
		{
			Control instance = Generator.LoadControl(FormTools.GetItemFromControlSet(PanelSet));
			return (instance, null);
		}
		var baseControl = ServiceManager.Services
			.GetService<ISchemaService>()
			.GetProvider<UserControlSchemaItemProvider>()
			.ChildItems
			.Cast<ControlItem>()
			.FirstOrDefault(item =>
				item.ControlType == type.FullName);
		ControlItem refControl = controlItem;
		if (refControl == null)
		{
			if( ToolboxPane.DragAndDropControl != null &&
			    ToolboxPane.DragAndDropControl.ControlType == type.ToString())
			{
				refControl = ToolboxPane.DragAndDropControl;
			}
			else
			{
				refControl = baseControl;
			}
		}
		
		var missingPropertyItems = refControl?
			.ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst)
			.Where(propItem => type.GetProperty(propItem.Name) == null)
			.ToList();
		
		if (missingPropertyItems == null || missingPropertyItems.Count == 0)
		{
			IComponent instance = Activator.CreateInstance(type) as IComponent;
			return (instance, refControl?.Name);
		}
		DynamicTypeFactory dynamicTypeFactory = new DynamicTypeFactory();
		Type newType =
			dynamicTypeFactory.CreateNewTypeWithDynamicProperties(
				parentType: type,
				inheritor: refControl,
				dynamicProperties: missingPropertyItems.Select(propItem =>
					new DynamicProperty
					{
						Name = propItem.Name,
						SystemType = propItem.SystemType,
						Category = "Data",
					})
			);
		IComponent component = Activator.CreateInstance(newType) as IComponent;
		if (component is Control control)
		{
			control.Name = type.Name;
			control.Text = type.Name;
		}
		return (component, refControl?.Name);
	}
	IComponent System.ComponentModel.Design.IDesignerHost.CreateComponent(Type componentClass)
	{
		return CreateComponent(componentClass, null, null);
	}
	// Gets a value indicating whether the designer host is currently in a transaction.
	public bool InTransaction 
	{ 
		get 
		{
			return transactionCount > 0;
		}
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
		get 
		{
			return transactionCount;
		}
		set 
		{
			transactionCount = value ;
		}
	}
	public DesignerTransaction CreateTransaction(string description)
	{
		return new DesignerTransactionImpl(this, description);
	}
	DesignerTransaction System.ComponentModel.Design.IDesignerHost.CreateTransaction()
	{
		return CreateTransaction("");
	}
	internal void OnTransactionOpened(EventArgs e) 
	{
		if (TransactionOpened != null)
			TransactionOpened(this, e);
	}
    
	internal void OnTransactionOpening(EventArgs e) 
	{
		if (TransactionOpening != null)
			TransactionOpening(this, e);
	}
	internal void OnTransactionClosed(DesignerTransactionCloseEventArgs e) 
	{
		if (TransactionClosed != null)
			TransactionClosed(this, e);
	}
    
	internal void OnTransactionClosing(DesignerTransactionCloseEventArgs e) 
	{
		if (TransactionClosing != null)
			TransactionClosing(this, e);
	}
	public void DestroyComponent(IComponent component)
	{
		string name;
		if (component == null) 
		{
			throw new ArgumentNullException("component");
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
		InheritanceAttribute ia = (InheritanceAttribute)TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
		if (ia != null && ia.InheritanceLevel != InheritanceLevel.NotInherited) 
		{
			throw new InvalidOperationException("Cannot remove inherited components.");
		}
		if(IsInherited(component))
		{
			return;
		}
		if (((IDesignerHost)this).InTransaction)
		{
			Remove(component);    
		}
		else
		{  
			DesignerTransaction t;
			using (t = ((IDesignerHost)this).CreateTransaction("Destroying Component " + name)) 
			{
				// We need to signal changing and then perform the remove.  Remove must be done by us and not
				// by Dispose because (a) people need a chance to cancel through a Removing event, and (b)
				// Dispose removes from the container last and anything that would sync Removed would end up
				// with a dead component.
				//
				//
				Remove(component);
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
				Activated(this,EventArgs.Empty);
			}
		}
		catch{}
	}
	public void OnLoadComplete()
	{
		try
		{
			if (LoadComplete != null)
			{
				LoadComplete(this,EventArgs.Empty);
			}
		}
		catch{}
	}
	public string RootComponentClassName
	{
		get
		{
			return rootComponent.GetType().Name;
		}
	}
	public Type GetType(string typeName)
	{
		Type type = null;
		ITypeResolutionService typeResolverService = (ITypeResolutionService)GetService(typeof(ITypeResolutionService));
		
		if (typeResolverService != null)
		{
			type = typeResolverService.GetType(typeName);
		}
		else
		{
			type = Type.GetType(typeName);
		}
		return type;
	}
	public event System.ComponentModel.Design.DesignerTransactionCloseEventHandler TransactionClosing;
	public IComponent RootComponent
	{
		get
		{
			return rootComponent;
		}
	}
	#endregion
	#region IServiceContainer Members
	public void RemoveService(Type serviceType, bool promote)
	{
		serviceContainer.RemoveService(serviceType,promote);
	}
	void System.ComponentModel.Design.IServiceContainer.RemoveService(Type serviceType)
	{
		serviceContainer.RemoveService(serviceType);
	}
	public void AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback, bool promote)
	{
		serviceContainer.AddService(serviceType,callback,promote);
	}
	void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, System.ComponentModel.Design.ServiceCreatorCallback callback)
	{
		serviceContainer.AddService(serviceType,callback);
	}
	void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
	{
		serviceContainer.AddService(serviceType,serviceInstance,promote);
	}
	void System.ComponentModel.Design.IServiceContainer.AddService(Type serviceType, object serviceInstance)
	{
		serviceContainer.AddService(serviceType,serviceInstance);
	}
	#endregion
	#region IServiceProvider Members
	public object GetService(Type serviceType)
	{
        object service = serviceContainer.GetService(serviceType);
        if (service == null)
        {
            return null;   
        }
		return serviceContainer.GetService(serviceType);
	}
	#endregion
	#region IContainer Members
	public ComponentCollection Components
	{
		get
		{
			return new ComponentCollection(GetAllComponents());
		}
	}
	public void Remove(IComponent component)
	{
		if(component==null)
		{
			throw new ArgumentException("component");
		}
		IDesigner designer = designers[component] as IDesigner;
        // fire off changing and removing event
		ComponentEventArgs ce = new ComponentEventArgs(component);
		OnComponentChanging(component,null);
		try
		{
			if(ComponentRemoving!=null)
			{
				ComponentRemoving(this, ce);
			}
		}
		catch
		{
			// dont throw here
		}
		// make sure we own the component
		if(component.Site!=null && component.Site.Container==this)
		{
			// remove from extender provider list
			if (component is IExtenderProvider)
			{
				IExtenderProviderService extenderProvider = (IExtenderProviderService)GetService(typeof(IExtenderProviderService));
				if(extenderProvider!=null)
				{
					extenderProvider.RemoveExtenderProvider((IExtenderProvider)component);
				}
			}
			// remove the site
			sites.Remove(component.Site.Name);
			// dispose the designer
			if(designer!=null)
			{
				designer.Dispose();
				// get rid of the designer from the list
				designers.Remove(component);
			}
			if(! _disposing)
			{
				// fire off removed event
				try
				{
					if(ComponentRemoved!=null)
					{
						ComponentRemoved(this, ce);
					}
					this.OnComponentChanged(component,null,null,null);
				}
				catch
				{
					// dont throw here
				}
				// breakdown component, container, and site relationship
				component.Site=null;
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
			service = (IInheritanceService) this.GetService(typeof(IInheritanceService));
			if ((service != null) && service.GetInheritanceAttribute(component).Equals(InheritanceAttribute.InheritedReadOnly))
			{
				return true;
			}
		}
		return false;
	}
	
	public void Add(IComponent component, string name)
	{
		// we have to have a component
		if(component==null)
		{
			throw new ArgumentException("component");
		}
		
		// if we dont have a name, create one
		if(name==null || name.Trim().Length==0)
		{
			// we need the naming service
			INameCreationService nameCreationService = GetService(typeof(INameCreationService))as INameCreationService;
			if(nameCreationService==null)
			{
				throw new Exception("Failed to get INameCreationService.");
			}
			Type nameType = DynamicTypeFactory.GetOriginalType(component.GetType());
			name = nameCreationService.CreateName(this, nameType);
		}
		else
		{
			name = ModifyNameIfPresent(name);
		}
		// if we own the component and the name has changed
		// we just rename the component
		if (component.Site != null && component.Site.Container == this &&
			name!=null && string.Compare(name,component.Site.Name,true)!=0) 
		{
			// name validation and component changing/changed events
			// are fired in the Site.Name property so we don't have 
			// to do it here...
			component.Site.Name=name;
			// bail out
			return;
		}
		// create a site for the component
		ISite site = new SiteImpl(component,name,this);
		// create component-site association
		
		
		component.Site=site;
		// the container-component association was established when 
		// we created the site through site.host.
		// we need to fire adding/added events. create a component event args 
		// for the component we are adding.
		ComponentEventArgs evtArgs = new ComponentEventArgs(component);
		// fire off adding event
		if(ComponentAdding!=null)
		{
			try
			{
				ComponentAdding(this,evtArgs);
			}
			catch{}
		}
		
		// if this is the root component
		IDesigner designer = null;
		if(rootComponent==null)
		{
			// set the root component
			rootComponent=component;
			// create the root designer
			designer = (IRootDesigner)TypeDescriptor.CreateDesigner(component,typeof(IRootDesigner));
			rootDesigner = (IRootDesigner)designer;
		}
		else
		{
			designer = TypeDescriptor.CreateDesigner(component,typeof(IDesigner)); 
		}
		if(designer!=null)
		{
			// add the designer to the list
			designers.Add(component,designer);
			// initialize the designer
			designer.Initialize(component);
		}
		// add to container component list
		sites.Add(site.Name,site);
		// now fire off added event
		if(ComponentAdded!=null)
		{
			try
			{
				ComponentAdded(this,evtArgs);
			}
			catch{}
		}
	}
	private string ModifyNameIfPresent(string componentName)
	{
		if (!sites.Contains(componentName))
		{
			return componentName;
		}
		for (int i = 1; i < 1000; i++)
		{
			string candidateName = componentName + i;
			if (!sites.Contains(candidateName))
			{
				return candidateName;
			}
		}
		throw new Exception("Could not create a valid name from " + componentName);
	}
	void System.ComponentModel.IContainer.Add(IComponent component)
	{
		Add(component,null);
	}
	/// Creates some of the more infrequently used services
	private object OnCreateService(IServiceContainer container, Type serviceType) 
	{
        
		// Create SelectionService
		if (serviceType == typeof(ISelectionService)) 
		{
			if (selectionService == null) 
			{
				selectionService = new SelectionServiceImpl(this);
			}
			return selectionService;
		}
		if (serviceType == typeof(ITypeDescriptorFilterService)) 
		{
			return new TypeDescriptorFilterServiceImpl(this);
		}         
		
		if (serviceType == typeof(IToolboxService)) 
		{
			if (toolboxService == null) 
			{
				toolboxService = new ToolboxServiceImpl(this);
			}
			return toolboxService;
		}
        
        
		if (serviceType == typeof(IMenuCommandService)) 
		{
			if (menuCommandService == null) 
			{
				menuCommandService = new MenuCommandServiceImpl(this);
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
                designerActionService = new DesignerActionService(this);
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
			if (d != null) d.Dispose();
			menuEditorService = null ;
		}
		if (selectionService != null) 
		{
			IDisposable d = selectionService as IDisposable;
			if (d != null) d.Dispose();
			selectionService = null;
		}
		if (menuCommandService != null) 
		{
			IDisposable d = menuCommandService as IDisposable;
			if (d != null) d.Dispose();
			menuCommandService = null;
		}
		if (toolboxService != null) 
		{
			IDisposable d = toolboxService as IDisposable;
			if (d != null) d.Dispose();
			toolboxService = null;
		}
		if (helpService != null) 
		{
			IDisposable d = helpService as IDisposable;
			if (d != null) d.Dispose();
			helpService = null;
		}
		if (referenceService != null) 
		{
			IDisposable d = referenceService as IDisposable;
			if (d != null) d.Dispose();
			referenceService = null;
		}
		this.Generator.Dispose();
		this.Generator = null;
		this.DataSet = null;
	}
	#endregion
	#region IComponentChangeService Members
	public event System.ComponentModel.Design.ComponentEventHandler ComponentRemoving;
	public void OnComponentChanged(object component, MemberDescriptor member, object oldValue, object newValue)
	{
        if (ComponentChanged != null) 
		{
			ComponentChangedEventArgs ce = new ComponentChangedEventArgs(component, member, oldValue, newValue);
			try
			{
				ComponentChanged(this, ce);
			}
			catch{}
		}
	}
	public void OnComponentChanging(object component, MemberDescriptor member)
	{
		if (ComponentChanging != null) 
		{
			ComponentChangingEventArgs ce = new ComponentChangingEventArgs(component, member);
			try
			{
				ComponentChanging(this, ce);
			}
			catch{}
		}
	}
	internal void OnComponentRename(object component, string oldName, string newName)
	{
		if (ComponentRename != null)
		{
			try
			{
				ComponentRename(this, new ComponentRenameEventArgs(component, oldName, newName));
			}
			catch{}
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
		while(ie.MoveNext())
		{
			ISite site = ie.Current as ISite;
			components[count++]=site.Component;
		}
		return components;
	}
	// This is called during Dispose and Reload methods to unload the current designer.
	private void UnloadDocument() 
	{
		if (helpService != null && rootDesigner != null) 
		{
			helpService.RemoveContextAttribute("Keyword", "Designer_" + rootDesigner.GetType().FullName);
		}
        
		// Note: Because this can be called during Dispose, we are very anal here
		// about checking for null references.
		// If we can get a selection service, clear the selection...
		// we don't want the property browser browsing disposed components...
		// or components who's designer has been destroyed.
		IServiceProvider sp = (IServiceProvider)this;
		ISelectionService selectionService = (ISelectionService)sp.GetService(typeof(ISelectionService));
		System.Diagnostics.Debug.Assert(selectionService != null, "ISelectionService not found");
		if (selectionService != null) 
		{
			selectionService.SetSelectedComponents(null);
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
		sites.Values.CopyTo(siteArray, 0);
		// Destroy all designers.  We save the base designer for last.
		//
		IDesigner[] d = new IDesigner[designers.Values.Count];
		designers.Values.CopyTo(d, 0);
		designers.Clear();
		// Loading, unloading, it's all the same.  It indicates that you
		// shouldn't dirty or otherwise mess with the buffer.  We also
		// create a transaction here to limit the effects of making
		// so many changes.
//			loadingDesigner = true;
		DesignerTransaction trans = CreateTransaction(null);
        
		try 
		{
			for (int i = 0; i < d.Length; i++) 
			{
				if (designers[i] != rootDesignerHolder) 
				{
					try 
					{
						d[i].Dispose();
					}
					catch 
					{
						System.Diagnostics.Debug.Fail("Designer " + designers[i].GetType().Name + " threw an exception during Dispose.");
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
						System.Diagnostics.Debug.Fail("Component " + site.Name + " threw during dispose.  Bad component!!");
					}
					if (comp.Site != null) 
					{
						Remove(comp);
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
				catch 
				{
				}
			}

			if (rootComponentHolder != null) 
			{
				try 
				{
					rootComponentHolder.Dispose();
				}
				catch 
				{
					System.Diagnostics.Debug.Fail("Component " + rootComponentHolder.GetType().Name + " threw during dispose.  Bad component!!");
				}
                
				if (rootComponentHolder.Site != null) 
				{
					Remove(rootComponentHolder);
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
		extenderProviders.Remove(provider);
	}
	public void AddExtenderProvider(IExtenderProvider provider)
	{
		extenderProviders.Add(provider);
	}
	#endregion
	#region IDesignerEventService Members
	public DesignerCollection Designers
	{
		get
		{
			// we just have one designer
			IDesignerHost[] designers = new IDesignerHost[]{this};
			return new DesignerCollection(designers);
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
		extenderProviders.CopyTo(e, 0);
		return e;
	}
	#endregion
	private Control _parentControl;
	public Control ParentControl
	{
		get
		{
			return _parentControl;
		}
		set
		{
			_parentControl = value;
		}
	}
	#region IDesignerOptionService Members
	public void SetOptionValue(string pageName, string valueName, object value)
	{
		// TODO:  Add DesignerHostImpl.SetOptionValue implementation
	}
	public object GetOptionValue(string pageName, string valueName)
	{
		switch(pageName)
		{
			case @"WindowsFormsDesigner\General":
			switch(valueName)
			{
				case "GridSize":
					return new Size(9, 9);
				default:
					return null;
			}
			default:
				return null;
		}
	}
	#endregion
}
