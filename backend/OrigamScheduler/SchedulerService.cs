#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Reflection;
using Origam;
using Origam.Extensions;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Workflow;
using Schedule;
using ConfigurationManager = Origam.ConfigurationManager;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OrigamScheduler;

public class SchedulerWorker(ILogger<SchedulerWorker> log) : BackgroundService
{
    private readonly ScheduleTimer timer = new();
    private ISchemaService schema;
    private IPersistenceService persistence;
    private WorkflowScheduleSchemaItemProvider schedules;
    private int numberOfWorkflowsRunning = 0;
    private readonly System.Timers.Timer restartTimer = new(interval: 1000);
    private bool restarting;
    private delegate void RunWorkflowDelegate(WorkflowSchedule schedule);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            log.LogInformation(message: "Starting ORIGAM Scheduler version {0}", args: version);
        }

        try
        {
            OrigamEngine.ConnectRuntime(runRestartTimer: false);
            schema = ServiceManager.Services.GetService<ISchemaService>();
            persistence = ServiceManager.Services.GetService<IPersistenceService>();
            schedules = schema.GetProvider<WorkflowScheduleSchemaItemProvider>();
            timer.Error += _timer_Error;
            InitSchedules();
            timer.Start();
            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(message: "Scheduler started successfully");
            }

            restartTimer.Elapsed += RestartTimer_Elapsed;
            restarting = false;
            restartTimer.Start();
        }
        catch (Exception ex)
        {
            if (log.IsEnabled(logLevel: LogLevel.Critical))
            {
                log.LogCritical(exception: ex, message: "Scheduler failed to start");
            }

            throw;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(millisecondsDelay: 1000, cancellationToken: stoppingToken);
        }
    }

    private void InitSchedules()
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.LogInformation(message: "Scheduler filter: {0}", args: settings.SchedulerFilter);
        }
        // Sort schedules alphabetically. If more schedules run at the same time, they will run in this order.
        List<WorkflowSchedule> sortedSchedules = schedules
            .ChildItems.OfType<WorkflowSchedule>()
            .ToList();
        sortedSchedules.Sort();
        foreach (WorkflowSchedule schedule in sortedSchedules)
        {
            if (
                string.IsNullOrEmpty(value: settings.SchedulerFilter)
                || settings.SchedulerFilter == schedule?.Group?.RootGroup?.Name
            )
            {
                Delegate d = new RunWorkflowDelegate(RunWorkflow);
                IScheduledItem item;
                item = GetScheduledTime(scheduleTime: schedule.ScheduleTime);
                if (log.IsEnabled(logLevel: LogLevel.Information))
                {
                    log.LogInformation(
                        message: "Scheduling job: {0}, {1}, Next run time: {2}",
                        args:
                        [
                            schedule.Name,
                            schedule.ScheduleTime.Name,
                            item.NextRunTime(time: DateTime.Now, IncludeStartTime: true),
                        ]
                    );
                }

                try
                {
                    timer.AddJob(Schedule: item, f: d, Params: schedule);
                }
                catch (Exception ex)
                {
                    if (log.IsEnabled(logLevel: LogLevel.Critical))
                    {
                        log.LogCritical(
                            message: string.Format(
                                format: "Failed to schedule job: {0}, {1}",
                                arg0: schedule.Name,
                                arg1: schedule.ScheduleTime?.Name
                            ),
                            args: ex
                        );
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
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.LogInformation(message: schedule.Name + " starting");
        }

        try
        {
            WorkflowEngine engine = new WorkflowEngine();
            engine.PersistenceProvider = persistence.SchemaProvider;
            engine.WorkflowBlock = workflow;
            RuleEngine ruleEngine = RuleEngine.Create(contextStores: null, transactionId: null);
            foreach (ISchemaItem parameter in schedule.ChildItems)
            {
                if (parameter != null)
                {
                    ISchemaItem context = workflow.GetChildByName(
                        name: parameter.Name,
                        itemType: ContextStore.CategoryConst
                    );
                    if (context == null)
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "name",
                            actualValue: parameter.Name,
                            message: "Workflow parameter not found for workflow schedule '"
                                + schedule.Path
                                + "'"
                        );
                    }

                    engine.InputContexts.Add(
                        key: context.PrimaryKey,
                        value: ruleEngine.Evaluate(item: parameter)
                    );
                }
            }

            numberOfWorkflowsRunning++;
            WorkflowHost.DefaultHost.ExecuteWorkflow(engine: engine);
            if (engine.WorkflowException != null)
            {
                throw engine.WorkflowException;
            }

            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(message: schedule.Name + " finished");
            }
        }
        catch (Exception ex)
        {
            if (log.IsEnabled(logLevel: LogLevel.Error))
            {
                log.LogOrigamError(
                    ex: ex,
                    message: string.Format(
                        format: "Error occured while running the workflow {0}",
                        arg0: workflow?.Name
                    )
                );
            }
        }
        finally
        {
            numberOfWorkflowsRunning--;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.LogInformation(
                message: "Stopping Scheduler. Number of workflows running: {0}",
                args: numberOfWorkflowsRunning.ToString()
            );
        }

        timer.Stop();
        ServiceManager.Services.GetService<IWorkQueueService>().UnloadService();
        while (numberOfWorkflowsRunning != 0)
        {
            await Task.Delay(millisecondsDelay: 500, cancellationToken: cancellationToken);
        }

        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.LogInformation(message: "Scheduler stopped.");
        }

        await base.StopAsync(cancellationToken: cancellationToken);
    }

    private void _timer_Error(object sender, ExceptionEventArgs args)
    {
        if (log.IsEnabled(logLevel: LogLevel.Error))
        {
            log.LogError(exception: args?.Error, message: "Schedule workflow error");
        }
    }

    private void RestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (restarting)
        {
            return;
        }

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

            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(message: "starting to restart scheduler");
            }

            timer.Stop();
            while (numberOfWorkflowsRunning != 0)
            {
                Thread.Sleep(millisecondsTimeout: 500);
            }

            timer.ClearJobs();
            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(
                    message: "Schedules stopped, waiting to unload all the other services..."
                );
            }

            schema.UnloadSchema();
            OrigamEngine.UnloadConnectedServices();
            // unload also work queues - moved to UnloadConnectedService()
            //IWorkbenchService wqService = ServiceManager.Services.GetService(typeof(IWorkQueueService));
            // ServiceManager.Services.UnloadService(wqService);
            // really wait for services to be unloaded
            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(message: "Scheduler stopped.");
            }

            OrigamEngine.ConnectRuntime();
            schema =
                ServiceManager.Services.GetService(serviceType: typeof(ISchemaService))
                as ISchemaService;
            persistence =
                ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                as IPersistenceService;
            schedules =
                schema.GetProvider(type: typeof(WorkflowScheduleSchemaItemProvider))
                as WorkflowScheduleSchemaItemProvider;
            InitSchedules();
            timer.Start();
            if (log.IsEnabled(logLevel: LogLevel.Information))
            {
                log.LogInformation(message: "Scheduler restarted successfully");
            }

            restarting = false;
            restartTimer.Interval = 1000;
        }
        catch (Exception ex)
        {
            if (log.IsEnabled(logLevel: LogLevel.Critical))
            {
                log.LogCritical(
                    message: "Scheduler restart failed. Please restart the scheduler windows service. ({0})",
                    args: ex
                );
            }

            throw;
        }
    }
}
