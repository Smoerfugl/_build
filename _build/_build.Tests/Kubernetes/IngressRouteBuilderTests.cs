using System.Linq;
using System.Text.Json;
using Build.Kubernetes;
using Build.Kubernetes.Builders;
using Build.Yaml;
using FluentAssertions;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _build.Tests.Kubernetes;

public class IngressRouteBuilderTests
{
    private readonly IngressRoute _sut;

    public IngressRouteBuilderTests()
    {
        _sut = new IngressRouteBuilder()
            .WithName("foo")
            .WithNamespace("test")
            .WithHostname("test.com")
            .WithEntrypoint(Entrypoint.Web)
            .WithServiceName("foo")
            .WithPort(80)
            .WithMiddlewares(new []{IngressRouteMiddleware.RedirectHttps()})
            .Build();
    }
    [Fact]
    public void IngressRouteBuilder_Match_ShouldBeTraefikCompliant()
    {
        _sut.Should().NotBeNull();
        _sut.Spec.Routes.First().Match.Should().Be($"Host(`test.com`)");
    }

    [Fact]
    public void IngressRoute_GivenSerializedYaml_ShouldMatchExistingYaml()
    {

        var yaml = KubernetesYaml.Serialize(_sut);
        var expected = @"apiVersion: traefik.containo.us/v1alpha1
kind: IngressRoute
metadata:
  labels:
    app.kubernetes.io/name: foo
    app.kubernetes.io/part-of: test
  name: foo
  namespace: test
spec:
  entrypoints:
  - web
  routes:
  - match: Host(`test.com`)
    kind: Rule
    services:
      - name: foo
        port: 80
    middlewares:
      - name: redirect-https
        namespace: default";

        yaml
            .Replace(" ", "").Replace("\n", "").Replace("\t","")
            .Should()
            .Be(
                expected.Replace(" ", "").Replace("\n", "").Replace("\t","")
            );

    }
}