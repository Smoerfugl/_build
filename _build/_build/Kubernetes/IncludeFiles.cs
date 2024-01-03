using System.Text;
using Build.Pipelines;
using k8s;

namespace Build.Kubernetes;

public interface IIncludeFiles
{
    IEnumerable<object> Invoke(Pipeline pipeline);
}

public class IncludeFiles : IIncludeFiles
{
    public IEnumerable<object> Invoke(Pipeline pipeline)
    {
        var executionPath = Environment.CurrentDirectory;
        var includedFiles = pipeline.IncludedFiles;

        var files = includedFiles.Select(file => new FileInfo(Path.Combine(executionPath, file))).ToList();
        var contents = ReadFiles(files);

        return contents.ToList();
    }

    private static IEnumerable<object> ReadFiles(List<FileInfo> files)
    {
        foreach (var file in files)
        {
            if (!file.Exists)
            {
                throw new Exception($"File {file.FullName} does not exist");
            }

            var fileContentDirty = File.ReadAllText(file.FullName, Encoding.Default).Trim();
            var obj = KubernetesYaml.Deserialize<object>(fileContentDirty);
            yield return obj;
        }
    }
}
