using Build.ShellBuilder;

namespace Build.MsBuild;

public interface IPublishSolution
{
    Task Invoke(string projectFolder, CancellationToken cancellationToken);
}

public class PublishSolution : IPublishSolution
{
    private const string OutputFolder = "output";

    public async Task Invoke(string projectFolder, CancellationToken cancellationToken)
    {
        var exists = Directory.Exists($"{OutputFolder}{Path.DirectorySeparatorChar}{projectFolder}");
        if (exists)
        {
            Directory.Delete($"{OutputFolder}{Path.DirectorySeparatorChar}{projectFolder}", true);
        }

        Console.WriteLine("Publishing project: " + projectFolder);
        var exitCode = await new ShellProcessBuilder("dotnet")
            .WithArgument("publish")
            .WithArgument("-c", "Release")
            .WithArgument(projectFolder)
            .WithArgument("-o", $"output/{projectFolder}")
            .Run(cancellationToken);

        if (exitCode != 0)
        {
            throw new ApplicationException($"Couldn't build {projectFolder} - check error log");
        }
    }
}