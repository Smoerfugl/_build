using Build.Pipelines;
using Docker.DotNet;

namespace Build.Docker;

public interface IBuildDockerImage
{
    Task Invoke(string registry, string project, string dockerfile, string tag);
}

public class BuildDockerImage : IBuildDockerImage
{
    private readonly DockerClient _client;

    public BuildDockerImage(DockerClient client)
    {
        _client = client;
    }

    public async Task Invoke(string registry, string project, string? dockerfile, string tag)
    {
        if (dockerfile == null)
        {
            Console.WriteLine($"Dockerfile not added skipping build on {project}");
            return;
        }

        var imageBuilder = new ImageBuilder(_client);
        await imageBuilder.BuildImage(new ImageBuilderParams($"{registry.TrimEnd('/')}/{project}", tag, dockerfile)
        {
            BuildArgs = new List<string>()
            {
                $"PROJECT={project}"
            }
        });
    }
}