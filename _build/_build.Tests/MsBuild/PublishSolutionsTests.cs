using Build.MsBuild;
using FluentAssertions;
using Xunit;

namespace _build.Tests.MsBuild;

public class PublishSolutionsTests
{

    [Fact]
    public void ShouldPublishSolutions()
    {
        var sut = new PublishSolution();
        sut.Invoking(d => d.Invoke("_build"))
            .Should().NotThrowAsync();
    }
}