namespace Camunda.Connector.SDK.Core.Api.Error;

public class ConnectorException : Exception
{
    public string ErrorCode { get; set; }

    public ConnectorException(string errorCode)
    {
        ErrorCode = errorCode;
    }

    public ConnectorException(string message, Exception? innerException = null)
        :base(message, innerException)
    {
    }

    public ConnectorException(string errorCode, string message, Exception? innerException = null)
    : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}