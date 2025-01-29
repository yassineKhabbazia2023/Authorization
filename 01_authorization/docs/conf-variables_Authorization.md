# Variables d'environnement

| Projet | Variable | Valeur | Description |
|---|---|---|---|
| Authorization.API | APPINSIGHTS_INSTRUMENTATIONKEY | ef648e6e-8440-4545-a001-0e71b1fa59e2 | Clé AppInsights |
| Authorization.API | APPLICATIONINSIGHTS_CONNECTION_STRING | InstrumentationKey=ef648e6e-8440-4545-a001-0e71b1fa59e2;IngestionEndpoint=https://francecentral-1.in.applicationinsights.azure.com/;LiveEndpoint=https://francecentral.livediagnostics.monitor.azure.com/;ApplicationId=58c023e6-78f4-497a-910a-1bb15a7462f8 | Chaîne de connexion AppInsights |
| Authorization.API | BrokerSetting__ManagedIdentityClientId | 22a4b114-7fcb-43f0-9f4d-6ae0d513b8c5 | ID de l'identité managée du broker |
| Authorization.API | BrokerSetting__ServiceBusNamespace | sbnscegpulsehubrec0101.servicebus.windows.net | Namespace du Service Bus |
| Authorization.API | BrokerSetting__PullTopics__0__TopicName | contact | Nom du topic contact |
| Authorization.API | BrokerSetting__PullTopics__0__Subscriptions__0 | contact-created-authorization | Subscription contactCreated sur le topic contact |
| Authorization.API | BrokerSetting__PullTopics__0__Subscriptions__1 | contact-updated-authorization | Subscription contactUpdated sur le topic contact |
| Authorization.API | BrokerSetting__PullTopics__0__Subscriptions__2 | contact-removed-authorization | Subscription contactRemoved sur le topic contact |
| Authorization.API | BrokerSetting__PullTopics__1__TopicName | account | Nom du topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__0 | account-created-authorization | Subscription accountCreated sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__1 | account-updated-authorization | Subscription accountUpdated sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__2 | account-removed-authorization | Subscription accountRemoved sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__3 | role-created-authorization | Subscription roleCreated sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__4 | role-updated-authorization | Subscription roleUpdated sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__1__Subscriptions__5 | role-deleted-authorization | Subscription roleDeleted sur le topic account |
| Authorization.API | BrokerSetting__PullTopics__2__TopicName | offer | Nom du topic offer |
| Authorization.API | BrokerSetting__PullTopics__2__Subscriptions__0 | offer-validated-authorization | Subscription offerValidated sur le topic offer |
| Authorization.API | BrokerSetting__PushTopicName | authorization | Nom du topic push authorization |
| Authorization.API | SqlAuthorizationConnectionString | Server=tcp:sqlcegpulseautrec0101.database.windows.net,1433;Initial Catalog=sqldbcegpulseautrec0101;Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Managed Identity;User Id=22a4b114-7fcb-43f0-9f4d-6ae0d513b8c5 | Chaîne de connexion SQL Authorization |
