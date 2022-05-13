using Build.ShellBuilder;

namespace Build.Docker;

public interface IImagePusher
{
    Task Invoke(ContainerTag tag);
}

public class ImagePusher : IImagePusher
{
    public async Task Invoke(ContainerTag tag)
    {
        var processBuilder = new ShellProcessBuilder("docker");

        processBuilder
            .WithArgument($"push {tag.Name}");

        await processBuilder.Run();
    }
}

public class ContainerTag
{
    public string Name { get; set; }
}