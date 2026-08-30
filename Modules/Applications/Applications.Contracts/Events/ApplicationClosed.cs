using Common.Application.Dictionary;
using Wolverine.Attributes;

namespace Applications.Contracts.Events;

[MessageIdentity("applicationClosed", Version = 1)]
public record ApplicationClosed(string ApplicationId, ApplicationCloseReason Reason);