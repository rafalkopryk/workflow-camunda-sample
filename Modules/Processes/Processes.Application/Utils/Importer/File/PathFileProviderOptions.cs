﻿namespace Processes.Application.Utils.Importer.File;

public record PathFileProviderOptions
{
    public PathDefinitions[] Definitions { get; set; } = Array.Empty<PathDefinitions>();

    public record PathDefinitions(string BpmnProcessId, string Path);
}