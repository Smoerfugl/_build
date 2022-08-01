using System.CommandLine;
using System.Text;
using Build.Commands;
using Octokit;
using Octokit.Internal;
using Slack.Webhooks;
using Spectre.Console;
using Emoji = Slack.Webhooks.Emoji;

namespace Build.Slack;

public class Commands : ICommands
{
    public static Argument<string> SlackWebhook = new() { Name = "Webhook url" };
    public static Argument<string> SlackChannel = new() { Name = "Slack channel" };
    public static Argument<string> Repository = new() { Name = "Repository" };
    public static Argument<int> PullRequestNumber = new() { Name = "PullRequestNumber" };

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("slack");
        command.AddArgument(SlackWebhook);
        command.AddArgument(SlackChannel);
        command.AddArgument(Repository);
        command.AddArgument(PullRequestNumber);
        command.SetHandler(async (slackWebhook, slackChannel, repositoryName, pullRequestNumber) =>
        {
            var ghToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (ghToken == null)
            {
                AnsiConsole.WriteException(new Exception("GITHUB_TOKEN not found"));
                return;
            }

            var g = new GitHubClient(new ProductHeaderValue("smoerfugl"),
                new InMemoryCredentialStore(new Credentials(ghToken)
                ));
            var repositories = await g.Repository.GetAllForCurrent();
            var repository = repositories.SingleOrDefault(d => d.Name == repositoryName);
            if (repository == null)
            {
                AnsiConsole.WriteException(new Exception("Repository not found"));
                return;
            }

            var pullRequest = await g.PullRequest.Get(repository.Owner.Login, repository.Name, pullRequestNumber);
            var pullRequestBody = GetReleaseNoteBody(pullRequest);
            if (!pullRequestBody.Contains("### Release Note"))
            {
                return;
            }

            AnsiConsole.WriteLine(pullRequestBody);

            var slackClient = new SlackClient(slackWebhook);
            var message = new SlackMessage()
            {
                Channel = slackChannel,
                IconEmoji = Emoji.Rocket,
                Markdown = true,
                Text = pullRequestBody,
                Attachments = new()
                {
                    new SlackAttachment()
                    {
                        ImageUrl = ""
                    }
                }
            };

            await slackClient.PostAsync(message);

            AnsiConsole.WriteLine(slackWebhook);
        }, SlackWebhook, SlackChannel, Repository, PullRequestNumber);
        builder.Add(command);
    }

    private string GetReleaseNoteBody(PullRequest pullRequest)
    {
        var strings = pullRequest.Body
            .Split(Environment.NewLine.ToCharArray())
            .ToList();

        var index = strings.FindIndex(s => s.Contains("### Release Note"));
        var skip = strings.Skip(index);
        return string.Join(Environment.NewLine, skip);
    }
}