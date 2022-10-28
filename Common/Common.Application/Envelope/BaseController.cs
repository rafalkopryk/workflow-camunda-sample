namespace Common.Application.Envelope;

using Common.Application.Errors;
using Microsoft.AspNetCore.Mvc;

public class BaseController : ControllerBase
{
    protected IActionResult Failure(string errorCode)
    {
        var error = new Error(errorCode);
        return errorCode switch
        {
            ErrorCode.OperationNotAllowed
            or ErrorCode.ResourceExists => UnprocessableEntity(error),
            ErrorCode.ResourceNotFound => NotFound(error),
            _ => StatusCode(500, error),
        };
    }
}
