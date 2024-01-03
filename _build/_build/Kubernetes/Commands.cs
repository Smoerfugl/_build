using System.CommandLine;
using System.CommandLine.Invocation;
using Build.Commands;
using Build.Pipelines;
using Spectre.Console;

namespace Build.Kubernetes;

public class Commands : ICommands
{
    private readonly IGenerateIngressRoutesList _generateIngressRoutesList;
    private readonly IGenerateCertificates _generateCertificates;
    private readonly IGenerateNamespace _generateNamespace;
    private readonly IGetPipeline _getPipeline;
    private readonly IDomain _domain;
    private readonly IGenerateDeployments _generateDeployments;
    private readonly IGenerateServices _generateServices;
    private readonly IKubernetesConfigRepository _kubernetesConfigRepository;
    private readonly IIncludeFiles _includeFiles;

    public Commands(IGenerateIngressRoutesList generateIngressRoutesList,
        IGenerateCertificates generateCertificates,
        IGenerateNamespace generateNamespace,
        IGetPipeline getPipeline,
        IDomain domain,
        IGenerateDeployments generateDeployments,
        IGenerateServices generateServices,
        IKubernetesConfigRepository kubernetesConfigRepository,
        IIncludeFiles includeFiles
    )
    {
        _generateIngressRoutesList = generateIngressRoutesList;
        _generateCertificates = generateCertificates;
        _generateNamespace = generateNamespace;
        _getPipeline = getPipeline;
        _domain = domain;
        _generateDeployments = generateDeployments;
        _generateServices = generateServices;
        _kubernetesConfigRepository = kubernetesConfigRepository;
        _includeFiles = includeFiles;
    }

    public static Option<string> Tag = new(new[] { "--tag" }, "Tag to use for the build");

    public static Option<string> File = new(new[] { "-f", "--file" }, "Output to file");
    // public static Option<bool> GenerateIngressRoutes =
    //     new(new[] { "ingress", "-i" }, () => false, "Generate ingress routes");

    private Task<T> TaskRunner<T>(ProgressContext p, Func<T> a, string name)
    {
        var t1 = p.AddTask(name);
        var d = a();
        t1.Value = 100;

        return Task.FromResult(d);
    }

    public void Register(CommandsBuilder builder)
    {
        var command = new Command("kubernetes", "Kubernetes related")
        {
            Tag
        };

        var ingressCommand = new Command("ingress")
        {
            Tag,
            File
        };

        ingressCommand.SetHandler(
            async context =>
            {
                var tagValue = context.ParseResult.GetValueForOption(Tag);
                var pipeline = _getPipeline.Invoke() ?? throw new Exception("Missing Pipeline.yml");
                var domain = _domain.Value ?? throw new Exception("Cannot generate ingress without domain");

                await AnsiConsole.Progress()
                    .StartAsync(async ctx =>
                    {
                        var ingressRoutes = await TaskRunner(ctx,
                            () => _generateIngressRoutesList.Invoke(pipeline, domain),
                            "Generating ingress routes");
                        var certificates = await TaskRunner(ctx, () => _generateCertificates.Invoke(ingressRoutes),
                            "Generating Certificates");
                        var @namespace = await TaskRunner(ctx, () => _generateNamespace.Invoke(),
                            "Generating namespace");
                        var deployments =
                            await TaskRunner(ctx, () => _generateDeployments.Invoke(tagValue), "Generate deployments");
                        var services = await TaskRunner(ctx, () => _generateServices.Invoke(deployments),
                            "Generating services");
                        var includedFiles = await TaskRunner(ctx, () => _includeFiles.Invoke(pipeline), "Include manifests");

                        _kubernetesConfigRepository.AddToManifesto(
                            ingressRoutes,
                            certificates,
                            @namespace,
                            deployments,
                            services,
                            includedFiles
                        );

                        var file = context.ParseResult.GetValueForOption(File);
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            await TaskRunner(ctx, async () => await _kubernetesConfigRepository.WriteToFile(file), "Saving to file");
                        }
                    });
            });


        command.AddCommand(ingressCommand);

        builder.Add(command);
    }
}