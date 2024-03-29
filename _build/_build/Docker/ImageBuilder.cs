using Build.ShellBuilder;

namespace Build.Docker;

public interface IImageBuilder
{
    Task<ContainerTag?> BuildImage(ImageBuilderParams imageBuilderParams, CancellationToken cancellationToken);
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
    public async Task<ContainerTag?> BuildImage(ImageBuilderParams imageBuilderParams, CancellationToken cancellationToken)
    {
        var processBuilder = new ShellProcessBuilder("docker");

        var imageName = $"{imageBuilderParams.ImageName.ToLower()}:{imageBuilderParams.ImageTag.ToLower()}";
        processBuilder
            .WithArgument("build")
            .WithArgument(".")
            .WithArgument($"-f \"{imageBuilderParams.DockerfilePath}\"")
            .WithArgument($"-t {imageName}");

        imageBuilderParams.BuildArgs.ForEach(d =>
            processBuilder.WithArgument($"--build-arg {d}")
        );
        
        await processBuilder.Run(cancellationToken);
        return new ContainerTag() { Name = imageName };
    }
}