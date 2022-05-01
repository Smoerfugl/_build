using System;
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
    public void GivenEcho_ShouldOutputToConsole()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }
        new ShellProcessBuilder("echo")
            .WithArgument("Hello World")
            .Run();
    }
}