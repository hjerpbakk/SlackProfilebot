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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            HostFactory.Run(host => {
                host.Service<ProfileBotHost>(service => {
                    service.ConstructUsing(name => new ProfileBotHost());
                    service.WhenStarted(n => { n.Start(); });
                    service.WhenStopped(n => n.Stop());
                });

                host.RunAsNetworkService();

                host.SetDisplayName("Slack Profilebot");
                host.SetServiceName("Slack Profilebot");
                host.SetDescription("Validates the profile of Slack users according to your team's rules.");
            });
            logger.Info("Stopping Profilebot.");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            const string FatalError = "Fatal error, Profilebot going down.";
            var exception = e.ExceptionObject as Exception;
            if (exception == null) {
                logger.Fatal(FatalError);
            }
            else {
                logger.Fatal(exception, FatalError);
            }
        }
    }
}