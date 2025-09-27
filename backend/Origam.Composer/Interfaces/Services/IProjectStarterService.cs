using Origam.Composer.DTOs;

namespace Origam.Composer.Interfaces.Services;

public interface IProjectStarterService
{
    void Create(Project project);

    void CreateTasks(Project project);

    List<IProjectBuilder> GetTasks();
}
