using Build.Pipelines;
using Build.ShellBuilder;
using Bullseye;

namespace Build.MsBuild;

public interface IPublishSolution
{
    Task Invoke(string projectFolder);
}

public class PublishSolutions : IPublishSolution
{
    private const string OutputFolder = "output";

    public Task Invoke(string projectFolder)
    {
        var exists = Directory.Exists($"{OutputFolder}{Path.DirectorySeparatorChar}{projectFolder}");
        if (exists)
        {
            Directory.Delete($"{OutputFolder}{Path.DirectorySeparatorChar}{projectFolder}", true);
        }


        Console.WriteLine("Publishing project: " + projectFolder);
        new ShellProcessBuilder("dotnet").WithArgument("publish")
            .WithArgument("-c", "Release")
            .WithArgument(projectFolder)
            .WithArgument("-o", $"output/{projectFolder}")
            .Run();

        return Task.CompletedTask;
    }
}