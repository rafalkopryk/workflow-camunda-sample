namespace Common.Application.BusinessRule;

public interface IBusinessRule
{
    string ErrorCode { get; }

    public bool IsBroken();
}