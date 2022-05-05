using System.CommandLine.Invocation;

namespace Build;

public class CommandHandler : ICommandHandler
{
    public CommandHandler()
    {
    }
    
    public Task<int> InvokeAsync(InvocationContext context)
    {
        Console.WriteLine("Hej");
        return Task.Run(() => 1);

        // var cmdLine = _rootCommand.Parse(args);
        //
        // var targetsToRun = cmdLine.CommandResult.Tokens.Select(token => token.Value).ToList();
        // if (targetsToRun.Count > 0)
        // {
        //     targetsToRun.Add("WriteToFile");
        // }
        //
        // var options = new Options(Options.Definitions.Select(d => (d.Aliases[0],
        //     cmdLine.GetValueForOption(
        //         _rootCommand.Options.OfType<Option<bool>>().Single(o => o.HasAlias(d.Aliases[0]))
        //     )))
        // );
        // var targets = new Targets();
        // AddTargets(targets, cmdLine);
        // options.Parallel = true;
        //
        // await targets.RunAndExitAsync(targetsToRun, options);
    }
}