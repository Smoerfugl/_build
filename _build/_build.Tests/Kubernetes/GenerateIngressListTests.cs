using System.Collections.Generic;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using Build.Tye;
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
            Https = true,
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
    
    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public void GivenHttps_ShouldReturnRoutes(bool https, int count)
    {
        var pipeline = new Pipeline()
        {
            Registry = "some.registry",
            Https = https,
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

        var env = Substitute.For<IEnv>();
        env.Value.ReturnsForAnyArgs(Environment.Development);
        var sut = new GenerateIngressRoutesList(env).Invoke(pipeline, "foo.com");
        
        sut.Count.Should().Be(count);
    }
    
}