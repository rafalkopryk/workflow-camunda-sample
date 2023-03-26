using Microsoft.Extensions.Options;

namespace Camunda.Connector.SDK.Runtime.Inbound.Importer.File;

internal class PathFileProvider : IBpmnProvider
{
    private readonly PathFileProviderOptions _options;

    public PathFileProvider(IOptions<PathFileProviderOptions> options)
    {
        _options = options.Value;
    }

    public async Task<byte[]> GetBpmn(ProcessDefinition processDefinition)
    {
        var pathDefinition = _options.Definitions.FirstOrDefault(x => string.Equals(x.BpmnProcessId, processDefinition.BpmnProcessId, StringComparison.OrdinalIgnoreCase));
        if (pathDefinition is null)
        {
            return Array.Empty<byte>();
        }

        var file = await System.IO.File.ReadAllBytesAsync(pathDefinition.Path);
        return file;
    }
}
