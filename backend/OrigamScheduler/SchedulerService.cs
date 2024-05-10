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
using System.IO;
using System.Collections;
using Origam;
using Origam.Extensions;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.OrigamEngine;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workflow;
using Origam.Rule;
using Schedule;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OrigamScheduler
{
	public class SchedulerService : System.ServiceProcess.ServiceBase
	{
        private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ScheduleTimer _timer = new ScheduleTimer();
		private ISchemaService _schema;
		private IPersistenceService _persistence;
		private WorkflowScheduleSchemaItemProvider _schedules;
		private int _numberOfWorkflowsRunning = 0;
		private string _logPath = "";
        private System.Timers.Timer RestartTimer = new System.Timers.Timer(1000);
        private bool restarting;

        private delegate void RunWorkflowDelegate(WorkflowSchedule schedule);

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SchedulerService()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			_logPath = Path.Combine(System.Windows.Forms.Application.StartupPath,  @"Debug\SchedulerLog.txt");
		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SchedulerService() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            //new SchedulerService().OnStart(null); System.Threading.Thread.Sleep(99999999);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// SchedulerService
			// 
			this.ServiceName = "OrigamSchedulerService";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Starting ORIGAM Scheduler version {0}", System.Windows.Forms.Application.ProductVersion);
            }
			try
			{
                //System.Threading.Thread.Sleep(15000);
				OrigamEngine.ConnectRuntime();

				_schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
				_persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
				_schedules = _schema.GetProvider(typeof(WorkflowScheduleSchemaItemProvider)) as WorkflowScheduleSchemaItemProvider;

				_timer.Error += new ExceptionEventHandler(_timer_Error);
				InitSchedules();
				_timer.Start();

                if (log.IsInfoEnabled)
                {
                    log.Info("Scheduler started successfully");
                }
                RestartTimer.Elapsed += new System.Timers.ElapsedEventHandler(RestartTimer_Elapsed);
                restarting = false;
                RestartTimer.Start();
            }
			catch(Exception ex)
			{
                if (log.IsFatalEnabled)
                {
                    log.Fatal("Scheduler failed to start", ex);
                }
				throw;
			}
		}

		private void InitSchedules()
		{
			OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Scheduler filter: {0}", settings.SchedulerFilter);
            }

			// Sort schedules alphabetically. If more schedules run at the same time, they will run in this order.
			ArrayList sortedSchedules = new ArrayList(_schedules.ChildItems);
			sortedSchedules.Sort();

			foreach(WorkflowSchedule schedule in sortedSchedules)
			{
				if(settings.SchedulerFilter == null 
                || settings.SchedulerFilter == "" 
                || settings.SchedulerFilter == schedule?.Group?.RootGroup?.Name)
				{
					DateTime now = DateTime.Now;
					Delegate d = new RunWorkflowDelegate(RunWorkflow);
					IScheduledItem item;

				
					item = GetScheduledTime(schedule.ScheduleTime);

                    if (log.IsInfoEnabled)
                    {
                        log.InfoFormat("Scheduling job: {0}, {1}, Next run time: {2}",
                            schedule.Name, schedule.ScheduleTime.Name, item.NextRunTime(DateTime.Now, true));
                    }
					try
					{
						_timer.AddJob(item, d, schedule);
					}
					catch(Exception ex)
					{
                        if (log.IsFatalEnabled)
                        {
                            log.Fatal(string.Format("Failed to schedule job: {0}, {1}", schedule.Name, schedule.ScheduleTime?.Name), ex);
                        }
					}
				}
			}
		}

		private IScheduledItem GetScheduledTime(AbstractScheduleTime scheduleTime)
		{
			return scheduleTime.GetScheduledTime();
		}

		private void RunWorkflow(WorkflowSchedule schedule)
		{
			IWorkflow workflow = schedule.Workflow;

            if (log.IsInfoEnabled)
            {
                log.Info(schedule.Name + " starting");
            }

			try
			{
				WorkflowEngine engine = new WorkflowEngine();
				engine.PersistenceProvider = _persistence.SchemaProvider;
				engine.WorkflowBlock = workflow;

				// input parameters
				RuleEngine ruleEngine = RuleEngine.Create(null, null);
				foreach(AbstractSchemaItem parameter in schedule.ChildItems)
				{
					if(parameter != null)
					{
						AbstractSchemaItem context = workflow.GetChildByName(parameter.Name, ContextStore.CategoryConst);
						
						if(context == null)
						{
							throw new ArgumentOutOfRangeException("name", parameter.Name, "Workflow parameter not found for workflow schedule '" + schedule.Path + "'");
						}
						
						engine.InputContexts.Add(context.PrimaryKey, ruleEngine.Evaluate(parameter));
					}
				}

				_numberOfWorkflowsRunning++;

				WorkflowHost.DefaultHost.ExecuteWorkflow(engine);

				if(engine.WorkflowException != null)
				{
					throw engine.WorkflowException;
				}

                if (log.IsInfoEnabled)
                {
                    log.Info(schedule.Name + " finished");
                }
			}
			catch(Exception ex)
			{
                if (log.IsErrorEnabled)
                {
                    log.LogOrigamError(string.Format("Error occured while running the workflow {0}", workflow?.Name), ex);
                }
			}
			finally
			{
				_numberOfWorkflowsRunning--;
			}
		}

		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Stopping Scheduler. Number of workflows running: {0}", _numberOfWorkflowsRunning.ToString());
            }
            _timer.Stop();
            // stop workqueues
            (ServiceManager.Services.GetService(
                typeof(IWorkQueueService)) as IWorkQueueService).UnloadService();
			// wait until all workflows are finished
			while(_numberOfWorkflowsRunning != 0) 
			{   
                // sleep for 0.5s
                System.Threading.Thread.Sleep(500);
			}
            if (log.IsInfoEnabled)
            {
                log.Info("Scheduler stopped.");
            }
		}

		private void _timer_Error(object sender, ExceptionEventArgs Args)
		{
            if (log.IsErrorEnabled)
            {
                log.Error("Schedule workflow error", Args?.Error);
            }
		}

        private void RestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (restarting) return;
            try
            {
                if (OrigamEngine.IsRestartPending())
                {
                    restarting = true;
                }
                else
                {
                    return;
                }


                // do the restarting stuff
                if (log.IsInfoEnabled)
                {
                    log.Info("starting to restart scheduler");
                }
                _timer.Stop();
                // wait until all workflows are finished
                while (_numberOfWorkflowsRunning != 0)
                {
                    // sleep for 0.5s
                    System.Threading.Thread.Sleep(500);
                }
                _timer.ClearJobs();

                if (log.IsInfoEnabled)
                {
                    log.Info("Schedules stopped, waiting to unload all the other services...");
                }
                //System.Threading.Thread.Sleep(30000);

                _schema.UnloadSchema();
                OrigamEngine.UnloadConnectedServices();

                // unload also work queues - moved to UnloadConnectedService()
                //IWorkbenchService wqService = ServiceManager.Services.GetService(typeof(IWorkQueueService));
                // ServiceManager.Services.UnloadService(wqService);

                // really wait for services to be unloaded
                if (log.IsInfoEnabled)
                {
                    log.Info("Scheduler stopped.");
                }

                // connect again
                OrigamEngine.ConnectRuntime();

                _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
                _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
                _schedules = _schema.GetProvider(typeof(WorkflowScheduleSchemaItemProvider)) as WorkflowScheduleSchemaItemProvider;

                //_timer.Error += new ExceptionEventHandler(_timer_Error);
                InitSchedules();
                _timer.Start();

                if (log.IsInfoEnabled)
                {
                    log.Info("Scheduler restarted successfully");
                }
                
                restarting = false;
                RestartTimer.Interval = 1000;
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled)
                {
                    log.Fatal("Scheduler restart failed. Please restart the scheduler windows service. ({0})",
                        ex);
                }
                throw;
            }
        }
    }
}
