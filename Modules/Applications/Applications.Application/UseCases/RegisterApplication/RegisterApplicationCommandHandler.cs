using Common.Application.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.RegisterApplication
{
    internal class RegisterApplicationCommandHandler : IRequestHandler<RegisterApplicationCommand>
    {
        private readonly IZeebeService _zeebeService;

        public RegisterApplicationCommandHandler(IZeebeService zeebeService)
        {
            _zeebeService = zeebeService;
        }

        public async Task<Unit> Handle(RegisterApplicationCommand request, CancellationToken cancellationToken)
        {
            var result = await _zeebeService.StartProcessInstance("register-application");
            return Unit.Value;
        }
    }
}
