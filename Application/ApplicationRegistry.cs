using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Audit;
using Application.Common.Auth.Authentication;
using Application.Common.Auth.Authorization;
using Application.Common.Behaviours;
using Application.Common.ErrorHandling;
using AspNetCoreRateLimit;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Sieve.Models;
using Sieve.Services;

namespace Application
{
    public static class ApplicationRegistry
    {

        public static IServiceCollection AddApplicationRegistry(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>))
                .AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>))
                .AddValidationPipeline()
                .AddAudit(configuration)
                .AddAuthentication(configuration)
                .AddAuthorization()
                .AddIpRateLimiting(configuration)
                .Configure<SieveOptions>(configuration.GetSection("Sieve"))
                .AddScoped<ISieveProcessor, ApplicationSieveProcessor>();

            return services;
        }

        //hook up validation into MediatR pipeline
        private static IServiceCollection AddValidationPipeline(
            this IServiceCollection services)
        {
            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationPipelineBehavior<,>));
            return services;
        }

        private static IServiceCollection AddAudit(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<AuditSettings>(configuration.GetSection(nameof(AuditSettings)));
            return services;
        }

        private static IServiceCollection AddAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //do not auto map JwtRegisteredClaimNames to ClaimTypes
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<PasswordHasherSettings>(configuration.GetSection(nameof(PasswordHasherSettings)));
            services.AddOptions();

            var settings = services.BuildServiceProvider().GetService<IOptions<JwtSettings>>().Value;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = settings.SigningCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.TokenValidationParameters = tokenValidationParameters;
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = (
                                context) =>
                            {
                                var token = context.HttpContext.Request.Headers["Authorization"];
                                if (token.Count > 0 && token[0].StartsWith(
                                        "Token ",
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    context.Token = token[0].Substring("Token ".Length).Trim();
                                }

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                {
                                    context.Response.Headers.Add(
                                        "Token-Expired",
                                        "true");
                                }

                                return Task.CompletedTask;
                            }
                        };
                    });
            return services;
        }

        private static IServiceCollection AddAuthorization(
            this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, AuthorizationHandler>();

            services.AddControllers(
                config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                }) .AddJsonOptions(options => 
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }

        private static IServiceCollection AddIpRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //Can be rate limited by Client Id as well
            //ClientRateLimitOptions
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }

        public static void AddSerilogLogging(
            this ILoggerFactory loggerFactory,
            IWebHostEnvironment env)
        {
            /*var log1 = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Error)
                //.Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .WriteTo.File(new CompactJsonFormatter(), "Logs/logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();*/

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(
                    "appsettings.json",
                    false,
                    true)
                .AddJsonFile(
                    $"appsettings.{env.EnvironmentName}.json",
                    true)
                .AddEnvironmentVariables()
                .Build();

            var log = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            loggerFactory.AddSerilog(log);
            Log.Logger = log;
        }

        public static IApplicationBuilder UseErrorHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}