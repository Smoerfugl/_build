using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Build.Docker;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace _build.Tests.Docker;

public class BuildImageTests
{
    private readonly ImageBuilder _imageBuilder;

    public BuildImageTests(ITestOutputHelper output)
    {
        _imageBuilder = new ImageBuilder();
        WriteDockerFile();
    }

    [Fact(Timeout = 100000)]
    public async Task ImageBuilder_GivenDockerFile_ShouldBuildImage()
    {
        var imageBuilderParams = new ImageBuilderParams("helo", "latest", "Dockerfile");
        var action = async () => await _imageBuilder.BuildImage(imageBuilderParams, new CancellationToken());

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ImageBuilder_GivenDockerFileWithBuildArguments_ShouldBuildImage_And_PushImage()
    {
        var imageBuilderParams = new ImageBuilderParams("helo", "latest", "Dockerfile")
        {
            BuildArgs = new() { "PROJECT=test.dll" }
        };

        var action = async () => await _imageBuilder.BuildImage(imageBuilderParams, new CancellationToken());

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
