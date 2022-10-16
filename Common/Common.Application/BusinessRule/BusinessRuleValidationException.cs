namespace Common.Application.BusinessRule;

public class BusinessRuleValidationException : Exception
{
    public BusinessRuleValidationException(IBusinessRule businessRule)
    {
        ErrorCode = businessRule.ErrorCode;
    }

    public string ErrorCode { get; set; }
}
