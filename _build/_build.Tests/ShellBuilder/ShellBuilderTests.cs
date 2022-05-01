using System;
using System.Threading.Tasks;
using _build.Tests.Docker;
using Build.ShellBuilder;
using Xunit;
using Xunit.Abstractions;

namespace _build.Tests.ShellBuilder;

public class ShellBuilderTests
{
    private readonly ITestOutputHelper _output;

    public ShellBuilderTests(ITestOutputHelper output)
    {
        _output = output;
        var converter = new Converter(output);
        Console.SetOut(converter);
    }
    
    [Fact]
    public async Task GivenEcho_ShouldOutputToConsole()
    {
        await new ShellProcessBuilder("echo")
            .WithArgument("Hello World")
            .Run();
    }
}