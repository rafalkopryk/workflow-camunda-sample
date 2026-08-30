using Wolverine.Attributes;

namespace Applications.Application.Features.CloseApplication;

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);