using Origam.Composer.DTOs;
using Origam.Composer.Interfaces.BuilderTasks;

namespace Origam.Composer.Interfaces.Services;

public interface IProjectBuilderService
{
    void PrepareTasks(Project project);

    void Create(Project project);

    List<IBuilderTask> GetTasks();
}
