namespace Common.Application.Envelope;

using Common.Application.Errors;
using Microsoft.AspNetCore.Mvc;

public class BaseController : ControllerBase
{
    protected IActionResult Failure(string errorCode)
    {
        return errorCode switch
        {
            ErrorCode.OperationNotAllowed
            or ErrorCode.ResourceExists => Problem(statusCode: 422, title: errorCode),
            ErrorCode.ResourceNotFound => Problem(statusCode: 404, title: errorCode),
            _ => Problem(statusCode: 422, title: errorCode),
        };
    }
}
