namespace Build.Environments;

public class EnvironmentVariable
{
    public EnvironmentVariable()
    {
        
    }
    public EnvironmentVariable(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; set; } = null!;
    public object? Value { get; set; }
}