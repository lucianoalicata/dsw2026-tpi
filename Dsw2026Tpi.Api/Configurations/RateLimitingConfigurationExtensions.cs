using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class RateLimitingConfigurationExtensions
    {
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var adminLogin = configuration.GetSection("RateLimiting:AdminLogin").Get<RateLimitSettings>();
            var patientLogin = configuration.GetSection("RateLimiting:PatientLogin").Get<RateLimitSettings>();
            var booking = configuration.GetSection("RateLimiting:AppointmentBooking").Get<RateLimitSettings>();
            var global = configuration.GetSection("RateLimiting:Global").Get<RateLimitSettings>();

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                    logger.LogWarning(
                        "Se excedió el Rate Limit: {Path} desde {Ip}",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);

                    var error = new ErrorResponse(nameof(ErrorCodes.RATE_LIMIT_EXCEDED), ErrorCodes.RATE_LIMIT_EXCEDED);
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(error, new JsonSerializerOptions(JsonSerializerDefaults.Web)), token);

                };
                options.AddPolicy("AdminLoginPolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:httpContext.Connection.RemoteIpAddress?.ToString() ??  "IP_desconocida",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit =adminLogin.PermitLimit,
                        Window = TimeSpan.FromSeconds(adminLogin.Seconds),
                        QueueLimit=0   
                    }));

                options.AddPolicy("PatientLoginPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "IP_desconocida",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = patientLogin.PermitLimit,
                            Window = TimeSpan.FromSeconds(patientLogin.Seconds),
                            QueueLimit =0
                        }));

                options.AddPolicy("AppointmentBookingPolicy",httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        
                        partitionKey: httpContext.User.Identity?.Name ?? "no_autenticado",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit =booking.PermitLimit,
                            Window = TimeSpan.FromSeconds(booking.Seconds),
                            QueueLimit = 0
                        }));
                
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "IP_desconocida",
                        factory: _ =>new FixedWindowRateLimiterOptions
                        {
                            PermitLimit= global.PermitLimit,
                            Window= TimeSpan.FromSeconds(global.Seconds),
                            QueueLimit = 0
                        }));
            });
            return services;
        }
    }
}