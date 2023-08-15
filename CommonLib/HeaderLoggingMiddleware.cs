using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CommonLib;

public class HeaderLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<HeaderLoggingMiddleware> _logger;

	public HeaderLoggingMiddleware(RequestDelegate next, ILogger<HeaderLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}
	public async Task Invoke(HttpContext context)
	{
		var headers = context.Request.Headers;
		var headerLog = new StringBuilder();

		foreach (var header in headers)
		{
			headerLog.Append($"{header.Key}: {header.Value}; ");
		}

		_logger.LogInformation("Incoming Request Headers: {Headers}", headerLog.ToString());
		await _next(context);
	}
}

public static class HeaderLoggingMiddlewareExtensions
{
	public static IApplicationBuilder UseHeaderLoggingMiddleware(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<HeaderLoggingMiddleware>();
	}
}
