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

public class GenerateCertificateTests
{
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void GivenIngressRoutesShouldGenerateCertificates(bool https, int numberOfCerts)
    {
        var pipeline = new Pipeline()
        {
            Registry = "some.registry",
            Https = https,
            Name = "testSolution",
            Services = new List<PipelineService>()
            {
                new()
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
        var ingressRoutes = new GenerateIngressRoutesList(env).Invoke(pipeline, "foo.com");

        var sut = new GenerateCertificates();
        var certificates = sut.Invoke(ingressRoutes);
        certificates.Count().Should().Be(numberOfCerts);
    }
}