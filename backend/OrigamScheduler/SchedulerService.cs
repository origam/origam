using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.Extensions.Hosting;
using ConfigurationManager = Origam.ConfigurationManager;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OrigamScheduler;

public class SchedulerWorker : BackgroundService
{
    private static readonly log4net.ILog log
        = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly ScheduleTimer timer = new();
    private ISchemaService schema;
    private IPersistenceService persistence;
    private WorkflowScheduleSchemaItemProvider schedules;
    private int numberOfWorkflowsRunning = 0;
    private readonly System.Timers.Timer restartTimer = new(1000);
    private bool restarting;

    private delegate void RunWorkflowDelegate(WorkflowSchedule schedule);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (log.IsInfoEnabled)
        {
            var version = Assembly.GetEntryAssembly()
                ?.GetName().Version?.ToString();
            log.InfoFormat("Starting ORIGAM Scheduler version {0}", version);
        }

        try
        {
            OrigamEngine.ConnectRuntime();
            schema =
                ServiceManager.Services.GetService(typeof(ISchemaService)) as
                    ISchemaService;
            persistence =
                ServiceManager.Services.GetService(typeof(IPersistenceService))
                    as IPersistenceService;
            schedules =
                schema.GetProvider(typeof(WorkflowScheduleSchemaItemProvider))
                    as WorkflowScheduleSchemaItemProvider;
            timer.Error += _timer_Error;
            InitSchedules();
            timer.Start();
            if (log.IsInfoEnabled)
            {
                log.Info("Scheduler started successfully");
            }

            restartTimer.Elapsed += RestartTimer_Elapsed;
            restarting = false;
            restartTimer.Start();
        }
        catch (Exception ex)
        {
            if (log.IsFatalEnabled)
            {
                log.Fatal("Scheduler failed to start", ex);
            }

            throw;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void InitSchedules()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (log.IsInfoEnabled)
        {
            log.InfoFormat("Scheduler filter: {0}", settings.SchedulerFilter);
        }
        // Sort schedules alphabetically. If more schedules run at the same time, they will run in this order.
        List<WorkflowSchedule> sortedSchedules = schedules.ChildItems
            .OfType<WorkflowSchedule>()
            .ToList();
        sortedSchedules.Sort();
        foreach (WorkflowSchedule schedule in sortedSchedules)
        {
            if (string.IsNullOrEmpty(settings.SchedulerFilter)
                || settings.SchedulerFilter == schedule?.Group?.RootGroup?.Name)
            {
                Delegate d = new RunWorkflowDelegate(RunWorkflow);
                IScheduledItem item;
                item = GetScheduledTime(schedule.ScheduleTime);
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat(
                        "Scheduling job: {0}, {1}, Next run time: {2}",
                        schedule.Name, schedule.ScheduleTime.Name,
                        item.NextRunTime(DateTime.Now, true));
                }

                try
                {
                    timer.AddJob(item, d, schedule);
                }
                catch (Exception ex)
                {
                    if (log.IsFatalEnabled)
                    {
                        log.Fatal(
                            string.Format("Failed to schedule job: {0}, {1}",
                                schedule.Name, schedule.ScheduleTime?.Name),
                            ex);
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
            engine.PersistenceProvider = persistence.SchemaProvider;
            engine.WorkflowBlock = workflow;
            RuleEngine ruleEngine = RuleEngine.Create(null, null);
            foreach (ISchemaItem parameter in schedule.ChildItems)
            {
                if (parameter != null)
                {
                    ISchemaItem context =
                        workflow.GetChildByName(parameter.Name,
                            ContextStore.CategoryConst);
                    if (context == null)
                    {
                        throw new ArgumentOutOfRangeException("name",
                            parameter.Name,
                            "Workflow parameter not found for workflow schedule '" +
                            schedule.Path + "'");
                    }

                    engine.InputContexts.Add(context.PrimaryKey,
                        ruleEngine.Evaluate(parameter));
                }
            }

            numberOfWorkflowsRunning++;
            WorkflowHost.DefaultHost.ExecuteWorkflow(engine);
            if (engine.WorkflowException != null)
            {
                throw engine.WorkflowException;
            }

            if (log.IsInfoEnabled)
            {
                log.Info(schedule.Name + " finished");
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    string.Format(
                        "Error occured while running the workflow {0}",
                        workflow?.Name), ex);
            }
        }
        finally
        {
            numberOfWorkflowsRunning--;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (log.IsInfoEnabled)
        {
            log.InfoFormat(
                "Stopping Scheduler. Number of workflows running: {0}",
                numberOfWorkflowsRunning.ToString());
        }

        timer.Stop();
        ServiceManager.Services.GetService<IWorkQueueService>().UnloadService();
        while (numberOfWorkflowsRunning != 0)
        {
            await Task.Delay(500, cancellationToken);
        }

        if (log.IsInfoEnabled)
        {
            log.Info("Scheduler stopped.");
        }

        await base.StopAsync(cancellationToken);
    }

    private void _timer_Error(object sender, ExceptionEventArgs args)
    {
        if (log.IsErrorEnabled)
        {
            log.Error("Schedule workflow error", args?.Error);
        }
    }

    private void RestartTimer_Elapsed(object sender,
        System.Timers.ElapsedEventArgs e)
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

            if (log.IsInfoEnabled)
            {
                log.Info("starting to restart scheduler");
            }

            timer.Stop();
            while (numberOfWorkflowsRunning != 0)
            {
                Thread.Sleep(500);
            }

            timer.ClearJobs();
            if (log.IsInfoEnabled)
            {
                log.Info(
                    "Schedules stopped, waiting to unload all the other services...");
            }

            schema.UnloadSchema();
            OrigamEngine.UnloadConnectedServices();
            // unload also work queues - moved to UnloadConnectedService()
            //IWorkbenchService wqService = ServiceManager.Services.GetService(typeof(IWorkQueueService));
            // ServiceManager.Services.UnloadService(wqService);
            // really wait for services to be unloaded
            if (log.IsInfoEnabled)
            {
                log.Info("Scheduler stopped.");
            }

            OrigamEngine.ConnectRuntime();
            schema =
                ServiceManager.Services.GetService(typeof(ISchemaService)) as
                    ISchemaService;
            persistence =
                ServiceManager.Services.GetService(typeof(IPersistenceService))
                    as IPersistenceService;
            schedules =
                schema.GetProvider(typeof(WorkflowScheduleSchemaItemProvider))
                    as WorkflowScheduleSchemaItemProvider;
            InitSchedules();
            timer.Start();
            if (log.IsInfoEnabled)
            {
                log.Info("Scheduler restarted successfully");
            }

            restarting = false;
            restartTimer.Interval = 1000;
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Fatal(
                    "Scheduler restart failed. Please restart the scheduler windows service. ({0})",
                    ex);
            }

            throw;
        }
    }
}