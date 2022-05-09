using System.CommandLine;
using Build.Commands;
using Build.Pipelines;
using Spectre.Console;

namespace Build.MsBuild;

public class Commands : ICommands
{
    private readonly IGetPipeline _getPipeline;
    private readonly IPublishSolution _publishSolutions;

    public Commands(IGetPipeline getPipeline, IPublishSolution publishSolutions)
    {
        _getPipeline = getPipeline;
        this._publishSolutions = publishSolutions;
    }

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("Build");

        var pipeline = _getPipeline.Invoke();
        var projects = pipeline?.Services.Select(d => d.Project).ToList();

        command.SetHandler(() =>
        {
            AnsiConsole.Progress()
                .HideCompleted(false)
                .StartAsync(ctx =>
                {
                    async void Action(string d)
                    {
                        var task = ctx.AddTask($"Building {d}");

                        while (!ctx.IsFinished)
                        {
                            await _publishSolutions.Invoke(d);
                            ctx.Refresh();
                            task.Increment(1);
                        }
                    }

                    projects.AsParallel().ForAll
                    (Action);
                    return Task.CompletedTask;
                });


            // projects?.AsParallel().ForAll(d =>
            //     {
            //         AnsiConsole
            //             .Status()
            //             .Start($"Building {d}", ctx =>
            //             {
            //                 _publishSolutions.Invoke(d);
            //             });
            //     }
            // );
        });

        builder.Add(command);
    }
}