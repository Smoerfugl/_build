using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Build;

public class App : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;

    private readonly RootCommand _rootCommand;

    private readonly ICommandArgs _commandArgs;

    public App(IHostApplicationLifetime applicationLifetime, RootCommand rootCommand, ICommandArgs commandArgs)
    {
        _applicationLifetime = applicationLifetime;
        _rootCommand = rootCommand;
        _commandArgs = commandArgs;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Write(
            new FigletText("Smørfugl._build")
                .LeftAligned()
                .Color(Color.Green));
        await _rootCommand.InvokeAsync(_commandArgs.Args);
        _applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}