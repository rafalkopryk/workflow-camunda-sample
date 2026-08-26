using Wolverine.Attributes;

namespace Processes.Operaton.WebApi.Features.CreditApplications.Close;

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);
