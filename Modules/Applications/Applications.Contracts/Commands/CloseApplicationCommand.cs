using Wolverine.Attributes;

namespace Applications.Contracts.Commands;

[MessageIdentity("close", Version = 1)]
public record CloseApplicationCommand(string ApplicationId);