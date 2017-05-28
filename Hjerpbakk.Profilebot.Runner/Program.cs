using System;
using NLog;
using Topshelf;

namespace Hjerpbakk.ProfileBot.Runner {
    internal class Program {
        static readonly Logger logger;

        static Program() {
            logger = LogManager.GetCurrentClassLogger();
        }

        static void Main() {
            logger.Info("Starting Profilebot.");
            HostFactory.Run(host => {
                host.Service<ProfileBotHost>(service => {
                    service.ConstructUsing(name => new ProfileBotHost());
                    service.WhenStarted(n => { n.Start(); });
                    service.WhenStopped(n => n.Stop());
                });

                host.UseNLog();

                host.OnException(exception => { logger.Fatal(exception, "Fatal error, Profilebot going down."); });

                host.RunAsNetworkService();

                host.SetDisplayName("Slack Profilebot");
                host.SetServiceName("Slack Profilebot");
                host.SetDescription("Validates the profile of Slack users according to your team's rules.");
            });
            logger.Info("Stopping Profilebot.");
        }
    }
}