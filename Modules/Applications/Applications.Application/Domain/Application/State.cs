﻿using Common.Application.BusinessRule;
using Common.Application.Dictionary;

namespace Applications.Application.Domain.Application;

public record State
{
    public const string APPLICATION_REGISTERED = nameof(APPLICATION_REGISTERED);
    public const string DECISION_GENERATED = nameof(DECISION_GENERATED);
    public const string CONTRACT_SIGNED = nameof(CONTRACT_SIGNED);
    public const string APPLICATION_CLOSED = nameof(APPLICATION_CLOSED);
    
    public string Level { get; protected init; }

    public DateTimeOffset Date { get; protected init; }

    public DateTimeOffset? ContractSigningDate { get; protected init; }

    public Decision? Decision { get; protected init; }

    protected State() { }

    public static State ApplicationRegistered(DateTimeOffset date)
    {
        return new State
        {
            Level = APPLICATION_REGISTERED,
            Date = date,
        };
    }

    public static State DecisionGenerated(State currentState, Decision decision, DateTimeOffset date)
    {
        return currentState with
        {
            Level = DECISION_GENERATED,
            Date = date,
            Decision = decision,
        };
    }

    public static State ApplicationClosed(State currentState, DateTimeOffset date)
    {
        return currentState with
        {
            Level = APPLICATION_CLOSED,
            Date = date,
        };
    }

    public static State ContractSigned(State currentState, DateTimeOffset date)
    {
        CheckRule(new LevelShouldBeDecisionGenerated(currentState));
        CheckRule(new DecisionShouldBePositive(currentState));

        return currentState with
        {
            Level = CONTRACT_SIGNED,
            Date = date,
            ContractSigningDate = date,
        };
    }

    private static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }

    internal class DecisionShouldBePositive : IBusinessRule
    {
        private readonly State _state;

        public DecisionShouldBePositive(State state)
        {
            _state = state;
        }

        public string ErrorCode => "DECISION_SHOULD_BE_POSITIVE";

        public bool IsBroken()
        {
            return _state.Decision != Common.Application.Dictionary.Decision.Positive;
        }
    }

    internal class LevelShouldBeDecisionGenerated : IBusinessRule
    {
        private readonly State _state;

        public LevelShouldBeDecisionGenerated(State state)
        {
            _state = state;
        }

        public string ErrorCode => "LEVEL_SHOULD_BE_DECISION_GENERATED";

        public bool IsBroken()
        {
            return _state.Level != State.DECISION_GENERATED;
        }
    }
}