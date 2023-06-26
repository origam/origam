namespace Origam.Workbench.Services;

public interface IBackGroundService
{
    // Should stop timers and other running tasks. This is useful when
    // unloading all services because they depend on each other. Not
    // stopping all tasks before calling UnloadService would result in exceptions. 
    void StopTasks();
}