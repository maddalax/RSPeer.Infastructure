using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RSPeer.Api.Jobs.Children;

namespace RSPeer.Api.Jobs.Commands
{
    public class RegisterJobsCommandHandler : IRequestHandler<RegisterJobsCommand, Unit>
    {
        private readonly IServiceScopeFactory _factory;

        public RegisterJobsCommandHandler(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public async Task<Unit> Handle(RegisterJobsCommand request, CancellationToken cancellationToken)
        {
            using (var scope = _factory.CreateScope())
            {
                var provider = scope.ServiceProvider;
                
                RecurringJob.AddOrUpdate($"set_client_count_{Program.Version}", () => provider.GetService<SetClientCountJob>().Execute(),
                    () => "*/5 * * * *", TimeZoneInfo.Utc);

                RecurringJob.AddOrUpdate($"save_ip_access_{Program.Version}", () => provider.GetService<SaveIpAccess>().Execute(),
                    () => "*/5 * * * *", TimeZoneInfo.Utc);

                RecurringJob.AddOrUpdate($"set_true_script_count_{Program.Version}", () => provider.GetService<SetTrueScriptCounts>().Execute(),
                    () => "00 8 * * *", TimeZoneInfo.Utc);
                
                RecurringJob.AddOrUpdate($"check_inuvation_access_{Program.Version}", () => provider.GetService<CheckInuvationAccess>().Execute(),
                    () => "*/10 * * * *", TimeZoneInfo.Utc);
                
                RecurringJob.AddOrUpdate($"send_discord_alert_{Program.Version}", () => provider.GetService<SendDiscordAlerts>().Execute(),
                    () => "*/5 * * * *", TimeZoneInfo.Utc);
                
                RecurringJob.AddOrUpdate($"instance_close_job_{Program.Version}", () => provider.GetService<InstanceCloseJob>().Execute(),
                    () => "*/3 * * * *", TimeZoneInfo.Utc);
            }

            return Unit.Value;
        }
    }
}