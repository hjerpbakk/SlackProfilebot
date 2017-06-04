using System;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.ProfileBot.Contracts;
using Hjerpbakk.ProfileBot.FaceDetection;
using LightInject;
using SlackConnector;

namespace Hjerpbakk.ProfileBot.Runner {
    public class ProfileBotHost {
        ProfilebotImplmentation profilebot;

        public void Start(string slackToken, AdminUser adminUser, FaceDetectionAPI faceDetectionAPI) {
            if (string.IsNullOrEmpty(slackToken)) {
                throw new ArgumentException(nameof(slackToken));
            }

            if (string.IsNullOrEmpty(adminUser.Id)) {
                throw new ArgumentException(nameof(adminUser.Id));
            }

            var serviceContainer = CompositionRoot(slackToken, adminUser, faceDetectionAPI);
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

        static IServiceContainer CompositionRoot(string slackToken, AdminUser adminUser, FaceDetectionAPI faceDetectionAPI) {
            var serviceContainer = new ServiceContainer();

            serviceContainer.RegisterInstance(slackToken);
            serviceContainer.RegisterInstance(adminUser);
            serviceContainer.RegisterInstance(faceDetectionAPI);
            serviceContainer.Register<IFaceDetectionClient, FaceDetectionClient>();
            serviceContainer.Register<ISlackProfileValidator, SlackProfileValidator>();
            serviceContainer.Register<ISlackConnector, SlackConnector.SlackConnector>();
            serviceContainer.Register<ISlackIntegration, SlackIntegration>();
            serviceContainer.Register<ProfilebotImplmentation>();

            return serviceContainer;
        }
    }
}