using System.CommandLine;
using Build.Commands;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace Build.Tye;

public class Commands : ICommands
{
    private readonly IGenerateTyeYaml _generateTyeYaml;
    private readonly ISerializer _serializer;

    public Commands(IGenerateTyeYaml generateTyeYaml, ISerializer serializer)
    {
        _generateTyeYaml = generateTyeYaml;
        _serializer = serializer;
    }
    
    public void Register(CommandsBuilder builder)
    {
        var command = new Command("tye", "Tye command line interface");
        
        command.SetHandler(
           () =>
           {
               AnsiConsole
                   .Progress()
                   .Start(ctx =>
                   {
                       var task = ctx.AddTask("Generating tye");
                       task.IsIndeterminate = true;
                       var tyeObject = _generateTyeYaml.Invoke();
                       task.Value = 100;
                       var yaml = _serializer.Serialize(tyeObject);
                       var writeTask = ctx.AddTask("Saving to tye.yaml");
                       writeTask.IsIndeterminate = true;
                       File.WriteAllText("tye.yaml", yaml);
                       writeTask.Value = 100;
                       writeTask.Description = "Saved to tye.yaml";
                   });
           });
        
        builder.Add(command);
    }
}