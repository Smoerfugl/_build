namespace Build.Tye;

public class TyeConfigGenerator
{
    public string? Name { get; set; }
    public string? Registry { get; set; }
    public string Namespace { get; set; } = null!;
    public List<TyeService> Services { get; set; } = new();
}