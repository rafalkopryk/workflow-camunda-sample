﻿using CSharpFunctionalExtensions;
using MediatR;
using Operations.Application.UseCases.FlowNodeInstances.SearchFlowNodeInstances.Dto;

namespace Operations.Application.UseCases.FlowNodeInstances.SearchFlowNodeInstances;

public record SearchFlowNodeInstancesQuery(SearchFlowNodeInstancesFlowNodeInstanceDto Filter) : IRequest<Result<SearchFlowNodeInstancesQueryResponse>>;
