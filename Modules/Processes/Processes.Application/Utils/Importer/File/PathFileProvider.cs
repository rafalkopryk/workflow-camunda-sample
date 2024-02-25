using Microsoft.Extensions.Options;
using Processes.Application.Utils;

namespace Processes.Application.Utils.Importer.File;

internal class PathFileProvider(IOptions<PathFileProviderOptions> options) : IBpmnProvider
{
    private readonly PathFileProviderOptions _options = options.Value;

    public async Task<byte[]> GetBpmn(ProcessDefinition processDefinition)
    {
        var pathDefinition = _options.Definitions.FirstOrDefault(x => string.Equals(x.BpmnProcessId, processDefinition.BpmnProcessId, StringComparison.OrdinalIgnoreCase));
        if (pathDefinition is null)
        {
            return [];
        }

        var file = await System.IO.File.ReadAllBytesAsync(pathDefinition.Path);
        return file;
    }
}
