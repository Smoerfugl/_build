using System.Diagnostics;

namespace Build.ShellBuilder;

public class ShellProcessBuilder
{
    private readonly List<ShellArgument> _arguments = new();
    private readonly ProcessStartInfo _startInfo;

    public ShellProcessBuilder(string command)
    {
        _startInfo = new ProcessStartInfo(command)
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
    }

    public ShellProcessBuilder WithArgument(params ShellArgument[] arguments)
    {
        _arguments.AddRange(arguments);
        return this;
    }

    public async Task<int> Run(CancellationToken cancellationToken)
    {
        var process = Build();
        Console.WriteLine($"Running command {process.StartInfo.FileName} {process.StartInfo.Arguments}");
        process.OutputDataReceived += (_, args) => Console.WriteLine("- [✓] : {0}", args.Data);
        process.ErrorDataReceived += (_, args) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("- [x]: {0}", args.Data);
            Console.ResetColor();
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    public Process Build()
    {
        _startInfo.Arguments = string.Join(" ", _arguments.Select(a => a.ToString()));
        
        var process = new Process()
        {
            StartInfo = _startInfo,
        };

        return process;
    }
}
public class ShellArgument
{
    public string Argument { get; }

    public ShellArgument(string argument)
    {
        Argument = argument;
    }
    
    public static implicit operator ShellArgument(string argument)
    {
        return new ShellArgument(argument);
    }
    
    public override string ToString()
    {
        return Argument;
    }
}
