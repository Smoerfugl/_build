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
        var exists = Directory.Exists(OutputFolder);
        if (exists)
        {
            Directory.Delete(OutputFolder, true);
        }


        Console.WriteLine("Publishing project: " + projectFolder);
        var process = new ShellProcessBuilder("dotnet").WithArgument("publish")
            .WithArgument("-c", "Release")
            .WithArgument("-o", $"output/{projectFolder}")
            .Build();

        process.Start();
        process.WaitForExit();
        return Task.CompletedTask;
    }
}