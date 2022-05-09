using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Build.Commands;

public interface ICommandsBuilder
{
    void Add(Command command);
    public IList<Command> Commands { get; }
}

public class CommandsBuilder : ICommandsBuilder
{
    private readonly IList<Command> _commands = new List<Command>();

    public void Add(Command command)
    {
        AddCommand(command);
    }

    public IList<Command> Commands
    {
        get => _commands;
    }

    private void AddCommand(Command command)
    {
        _commands.Add(command);
    }
}

public static class CommandsExtensions
{
    public static void RegisterCommands<T>(this RootCommand rootCommand, IServiceProvider serviceCollection)
    {
        RegisterCommands(rootCommand, typeof(T).Assembly, serviceCollection);
    }

    public static void RegisterCommands(this RootCommand rootCommand, Assembly assembly,
        IServiceProvider serviceProvider)
    {
        var builder = new CommandsBuilder();

        var commands = assembly
            .GetTypes()
            .Where(d => d.IsClass)
            .Where(d => d.IsAssignableTo(typeof(ICommands)))
            .ToList();

        foreach (var command in commands)
        {
            var instance = (ICommands)ActivatorUtilities.CreateInstance(serviceProvider, command);
            instance.Register(builder);
        }

        foreach (var builderCommand in builder.Commands)
        {
            rootCommand?.AddCommand(builderCommand);
        }
    }
}