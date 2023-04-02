using System.Collections.Generic;
using System.Linq;
using Build.Environments;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Tye;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace _build.Tests.Kubernetes;

public class GenerateDeploymentTests
{
    private readonly IEnv _ienv;
    private readonly IGetPipeline _getPipelineMock;
    private readonly Pipeline _pipeline;

    public GenerateDeploymentTests()
    {
        _pipeline = CreatePipeline();
        _getPipelineMock = Substitute.For<IGetPipeline>();
        _getPipelineMock.Invoke().ReturnsForAnyArgs(_pipeline);
        _ienv = Substitute.For<IEnv>();
        _ienv.Value.ReturnsForAnyArgs(Environment.Development);
    }

    [Fact]
    public void GivenPipeline_ShouldReturnDeployment()
    {
        var deployments = new GenerateDeployments(_getPipelineMock, _ienv).Invoke("latest").ToList();

        var deployment = deployments.First();
        deployment.Spec.Replicas.Should().Be(_pipeline.Services.First().Replicas);
    }

    [Fact]
    public void GivenPipeline_ReadinessProbe()
    {
        var expected = "/health";
        _pipeline.Services.First().Readiness = expected;
        
        var deployments = new GenerateDeployments(_getPipelineMock, _ienv).Invoke("latest").ToList().First();

        var pod = deployments.Spec
            .Template
            .Spec
            .Containers
            .First();
        
        pod.ReadinessProbe.HttpGet.Path
            .Should().Be(expected);
    }

    [Fact]
    public void GivenPipeline_LivelinessShouldNotExist()
    {
        var expected = "/health";
        _pipeline.Services.First().Readiness = expected;
        
        var deployments = new GenerateDeployments(_getPipelineMock, _ienv).Invoke("latest").ToList().First();

        var pod = deployments.Spec
            .Template
            .Spec
            .Containers
            .First();
        pod.LivenessProbe.Should()
            .BeNull();
    }

    private static Pipeline CreatePipeline()
    {
        var pipeline = new Pipeline()
        {
            Name = "test",
            Services = new List<PipelineService>()
            {
                new()
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
                        new("Some.Variable", "Some.Value")
                    }
                }
            }
        };
        return pipeline;
    }
}