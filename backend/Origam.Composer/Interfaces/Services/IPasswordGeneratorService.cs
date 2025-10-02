namespace Origam.Composer.Interfaces.Services;

public interface IPasswordGeneratorService
{
    string Generate(int length);
}
