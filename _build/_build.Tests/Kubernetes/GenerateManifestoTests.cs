using System.Collections.Generic;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using FluentAssertions;
using Xunit;

namespace _build.Tests.Kubernetes;

public class GenerateManifestoTests
{
    [Fact]
    public void GenerateManifesto_ReturnsManifesto()
    {
        var pipeline = new Pipeline()
        {
            Registry = "some.registry",
            Name = "some-project",
            Services = new List<PipelineService>()
            {
                new PipelineService()
                {
                    Project = "some-project",
                    Replicas = 1,
                    ServicePort = 80,
                    Name = "Api",
                    Hostname = "api.some-project.svc.cluster.local",
                }
            }
        };
        var manifesto = new GenerateManifesto().Invoke(pipeline, Env.Development, "test");

        manifesto.Should().NotBeNullOrEmpty();
    }
}