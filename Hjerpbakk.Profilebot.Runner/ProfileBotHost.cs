using System;
using Hjerpbakk.ProfileBot.Contracts;
using LightInject;
using SlackConnector;

namespace Hjerpbakk.ProfileBot.Runner {
    public class ProfileBotHost {
        ProfilebotImplmentation profilebot;

        public void Start(string slackToken, string adminUserId) {
            if (string.IsNullOrEmpty(slackToken)) {
                throw new ArgumentException(nameof(slackToken));
            }

            if (string.IsNullOrEmpty(adminUserId)) {
                throw new ArgumentException(nameof(adminUserId));
            }

            var serviceContainer = CompositionRoot(slackToken, adminUserId);
            profilebot = serviceContainer.GetInstance<ProfilebotImplmentation>();
            profilebot
                .Connect()
                .ContinueWith(task => {
                    if (!task.IsCompleted || task.IsFaulted) {
                        Console.WriteLine($"Error connecting to Slack: {task.Exception}");
                    }
                });
        }

        public void Stop() {
            Console.WriteLine("Disconnecting...");
            profilebot.Dispose();
            profilebot = null;
        }

        static IServiceContainer CompositionRoot(string slackToken, string adminUserId) {
            var serviceContainer = new ServiceContainer();

            serviceContainer.RegisterInstance(slackToken);
            serviceContainer.RegisterInstance(new AdminUser(adminUserId));
            serviceContainer.Register<ISlackProfileValidator, SlackProfileValidator>();
            serviceContainer.Register<ISlackConnector, SlackConnector.SlackConnector>();
            serviceContainer.Register<ISlackIntegration, SlackIntegration>();
            serviceContainer.Register<ProfilebotImplmentation>();

            return serviceContainer;
        }
    }
}