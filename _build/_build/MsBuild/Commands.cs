using System.CommandLine;
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
    private readonly IImagePusher _imagePusher;

    public Commands(IGetPipeline getPipeline, IPublishSolution publishSolutions, IBuildDockerImage buildDockerImage,
        IImagePusher imagePusher)
    {
        _getPipeline = getPipeline;
        _publishSolutions = publishSolutions;
        _buildDockerImage = buildDockerImage;
        _imagePusher = imagePusher;
    }

    public static Option<string> Tag = new(new[] { "--tag" }, () => "latest", "Tag to use for the build");
    public static Option<bool> Push = new(new[] { "--push" }, () => false, "Should push to registry");

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("build")
        {
            Tag,
            Push
        };

        command.SetHandler(
            async (context) =>
            {
                var tagValue = context.ParseResult.GetValueForOption(Tag);
                var shouldPush = context.ParseResult.GetValueForOption(Push);
                var cancellationToken = context.GetCancellationToken();
                
                var pipeline = _getPipeline.Invoke();
                var projects = pipeline.Services.Select(d => d.Project).ToList();
                await AnsiConsole.Progress()
                    .AutoClear(false)
                    .AutoRefresh(false)
                    .HideCompleted(false)
                    .StartAsync(async ctx =>
                    {
                        var publishTasks = projects.Select(project =>
                        {
                            var t = ctx.AddTask($"Publishing {project}");
                            t.IsIndeterminate = true;
                            return _publishSolutions.Invoke(project, cancellationToken);
                        });

                        await Task.WhenAll(publishTasks);

                        var buildTasks = pipeline.Services.Select(service =>
                        {
                            var t = ctx.AddTask(
                                $"Building {pipeline.Registry.ToLower()}/{service.Name.ToLower()}:{tagValue}");
                            t.IsIndeterminate = true;
                            if (string.IsNullOrWhiteSpace(service.Dockerfile))
                            {
                                t.Description = $"Skipped {service.Name}";
                                return null;
                            }

                            var imageBuild = _buildDockerImage
                                .Invoke(pipeline.Registry, service.Project, service.Dockerfile, tagValue, cancellationToken);

                            imageBuild.ContinueWith(_ =>
                            {
                                t.Value = 100;
                                t.Description =
                                    $"Built {pipeline.Registry.ToLower()}/{service.Project.ToLower()}:{tagValue}";
                            });
                            return imageBuild;
                        });

                        var res = await Task.WhenAll(buildTasks!);
                        var images = res.Where(d => d != null);
                        if (shouldPush)
                        {
                            var pushTasks = images.Select(image =>
                            {
                                var t = ctx.AddTask($"Pushing {image?.Name}");
                                t.IsIndeterminate = true;
                                var task = _imagePusher.Invoke(image!, cancellationToken);
                                task.ContinueWith(_ =>
                                {
                                    t.Value = 100;
                                    t.Description = $"Pushed {image?.Name}";
                                });
                                return task;
                            });

                            await Task.WhenAll(pushTasks);
                        }
                    });
            });

        builder.Add(command);
    }
}