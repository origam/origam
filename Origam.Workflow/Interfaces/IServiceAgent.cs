using System;
using System.Collections;

using Origam.Schema.EntityModel;

using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Rule;

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for IServiceAgent.
	/// </summary>
	public interface IServiceAgent
	{
		IPersistenceProvider PersistenceProvider{get; set;}
		RuleEngine RuleEngine{get; set;}
		Hashtable Parameters{get;}
		string MethodName{get; set;}
		AbstractDataStructure OutputStructure{get; set;}
		object Result{get;}
		void Run();
	}
}
