using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RSPeer.Application.Exceptions;
using RSPeer.Common.Extensions;
using RSPeer.Persistence;

namespace RSPeer.Application.Features.Admin.Dashboard.Queries
{
    public class ValidateDashboardKeyQueryHandler : IRequestHandler<ValidateDashboardKeyQuery, Unit>
    {
        private readonly RsPeerContext _db;

        public ValidateDashboardKeyQueryHandler(RsPeerContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(ValidateDashboardKeyQuery request, CancellationToken cancellationToken)
        {
            var error = "dashboard_key_invalid";
            if (string.IsNullOrEmpty(request.Key))
            {
                throw new AuthorizationException(error);
            }
            
            var key = await _db.UserJsonData.OrderByDescending(w => w.Id).FirstOrDefaultAsync(
                w => w.UserId == request.UserId && w.Key == "dashboard_access_key", cancellationToken);
            
            if (key == null)
            {
               throw new AuthorizationException(error);
            }
            
            var obj = key.Value.To<JObject>();

            var value = obj.Value<string>("value");

            if (value != request.Key)
            {
                throw new AuthorizationException(error);
            }
            
            var expirationRaw = obj.Value<string>("expiration");
            var expiration = DateTimeOffset.Parse(expirationRaw);

            if (DateTimeOffset.UtcNow < expiration)
            {
                return Unit.Value;
            }
            
            _db.UserJsonData.Remove(key);
            await _db.SaveChangesAsync(cancellationToken);
            throw new AuthorizationException(error);
        }
    }
}