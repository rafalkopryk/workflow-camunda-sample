using Operations.Application.UseCases.FlowNodeInstances.Shared.Dto;

namespace Operations.Application.UseCases.FlowNodeInstances.SearchFlowNodeInstances;

public record SearchFlowNodeInstancesQueryResponse(FlowNodeInstanceDto[] Items);
