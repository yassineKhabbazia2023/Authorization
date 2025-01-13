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
using Pulse.Authorization.Core.Exceptions;
using Pulse.Authorization.API.Configuration.Models;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Back.Events.Configurations;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events;
using Pulse.Authorization.Infrastructure.Providers.Interfaces;
using Pulse.Authorization.Infrastructure.Providers;
using Pulse.Authorization.Infrastructure.Interfaces;
using Pulse.Authorization.Infrastructure.Constants;

namespace Pulse.Authorization.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void Register(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterCors(services);
            RegisterBroker(services, configuration);
            RegisterDatabase(services, configuration);
            RegisterServices(services);
        }

        private static void RegisterBroker(IServiceCollection services, IConfiguration configuration)
        {
            var brokerSettings = configuration!.GetSection("BrokerSetting").Get<BrokerSetting>();

            if (string.IsNullOrWhiteSpace(brokerSettings!.ServiceBusNamespace))
            {
                throw new NullArgumentException(Errors.NotFoundServiceBusNamespaceCode, Errors.NotFoundServiceBusNamespaceMessage);
            }

            if (string.IsNullOrWhiteSpace(brokerSettings!.ManagedIdentityClientId))
            {
                throw new NullArgumentException(Errors.NotFoundManagedIdentityClientIdCode, Errors.NotFoundManagedIdentityClientIdMessage);
            }

            var options = new BrokerOptions
            {
                ServiceBusNamespace = brokerSettings!.ServiceBusNamespace,
                ManagedIdentityClientId = brokerSettings!.ManagedIdentityClientId,
                PushTopicNames = [brokerSettings!.PushTopicName!],
            };

            if (brokerSettings!.PullTopics?.Count != 0)
            {
                foreach (var topic in brokerSettings!.PullTopics!)
                {
                    options.AddPullTopicItem(topic.TopicName!, topic.Subscriptions!);
                }
            }

            services.AddScoped<IContactEventRepository, ContactEventRepository>();
            services.AddScoped<IAccountEventRepository, AccountEventRepository>();
            services.AddScoped<IRoleEventRepository, RoleEventRepository>();
            services.AddScoped<ISubscriptionEventRepository, SubscriptionEventRepository>();
            services.AddKeyedScoped<IEventHandler, ContactCreatedEventHandler>(nameof(ContactCreatedEvent));
            services.AddKeyedScoped<IEventHandler, ContactUpdatedEventHandler>(nameof(ContactUpdatedEvent));
            services.AddKeyedScoped<IEventHandler, ContactRemovedEventHandler>(nameof(ContactRemovedEvent));
            services.AddKeyedScoped<IEventHandler, AccountRemovedEventHandler>(nameof(AccountRemovedEvent));
            services.AddKeyedScoped<IEventHandler, AccountCreatedEventHandler>(nameof(AccountCreatedEvent));
            services.AddKeyedScoped<IEventHandler, AccountUpdatedEventHandler>(nameof(AccountUpdatedEvent));
            services.AddKeyedScoped<IEventHandler, RoleCreatedEventHandler>(nameof(RoleCreatedEvent));
            services.AddKeyedScoped<IEventHandler, RoleUpdatedEventHandler>(nameof(RoleUpdatedEvent));
            services.AddKeyedScoped<IEventHandler, RoleDeletedEventHandler>(nameof(RoleDeletedEvent));
            services.AddKeyedScoped<IEventHandler, SubscriptionValidatedEventHandler>(nameof(SubscriptionValidatedEvent));

            services.AddScoped<IAuthorizationEventPublisher, AuthorizationEventPublisher>();

            services.AddEventPullServices(options);
            services.AddEventPushServices(options);
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
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
                    opt.EnableRetryOnFailure(1, TimeSpan.FromMilliseconds(GlobalConstants.RetryTimespan), null);
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
