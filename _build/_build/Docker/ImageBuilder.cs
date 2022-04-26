using System.Collections.ObjectModel;
using System.Diagnostics;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace Build.Docker;

public interface IImageBuilder
{
}

public class ImageBuilderParams
{
    public ImageBuilderParams(string imageName, string imageTag, string dockerfilePath)
    {
        ImageName = imageName;
        ImageTag = imageTag;
        DockerfilePath = dockerfilePath;
    }

    public string ImageName { get; set; }
    public string ImageTag { get; set; }
    public List<string> BuildArgs { get; set; } = new();
    public string DockerfilePath { get; set; }
}

public class ImageBuilder : IImageBuilder
{
    private readonly DockerClient _dockerClient;

    public ImageBuilder(DockerClient dockerClient)
    {
        _dockerClient = dockerClient;
    }

    public async Task<List<string?>> BuildImage(ImageBuilderParams imageBuilderParams)
    {
        var startInfo = new ProcessStartInfo("docker")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        AddArguments(imageBuilderParams, startInfo);
        return await StartProcess(startInfo);
    }

    public async Task<List<string?>> StartProcess(ProcessStartInfo startInfo)
    {
        List<string?> output = new List<string?>();
        var process = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        process.OutputDataReceived += (sender, args) => { output.Add(args.Data); };
        process.ErrorDataReceived += (sender, args) => { output.Add(args.Data); };
        process.Start();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new Exception("Docker build failed with ExitCode " + process.ExitCode);
        }

        return output;
    }

    private void AddArguments(ImageBuilderParams imageBuilderParams, ProcessStartInfo startInfo)
    {
        var arguments = new List<string>();
        arguments.Add("build");
        arguments.Add(".");
        arguments.Add($"-t {imageBuilderParams.ImageName}:{imageBuilderParams.ImageTag}");
        arguments.Add($"-f \"{imageBuilderParams.DockerfilePath}\"");
        imageBuilderParams.BuildArgs.ForEach(d =>
            arguments.Add($"--build-arg {d}")
        );
        startInfo.Arguments = string.Join(" ", arguments);
    }
}