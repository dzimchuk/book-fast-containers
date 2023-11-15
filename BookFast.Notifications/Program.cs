using BookFast.Integration;
using BookFast.Integration.Models.Identity;
using BookFast.Notifications;
using BookFast.Notifications.Policies;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        services.Configure<CommunicationServiceOptions>(hostContext.Configuration.GetSection("CommunicationService"));

        var serviceBusOptions = hostContext.Configuration.GetServiceBusOptions(hostContext.HostingEnvironment);

        services.AddMassTransit(config =>
        {
            config.AddConsumer<MailSender<ConfirmEmail>>().ExcludeFromConfigureEndpoints();
            config.AddConsumer<MailSender<ResetPassword>>().ExcludeFromConfigureEndpoints();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(serviceBusOptions.ConnectionString);

                cfg.ReceiveEndpoint(serviceBusOptions.GetMailSenderQueueName(), endpointConfig =>
                {
                    endpointConfig.MaxDeliveryCount = 10;

                    endpointConfig.UseMessageRetry(r => r.Intervals(500, 1000));

                    endpointConfig.UseKillSwitch(killSwitchOptions =>
                    {
                        killSwitchOptions.SetActivationThreshold(10);
                        killSwitchOptions.SetTripThreshold(1);
                        killSwitchOptions.SetTrackingPeriod(TimeSpan.FromMinutes(1));
                        killSwitchOptions.SetRestartTimeout(TimeSpan.FromSeconds(60));

                        killSwitchOptions.SetExceptionFilter(filter =>
                        {
                            filter.Handle(typeof(MailServiceThrottlingException));
                        });
                    });

                    // do not create exchange (topic and subscription), just the queue
                    endpointConfig.ConfigureMessageTopology<MailMessage<ConfirmEmail>>(false);
                    endpointConfig.ConfigureMessageTopology<MailMessage<ResetPassword>>(false);

                    endpointConfig.ConfigureConsumer<MailSender<ConfirmEmail>>(context);
                    endpointConfig.ConfigureConsumer<MailSender<ResetPassword>>(context);
                });

                // configure other consumers, sagas, etc
                //cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

host.Run();
