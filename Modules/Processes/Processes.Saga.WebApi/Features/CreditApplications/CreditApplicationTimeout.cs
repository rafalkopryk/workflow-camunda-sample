using JasperFx.Core;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Persistence.Sagas;

namespace Processes.Saga.WebApi.Features.CreditApplications;

[MessageIdentity("applicationTimeouted", Version = 1)]
public record CreditApplicationTimeout() : TimeoutMessage(5.Minutes())
{
    [SagaIdentity]
    public string ApplicationId { get; init; } = null!;
}
