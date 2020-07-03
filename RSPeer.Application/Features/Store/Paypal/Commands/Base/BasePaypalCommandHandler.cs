using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using System.Text.Json;
using RSPeer.Application.Exceptions;

namespace RSPeer.Application.Features.Store.Paypal.Commands.Base
{
	public abstract class BasePaypalCommandHandler<T, TU> : IRequestHandler<T, TU> where T : IRequest<TU>
	{
		private readonly HttpClient _client;
		private readonly IMediator _mediator;

		protected BasePaypalCommandHandler(IHttpClientFactory factory, IMediator mediator)
		{
			_client = factory.CreateClient("Paypal");
			_mediator = mediator;
		}

		public abstract Task<TU> Handle(T request, CancellationToken cancellationToken);

		protected async Task<string> MakeAuthorizedPost(string baseUrl, string path, object payload)
		{
			return await MakeAuthorizedRequest(HttpMethod.Post, baseUrl, path, payload);
		}
		
		protected async Task<string> MakeAuthorizedRequest(HttpMethod method, string baseUrl, string path, object payload = null)
		{
			var token = await _mediator.Send(new PaypalGetAccessTokenCommand { BaseUrl = baseUrl });
			var request = new HttpRequestMessage
			{
				Headers =
				{
					{"Authorization", "Bearer " + token}
				},
				Method = method,
				RequestUri = new Uri(baseUrl + path)
			};
			if (payload != null)
			{
				request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8,
					"application/json");
			}
			var res = await _client.SendAsync(request);
			var body = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode) throw new PaypalException(body);
			return body;
		}
	}
}