using Docker.DotNet;
using Microsoft.IdentityModel.Tokens;

namespace Build.Docker;

public class DockerClientFactory
{
    public DockerClient Invoke()
    {
        var uri = "";
        if ( Environment.OSVersion.Platform == PlatformID.Unix )
        {
            uri = "unix:///var/run/docker.sock";
        } else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            uri = "npipe://./pipe/docker_engine";
        }

        if (uri.IsNullOrEmpty())
        {
            throw new Exception("Unable to determine docker socket location");
        }
        
        var client = new DockerClientConfiguration(
                new Uri(uri))
            .CreateClient();
        return client;
    }
}