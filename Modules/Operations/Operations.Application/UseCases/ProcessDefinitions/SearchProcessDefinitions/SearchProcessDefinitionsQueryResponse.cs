﻿using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;

public record SearchProcessDefinitionsQueryResponse(ProcessDefinitionDto[] Items);
