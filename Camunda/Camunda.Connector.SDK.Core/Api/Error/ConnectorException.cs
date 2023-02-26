namespace Camunda.Connector.SDK.Core.Api.Error;

public class ConnectorException : Exception
{
    public string ErrorCode { get; set; }

    public ConnectorException(string errorCode)
    {
        ErrorCode = errorCode;
    }
}