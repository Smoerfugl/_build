using Build.ShellBuilder;

namespace Build.Docker;

public interface IImagePusher
{
    Task Invoke(ContainerTag tag, CancellationToken cancellationToken);
}

public class ImagePusher : IImagePusher
{
    public async Task Invoke(ContainerTag tag, CancellationToken cancellationToken)
    {
        var processBuilder = new ShellProcessBuilder("docker");

        processBuilder
            .WithArgument($"push {tag.Name}");

        await processBuilder.Run(cancellationToken);
    }
}

public class ContainerTag
{
    public string Name { get; set; } = null!;
}