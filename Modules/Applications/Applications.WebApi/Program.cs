using Applications.Application.Extensions;
using Applications.Application.UseCases.CancelApplication;
using Applications.Application.UseCases.GetApplication;
using Applications.Application.UseCases.RegisterApplication;
using Applications.Application.UseCases.SignContract;
using Common.Application.Extensions;
using MediatR;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddCors();
builder.Services.AddOpenApi();

builder.Host.UseWolverine(opts => opts.ConfigureWolverine(builder.Configuration));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/openapi/v1.json", "Applications Api"));

app.UseHttpsRedirection();

app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

await app.ConfigureApplication();

var applicationsEndpoints = app.MapGroup("/applications");
applicationsEndpoints.MapPost("/", async Task<IResult> (RegisterApplicationCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result switch
    {
        RegisterApplicationCommandResponse.OK => TypedResults.Created(),
        RegisterApplicationCommandResponse.ResourceExists resourceExists => TypedResults.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: nameof(resourceExists)),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
})
.WithName("RegisterApplication");

applicationsEndpoints.MapGet("/{applicationId}", async Task<IResult> (string applicationId, IMediator mediator) =>
{
    var result = await mediator.Send(new GetApplicationQuery(applicationId));
    return result switch
    {
        GetApplicationQueryResponse.OK ok => TypedResults.Ok(ok),
        GetApplicationQueryResponse.ResourceNotFound resourceNotFound => TypedResults.NotFound(),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
})
.WithName("GetApplication");

applicationsEndpoints.MapPost("/{applicationId}/signature", async Task<IResult> (string applicationId, IMediator mediator) =>
{
    var result = await mediator.Send(new SignContractCommand(applicationId));
    return result switch
    {
        SignContractCommandResponse.OK => TypedResults.Ok(),
        SignContractCommandResponse.ResourceNotFound resourceNotFound => TypedResults.NotFound(),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
})
.WithName("SignContract");

applicationsEndpoints.MapPost("/{applicationId}/cancellation", async Task<IResult> (string applicationId, IMediator mediator) =>
{
    var result = await mediator.Send(new CancelApplicationCommand(applicationId));
    return result switch
    {
        CancelApplicationCommandResponse.OK => TypedResults.Ok(),
        CancelApplicationCommandResponse.ResourceNotFound resourceNotFound => TypedResults.NotFound(),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };
})
.WithName("CancelApplication");

await app.RunAsync();