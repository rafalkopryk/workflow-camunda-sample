using MediatR;

namespace Operations.Application.UseCases.ProcessDefinitions.GetProcessDefinitionXml;

public record GetProcessDefinitionXmlQuery(long ProcessDefinitionKey) : IRequest<string>;
