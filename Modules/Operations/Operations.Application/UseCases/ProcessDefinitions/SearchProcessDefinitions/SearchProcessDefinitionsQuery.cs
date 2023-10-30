using CSharpFunctionalExtensions;
using MediatR;
using Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

namespace Operations.Application.UseCases.ProcessDefinitions.SearchProcessDefinitions;

public record SearchProcessDefinitionsQuery(ProcessDefinitionDto? Filter) : IRequest<Result<SearchProcessDefinitionsQueryResponse>>;
