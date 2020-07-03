using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RSPeer.Application.Features.UserData.Commands;
using RSPeer.Common.Extensions;
using RSPeer.Domain.Entities;

namespace RSPeer.Application.Features.UserData.Queries
{
    public class GetBotPreferencesQueryHandler : IRequestHandler<GetBotPreferencesQuery, BotPreferences>
    {
        private readonly IMediator _mediator;

        public GetBotPreferencesQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<BotPreferences> Handle(GetBotPreferencesQuery request, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new GetUserJsonDataQuery {UserId = request.UserId, Key = "bot_preferences"}, cancellationToken);
            BotPreferences pref;
            
            if (!string.IsNullOrEmpty(data))
            {
                pref = data.To<BotPreferences>();
            }
            else
            {
                pref = new BotPreferences();
                await _mediator.Send(new SaveUserJsonDataCommand
                    {Key = "bot_preferences", UserId = request.UserId, Value = pref}, cancellationToken);
            }

            return pref;
        }
        
    }
}