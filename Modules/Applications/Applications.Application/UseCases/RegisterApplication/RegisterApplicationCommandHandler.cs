using Applications.Application.Domain.Application;
using Applications.Application.Infrastructure.Database;
using Common.Application.Errors;
using Common.Application.Serializer;
using CSharpFunctionalExtensions;
using GatewayProtocol;
using MediatR;
using System.Text.Json;

namespace Applications.Application.UseCases.RegisterApplication;

internal class RegisterApplicationCommandHandler : IRequestHandler<RegisterApplicationCommand, Result>
{
    private readonly Gateway.GatewayClient _client;
    private readonly CreditApplicationDbContext _creditApplicationDbContext;

    public RegisterApplicationCommandHandler(Gateway.GatewayClient client, CreditApplicationDbContext creditApplicationDbContext)
    {
        _client = client;
        _creditApplicationDbContext = creditApplicationDbContext;
    }

    public async Task<Result> Handle(RegisterApplicationCommand command, CancellationToken cancellationToken)
    {
        if (_creditApplicationDbContext.Applications.Any(application => application.ApplicationId == command.ApplicationId))
        {
            return Result.Failure(ErrorCode.ResourceExists);
        }

        var creditApplication = CreateCreditApplication(command);

        await _creditApplicationDbContext.AddAsync(creditApplication, cancellationToken);
        await _creditApplicationDbContext.SaveChangesAsync(cancellationToken);

        await _client.CreateProcessInstanceAsync(new CreateProcessInstanceRequest
        {
            BpmnProcessId = "credit-application",
            Variables = JsonSerializer.Serialize(
                new
                {
                    creditApplication.ApplicationId,
                    creditApplication.Amount,
                    creditApplication.CreditPeriodInMonths,
                    creditApplication.Declaration.AverageNetMonthlyIncome,
                }, JsonSerializerCustomOptions.CamelCase),
            Version = -1,
        });

        return Result.Success();
    }

    private static CreditApplication CreateCreditApplication(RegisterApplicationCommand request)
    {
        return CreditApplication.Create(
            request.ApplicationId,
            request.CreditApplication.Amount,
            request.CreditApplication.CreditPeriodInMonths,
            new CustomerPersonalData
            {
                FirstName = request.CreditApplication.CustomerPersonalData.FirstName,
                LastName = request.CreditApplication.CustomerPersonalData.LastName,
                Pesel = request.CreditApplication.CustomerPersonalData.Pesel,
            },
            new Declaration
            {
                AverageNetMonthlyIncome = request.CreditApplication.Declaration.AverageNetMonthlyIncome
            },
            DateTimeOffset.Now);
    }
}
