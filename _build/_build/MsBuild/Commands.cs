using System.CommandLine;
using System.Text.Json;
using Build.Commands;
using Build.Docker;
using Build.Pipelines;
using Spectre.Console;

namespace Build.MsBuild;

public class Commands : ICommands
{
    private readonly IGetPipeline _getPipeline;
    private readonly IPublishSolution _publishSolutions;
    private readonly IBuildDockerImage _buildDockerImage;

    public Commands(IGetPipeline getPipeline, IPublishSolution publishSolutions, IBuildDockerImage buildDockerImage)
    {
        _getPipeline = getPipeline;
        this._publishSolutions = publishSolutions;
        _buildDockerImage = buildDockerImage;
    }

    public static Option<string> Tag = new(new[] { "--tag" }, () => "latest", "Tag to use for the build");

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("build")
        {
            Tag
        };

        var pipeline = _getPipeline.Invoke();
        var projects = pipeline?.Services.Select(d => d.Project).ToList();

        command.SetHandler(
            async (string tagValue) =>
            {
                await AnsiConsole.Progress()
                    .HideCompleted(false)
                    .StartAsync(async ctx =>
                    {
                        if (projects == null)
                        {
                            return;
                        }

                        var publishTasks = projects.Select(project =>
                        {
                            var t = ctx.AddTask($"Publishing {project}");
                            t.IsIndeterminate = true;
                            return _publishSolutions.Invoke(project)
                                .ContinueWith(_ => t.Value = 100);
                        });

                        await Task.WhenAll(publishTasks);

                        var buildTasks = pipeline?.Services.Select(service =>
                        {
                            var t = ctx.AddTask($"Building container {service.Name}");
                            t.IsIndeterminate = true;
                            if (string.IsNullOrWhiteSpace(service.Dockerfile))
                            {
                                t.Description = $"Skipped {service.Name}";
                                return Task.CompletedTask;
                            }

                            var tag = tagValue;
                            return _buildDockerImage
                                .Invoke(pipeline.Registry, service?.Name, service.Dockerfile, tagValue)
                                .ContinueWith(_ =>
                                {
                                    t.Value = 100;
                                    t.Description = $"Built {pipeline.Registry}{service.Name}:{tag}";
                                });
                        });

                        if (buildTasks != null)
                        {
                            await Task.WhenAll(buildTasks);
                        }
                    });
            }, Tag);

        builder.Add(command);
    }
}