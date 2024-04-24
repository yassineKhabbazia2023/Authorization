// <copyright file="ServicesConfiguration.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Pulse.Authorization.Infrastructure.Context;
using Pulse.Authorization.Infrastructure.Repositories;
using Pulse.Authorization.Core.Interfaces;
using Pulse.Authorization.Core.Services;
using IAuthorizationService = Pulse.Authorization.Core.Interfaces.IAuthorizationService;

namespace Pulse.Authorization.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
        }

        public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            var connectionString = configuration["SqlAuthorizationConnectionString"];
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

            services.AddDbContextPool<AuthorizationContext>(options =>
            {
                options.UseSqlServer(connectionString, opt =>
                {
                    opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });

            services.AddHealthChecks()
                .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
        }

        public static void RegisterApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationInsightsConexionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = applicationInsightsConexionString;
            });
        }

        public static void RegisterCors(this IServiceCollection services)
        {
            services.AddCors(options =>
                {
                    options.AddPolicy(
                        name: "CorsPolicy",
                        builder =>
                        {
                            builder.AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowCredentials()
                                    .SetIsOriginAllowed(_ => true)
                                    .WithExposedHeaders("content-range", "content-type", "accept-ranges", "link");
                        });
                });
        }
    }
}
