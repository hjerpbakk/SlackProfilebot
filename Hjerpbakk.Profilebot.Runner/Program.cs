using System;
using System.Threading;
using Hjerpbakk.ProfileBot.Runner.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;
using Topshelf;

namespace Hjerpbakk.ProfileBot.Runner {
    internal class Program {
        static readonly Logger logger;

        static Program() {
            logger = LogManager.GetCurrentClassLogger();
        }

        static void Main() {
            var configurationReader = new ConfigReader();
            if (!string.IsNullOrEmpty(configurationReader.ApplicationInsightsInstrumentationKey)) {
                TelemetryConfiguration.Active.InstrumentationKey = configurationReader.ApplicationInsightsInstrumentationKey;
            }

            var keepAliveTimer = new Timer(HeartBeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(100));
            logger.Info("Starting heartbeat.");

            logger.Info("Starting Profilebot.");
            HostFactory.Run(host => {
                host.Service<ProfileBotHost>(service => {
                    service.ConstructUsing(name => new ProfileBotHost());
                    service.WhenStarted(n => { n.Start(configurationReader.SlackApiKey, configurationReader.AdminUserId); });
                    service.WhenStopped(n => n.Stop());
                });

                host.UseNLog();

                host.OnException(exception => { logger.Fatal(exception, "Fatal error, Profilebot going down."); });

                host.RunAsNetworkService();

                host.SetDisplayName("Slack Profilebot");
                host.SetServiceName("Slack Profilebot");
                host.SetDescription("Validates the profile of Slack users according to your team's rules.");
            });

            logger.Info("Profilebot stopped.");

            keepAliveTimer.Dispose();
            logger.Info("Heartbeat stopped.");
        }

        static void HeartBeat(object state) {
            logger.Info("Still alive");
        }
    }
}