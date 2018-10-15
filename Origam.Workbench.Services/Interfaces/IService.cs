using System;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// This interface must be implemented by all services.
	/// </summary>
	public interface IService
	{
		/// <summary>
		/// This method is called after the services are loaded.
		/// </summary>
		void InitializeService();
		
		/// <summary>
		/// This method is called before the service is unloaded.
		/// </summary>
		void UnloadService();
		
		event EventHandler Initialize;
		event EventHandler Unload;
	}
}
