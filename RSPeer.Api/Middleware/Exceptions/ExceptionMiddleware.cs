using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RSPeer.Application.Exceptions;
using RSPeer.Application.Features.Scripts.Commands.CompileScript.Models;
using RSPeer.Common.Enviroment;
using RSPeer.Domain.Exceptions;

namespace RSPeer.Api.Middleware.Exceptions
{
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _client;

		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> client)
		{
			_next = next;
			_client = client;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			var ignored = new List<Type>
			{
				typeof(UserException),
				typeof(WebPathException)
			};

			if (!ignored.Contains(exception.GetType()))
			{
				var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
				var path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
				_client.LogError(exception, $"{path}?{query}");
			}

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

			if (exception is AuthorizationException)
			{
				context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
			}

			var error = exception.Message;

			if (exception is CompileException)
			{
				return context.Response.WriteAsync(exception.ToString());
			}

			return context.Response.WriteAsync(JsonSerializer.Serialize(new
			{
				error
			}));
		}
	}
}