using System.Collections.Generic;
using System.Linq;
using Build.Environments;
using Build.Kubernetes;
using Build.Pipelines;
using FluentAssertions;
using Xunit;

namespace _build.Tests.Kubernetes;

public class GenerateDeploymentTests
{
    [Fact]
    public void GivenPipeline_ShouldReturnDeployment()
    {
        var pipeline = new Pipeline()
        {
            Name = "test",
            Services = new List<PipelineService>()
            {
                new PipelineService()
                {
                    Name = "Some.Service",
                    Hostname = "Some.Service.com",
                    Project = "Some.Project",
                    Replicas = 2,
                    Resources = new ServiceResources()
                    {
                        Limits = new Dictionary<ResourceUnits, string>()
                        {
                            { ResourceUnits.CPU, "1" },
                            { ResourceUnits.Memory, "2Gi" }
                        },
                        Requests = new Dictionary<ResourceUnits, string>()
                        {
                            { ResourceUnits.CPU, "1" },
                            { ResourceUnits.Memory, "2Gi" }
                        }
                    },
                    ServicePort = 80,
                }
            },
            Registry = "docker.io",
            EnvironmentVariables = new Dictionary<string, List<EnvironmentVariable>>()
            {
                {
                    "staging", new List<EnvironmentVariable>()
                    {
                        new EnvironmentVariable("Some.Variable", "Some.Value")
                    }
                }
            }
        };
        var deployments = new GenerateDeployments().Invoke(pipeline, Env.Staging).ToList();

        deployments.Should().NotBeNullOrEmpty();
        deployments.Count.Should().Be(1);
        deployments.First().Spec.Replicas.Should().Be(pipeline.Services.First().Replicas);
    }
}