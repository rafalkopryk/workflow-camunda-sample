using CSharpFunctionalExtensions;
using MediatR;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;

public record SearchProcessDefinitionsQuery(ProcessDefinitionDto? Filter, long[]? SearchAfter, int? Size = 50) : IRequest<Result<SearchProcessDefinitionsQueryResponse>>;
