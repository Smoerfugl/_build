// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using Build.Kubernetes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


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

                services.AddSingleton(sp =>
                    new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                        .DisableAliases()
                        .Build()
                );

                services.AddSingleton(sp =>
                    new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build()
                );

                // services.AddScoped<ICommandHandler, CommandHandler>();

                services.AddSingleton<ICommandArgs>(sp => new CommandCommandArgs { Args = args });


                var rc = new RootCommand();
                rc.AddGlobalOption(
                    new Option<string>(new[] { "--domain" }, () => "localhost",
                        "The domain to use for the build")
                );
                services.AddSingleton(sp =>
                {
                    rc.RegisterCommands<Program>(sp);
                    return rc;
                });

                services.AddHostedService<App>();
            })
            .UseConsoleLifetime();
}

public class CommandCommandArgs : ICommandArgs
{
    public string[] Args { get; set; }
}

public interface ICommandArgs
{
    public string[] Args { get; set; }
}