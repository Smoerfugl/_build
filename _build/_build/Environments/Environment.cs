namespace Build.Environments;

public interface IEnv
{
    Environment Value { get; }
}
public class Env : IEnv
{
    public Env(Environment env)
    {
        Value = env;
    }

    public Environment Value { get; }
    public override string ToString() => Value.ToString();
}
public enum Environment
{
    Development,
    Staging,
    Production
}