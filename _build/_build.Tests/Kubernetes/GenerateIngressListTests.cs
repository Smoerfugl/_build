using System.Collections.Generic;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace _build.Tests.Kubernetes;

public class GenerateIngressListTests
{
    [Fact]
    public void GenerateIngressRutesList_GivenPipelineObject_ShouldReturnTwoIngressRoutes()
    {
        var pipeline = new Pipeline()
        {
            Registry = "some.registry",
            Name = "testSolution",
            Services = new List<PipelineService>()
            {
                new PipelineService()
                {
                    Project = "some-project",
                    Replicas = 2,
                    Name = "SomeProject",
                    Hostname = "foo.bar.com",
                    ServicePort = 80,
                }
            },
            EnvironmentVariables = new Dictionary<string, List<EnvironmentVariable>>()
        };

        var ienv = Substitute.For<IEnv>();
        ienv.Value.ReturnsForAnyArgs(Environment.Development);
        var sut = new GenerateIngressRoutesList(ienv).Invoke(pipeline, "foo.com");
        
        sut.Count.Should().Be(2);
    }
}