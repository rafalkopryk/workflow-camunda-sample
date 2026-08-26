using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Close;

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId);
