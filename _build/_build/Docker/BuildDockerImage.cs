using Docker.DotNet;

namespace Build.Docker;

public interface IBuildDockerImage
{
    Task Invoke(string service);
}

public class BuildDockerImage: IBuildDockerImage
{
    private readonly DockerClient _client;

    public BuildDockerImage(DockerClient client)
    {
        _client = client;
    }

    public async Task Invoke(string service)
    {
        
        var imageBuilder = new ImageBuilder(_client);
        await imageBuilder.BuildImage(new ImageBuilderParams(service, "latest", "dockerfile")
        {
            
        });
    }
}
