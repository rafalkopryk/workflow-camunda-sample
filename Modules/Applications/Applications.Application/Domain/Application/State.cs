using Common.Application.Dictionary;

namespace Applications.Application.Domain.Application;

public record ApplicationState(ApplicationLevel Level, DateTimeOffset Date, Decision Decision);

public static class ApplicationStateExtensions
{
    extension(List<ApplicationState> history)
    {
        public ApplicationState? Current => history.OrderByDescending(x => x.Date).FirstOrDefault();

        public List<ApplicationState> Append(ApplicationState state)
        {
            return [.. history, state];
        }

        public DateTimeOffset? ContractSigningDate => history.FirstOrDefault(x => x.Level == ApplicationLevel.ContractSigned)?.Date;
    }
    
    extension(ApplicationState)
    {
        public static ApplicationState ApplicationRegistered(DateTimeOffset date) => new ApplicationState(ApplicationLevel.ApplicationRegistered, date, Decision.NotExists);

        public static ApplicationState DecisionGenerated(DateTimeOffset date, Decision decision)  => new ApplicationState(ApplicationLevel.DecisionGenerated, date, decision);

        public static ApplicationState ApplicationClosed(DateTimeOffset date, Decision decision) => new ApplicationState(ApplicationLevel.ApplicationClosed, date, decision);

        public static ApplicationState ContractSigned(DateTimeOffset date) => new (ApplicationLevel.ContractSigned, date, Decision.Positive);
    }
}