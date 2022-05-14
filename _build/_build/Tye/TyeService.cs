namespace Build.Tye;

public class TyeService
{
    public TyeService()
    {
    }

    public TyeService(string name, string project)
    {
        Name = name;
        Project = project;
    }

    private string _name = "";

    public string Name
    {
        get => _name;
        set => _name = value.ToLower();
    }

    public string Project { get; set; } = null!;
    public int? Replicas { get; set; } = 1;
    public List<EnvironmentVariable> Env { get; set; } = new();
}