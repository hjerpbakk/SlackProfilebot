using System;
using System.Threading;
using Hjerpbakk.Profilebot.Runner.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;
using Topshelf;

namespace Hjerpbakk.Profilebot.Runner {
    internal class Program {
        static readonly Logger logger;

        static Timer keepAliveTimer;

        static Program() {
            logger = LogManager.GetCurrentClassLogger();
        }

        static void Main() {
            try {
                var configurationReader = new ConfigReader();
                if (!string.IsNullOrEmpty(configurationReader.ApplicationInsightsInstrumentationKey)) {
                    TelemetryConfiguration.Active.InstrumentationKey = configurationReader.ApplicationInsightsInstrumentationKey;
                }

                if (configurationReader.ShouldStartHeartBeat) {
                    keepAliveTimer = new Timer(HeartBeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(100));
                    logger.Info("Starting heartbeat.");
                }


                logger.Info("Starting Profilebot.");
                HostFactory.Run(host => {
                    host.Service<ProfileBotHost>(service => {
                        service.ConstructUsing(name => new ProfileBotHost());
                        service.WhenStarted(n => { n.Start(configurationReader.SlackApiKey, configurationReader.AdminUser, configurationReader.FaceDetectionConfiguration, configurationReader.BlobStorageConfiguration); });
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

                if (keepAliveTimer == null) {
                    return;
                }

                keepAliveTimer.Dispose();
                logger.Info("Heartbeat stopped.");
            }
            catch (Exception e) {
                logger.Fatal(e, "Could not start bot");
            }
        }

        static void HeartBeat(object state) {
            logger.Info("Still alive");
        }
    }
}