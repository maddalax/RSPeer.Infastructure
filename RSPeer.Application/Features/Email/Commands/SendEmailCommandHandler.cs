using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RSPeer.Application.Features.Email.Commands
{
    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, HttpStatusCode>
    {
        private readonly ISendGridClient _client;

        public SendEmailCommandHandler(ISendGridClient client)
        {
            _client = client;
        }

        public async Task<HttpStatusCode> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            var email = MailHelper.CreateSingleEmail(new EmailAddress(request.FromEmail, request.FromName),
                new EmailAddress(request.ToEmail), request.Subject, 
                request.IsHtml ? string.Empty : request.Body, request.IsHtml ? request.Body : string.Empty);
            var result = await _client.SendEmailAsync(email, cancellationToken);
            return result.StatusCode;
        }
    }
}