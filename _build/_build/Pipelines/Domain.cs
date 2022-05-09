namespace Build.Pipelines;

public class Domain : IDomain
{
    public Domain(string domain)
    {
        Value = domain;
    }

    public string Value { get; }
}

public interface IDomain
{
    public string Value { get; }
}