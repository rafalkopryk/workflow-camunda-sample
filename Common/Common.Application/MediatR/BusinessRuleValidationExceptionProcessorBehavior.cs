using Common.Application.BusinessRule;
using CSharpFunctionalExtensions;
using MediatR;

namespace Common.Application.MediatR;
public class BusinessRuleValidationExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
	where TResponse : IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
		try
		{
			return await next();
        }
		catch (BusinessRuleValidationException ex)
		{
            if (!typeof(TResponse).IsAssignableTo(typeof(IResult))) throw;

            IResult result = Result.Failure(ex.ErrorCode);
            return (TResponse)result;
        }
    }
}
