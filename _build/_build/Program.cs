// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using Build.Commands;
using Build.Docker;
using Build.Environments;
using Build.Pipelines;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Environment = Build.Environments.Environment;


namespace Build;

internal class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        new HostBuilder()
            .ConfigureAppConfiguration((_, configApp) => configApp.AddCommandLine(args))
            .ConfigureServices((_, services) =>
            {
                services.Scan(scan => scan
                    .FromAssemblyOf<App>()
                    .AddClasses()
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

                services.AddSingleton(_ =>
                    new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                        .DisableAliases()
                        .Build()
                );

                services.AddSingleton(_ =>
                    new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .IgnoreUnmatchedProperties()
                        .Build()
                );

                services.AddSingleton<ICommandArgs>(_ => new CommandCommandArgs(args));


                var rc = new RootCommand();
                var domainOption =
                    new Option<string>(new[] { "--domain" }, () => "localhost",
                        "The domain to use for the build");
                rc.AddGlobalOption(domainOption);
                var environmentOption = new Option<string>(new[] { "--environment", "-e" },
                    () => Environment.Development.ToString(),
                    "The environment to use for the build");
                rc.AddGlobalOption(environmentOption);

                var parseResult = rc.Parse(args);
                var domainValue = parseResult.CommandResult.GetValueForOption(domainOption)!;
                services.AddSingleton<IDomain>(new Domain(domainValue));

                var environmentValue = parseResult.CommandResult.GetValueForOption(environmentOption)!;
                services.AddSingleton<IEnv>(_ => new Env(Enum.Parse<Environment>(environmentValue, true)));

                services.AddSingleton(sp =>
                {
                    rc.RegisterCommands<Program>(sp);
                    return rc;
                });

                services.AddSingleton(new DockerClientFactory().Invoke());

                services.AddHostedService<App>();
            })
            .UseConsoleLifetime();
}

public class CommandCommandArgs : ICommandArgs
{
    public CommandCommandArgs(string[] args)
    {
        Args = args;
    }

    public string[] Args { get; set; }
}

public interface ICommandArgs
{
    public string[] Args { get; set; }
}