namespace Build.Docker;

public interface IBuildDockerImage
{
    Task<ContainerTag?> Invoke(string registry, string project, string dockerfile, string tag);
}

public class BuildDockerImage : IBuildDockerImage
{
    public async Task<ContainerTag?> Invoke(string registry, string project, string? dockerfile, string tag)
    {
        if (dockerfile == null)
        {
            Console.WriteLine($"Dockerfile not added skipping build on {project}");
            return null;
        }

        var imageBuilder = new ImageBuilder();
        var imageName = await imageBuilder.BuildImage(new ImageBuilderParams($"{registry.TrimEnd('/')}/{project}", tag, dockerfile)
        {
            BuildArgs = new List<string>()
            {
                $"PROJECT={project}"
            }
        });

        return imageName;
    }
}