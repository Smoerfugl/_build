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
        var command = new Command("build");

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

                        // while (!ctx.IsFinished)
                        // {
                        var t = _publishSolutions.Invoke(d);
                        var done = false;
#pragma warning disable CS4014
                        Task.Run(() => t)
                            .ContinueWith(_ =>
#pragma warning restore CS4014
                            {
                                done = true;
                                task.Value = 100;
                            });
                        task.Increment(1);
                        while (!done)
                        {
                            task.Increment(1);
                            Console.WriteLine("incremeted");
                            await Task.Delay(400);
                        }
                    }

                    projects.AsParallel().ForAll
                        (Action);
                    return Task.CompletedTask;
                });
        });

        builder.Add(command);
    }
}