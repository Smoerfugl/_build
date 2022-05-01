using System;
using System.IO;
using System.Threading.Tasks;
using Build.Docker;
using Docker.DotNet;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace _build.Tests.Docker;

public class BuildImageTests
{
    private readonly ITestOutputHelper _output;
    private readonly DockerClient _client;
    private readonly ImageBuilder _imageBuilder;

    public BuildImageTests(ITestOutputHelper output)
    {
        _output = output;
        var converter = new Converter(output);
        Console.SetOut(converter);
        _client = new DockerClientFactory().Invoke();
        _imageBuilder = new ImageBuilder(_client);
        WriteDockerFile();
    }

    [Fact(Timeout = 100000)]
    public async Task ImageBuilder_GivenDockerFile_ShouldBuildImage()
    {
        var imageBuilderParams = new ImageBuilderParams("helo", "latest", "Dockerfile");
        var action = async () => await _imageBuilder.BuildImage(imageBuilderParams);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ImageBuilder_GivenDockerFileWithBuildArguments_ShouldBuildImage_And_PushImage()
    {
        var imageBuilderParams = new ImageBuilderParams("helo", "latest", "Dockerfile")
        {
            BuildArgs = new() { "PROJECT=test.dll" }
        };

        var action = async () => await _imageBuilder.BuildImage(imageBuilderParams);

        await action.Should().NotThrowAsync();
    }

    private void WriteDockerFile()
    {
        var dockerDockerfileFromScratchAddHelloCmdHello = @"#syntax=docker/dockerfile:1
FROM scratch
CMD [""/hello""]";
        File.WriteAllText("Dockerfile", dockerDockerfileFromScratchAddHelloCmdHello);
    }
}