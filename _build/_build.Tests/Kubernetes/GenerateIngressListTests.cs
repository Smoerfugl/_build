using System.Collections.Generic;
using Build.Kubernetes;
using Build.Pipeline;
using Build.Tye;
using FluentAssertions;
using Xunit;

namespace _build.Tests.Kubernetes;

public class GenerateIngressListTests
{
    [Fact]
    public void Foo()
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
        
        var sut = new GenerateIngressRoutesRoutesList().Invoke(pipeline, "foo.com");
        
        sut.Count.Should().Be(2);
    }
}