using System.Net;
using MediatR;

namespace RSPeer.Application.Features.Email.Commands
{
    public class SendEmailCommand : IRequest<HttpStatusCode>
    {
        public string FromEmail { get; set; }
        public string FromName { get; } = "noreply@rspeer.org";
        
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        
        public string Body { get; set; }
        
        public bool IsHtml { get; set; }
    }
}