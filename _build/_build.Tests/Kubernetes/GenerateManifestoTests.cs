using System.Collections.Generic;
using AutoMapper.Configuration.Annotations;
using Build.Kubernetes;
using Build.Pipelines;
using Build.Environments;
using FluentAssertions;
using NSubstitute;
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

        var pipelineMock = Substitute.For<IGetPipeline>();
        pipelineMock.Invoke().ReturnsForAnyArgs(pipeline);
        var env = Substitute.For<IEnv>();
            env.Value.ReturnsForAnyArgs(Environment.Development);

        var generateNamespace = new GenerateNamespace(pipelineMock, env);
        var generateCertificates = new GenerateCertificates();
        var generateIngressRoutes = new GenerateIngressRoutesList(env);
        var domain = new Domain("some-project.com");
        var manifesto = new GenerateManifesto(generateNamespace, generateCertificates, generateIngressRoutes, domain).Invoke(pipeline);

        manifesto.Should().NotBeNullOrEmpty();
    }
}