using System.Diagnostics;
using Build.MsBuild;

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
            CreateNoWindow = false,
        };
    }

    public ShellProcessBuilder WithArgument(params ShellArgument[] arguments)
    {
        _arguments.AddRange(arguments);
        return this;
    }

    public Process Build()
    {
        _startInfo.Arguments = string.Join(" ", _arguments.Select(a => a.ToString()));
        return new()
        {
            StartInfo = _startInfo,
        };
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
