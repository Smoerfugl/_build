using Build.Kubernetes.Builders;
using FluentAssertions;
using Xunit;

namespace _build.Tests.Kubernetes.Builders;

public class DeploymentBuilderTests
{
    [Fact]
    public void GivenValues_ShouldReturnDeployment()
    {
        var deployment = new DeploymentBuilder()
            .WithNamespace("default")
            .WithName("Some")
            .WithSpec(new DeploymentSpecBuilder().Build())
            .Build();

        deployment.Should().NotBeNull();
    }
}