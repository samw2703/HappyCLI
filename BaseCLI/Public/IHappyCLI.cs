namespace HappyCLI;

public interface IHappyCLI
{
    Task<string> Execute(string[] args);
}
