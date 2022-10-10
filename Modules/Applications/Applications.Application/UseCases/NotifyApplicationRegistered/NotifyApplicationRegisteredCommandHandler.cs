using Common.Application.Zeebe;
using MediatR;

namespace Applications.Application.UseCases.NotifyApplicationRegistered
{
    internal class NotifyApplicationRegisteredCommandHandler : IRequestHandler<NotifyApplicationRegisteredCommand>
    {
        private readonly IZeebeService _zeebeService;

        public NotifyApplicationRegisteredCommandHandler(IZeebeService zeebeService)
        {
            _zeebeService = zeebeService;
        }

        public async Task<Unit> Handle(NotifyApplicationRegisteredCommand request, CancellationToken cancellationToken)
        {
            //logic simualtion
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

            await _zeebeService.CompleteJob(request.Job, cancellationToken);
            return Unit.Value;
        }
    }
}
