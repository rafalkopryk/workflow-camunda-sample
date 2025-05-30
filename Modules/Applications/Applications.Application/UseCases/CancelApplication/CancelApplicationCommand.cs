﻿namespace Applications.Application.UseCases.CancelApplication;

public record CancelApplicationCommand(string ApplicationId);

public abstract record CancelApplicationCommandResponse
{
    public record OK(): CancelApplicationCommandResponse
    {
        public static readonly OK Result = new ();
    }

    public record ResourceNotFound() : CancelApplicationCommandResponse
    {
        public static readonly ResourceNotFound Result = new();
    }
}
