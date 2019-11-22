using System;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using Hjerpbakk.Profilebot;
using Hjerpbakk.Profilebot.Runner;
using LightInject;
using Microsoft.ProjectOxford.Face;
using SlackConnector;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Runner {
    public class ProfileBotHost {
        ProfilebotImplmentation profilebot;

        public void Start(string slackToken, SlackUser adminUser, FaceDetectionConfiguration faceDetectionConfiguration, BlobStorageConfiguration blobStorageConfiguration) {
            if (string.IsNullOrEmpty(slackToken)) {
                throw new ArgumentException(nameof(slackToken));
            }

            if (string.IsNullOrEmpty(adminUser.Id)) {
                throw new ArgumentException(nameof(adminUser.Id));
            }

            var serviceContainer = CompositionRoot(slackToken, adminUser, faceDetectionConfiguration, blobStorageConfiguration);
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

        static IServiceContainer CompositionRoot(string slackToken, SlackUser adminUser, FaceDetectionConfiguration faceDetectionConfiguration, BlobStorageConfiguration blobStorageConfiguration) {
            var serviceContainer = new ServiceContainer();
            serviceContainer.RegisterInstance(slackToken);
            serviceContainer.RegisterInstance(adminUser);

            if (string.IsNullOrEmpty(faceDetectionConfiguration.Key)) {
                serviceContainer.Register<IFaceDetectionClient, FaceServiceClientDummy>();
                serviceContainer.Register<IFaceWhitelist, FaceWhitelistDummy>();
            }
            else {
                serviceContainer.RegisterInstance(faceDetectionConfiguration);
                serviceContainer.RegisterInstance<IFaceServiceClient>(new FaceServiceClient(faceDetectionConfiguration.Key, faceDetectionConfiguration.URL));
                serviceContainer.Register<IFaceDetectionClient, FaceDetectionClient>();
                serviceContainer.RegisterInstance<IFaceWhitelist>(new FaceWhitelist(blobStorageConfiguration));
            }

            serviceContainer.Register<ISlackProfileValidator, SlackProfileValidator>();
            serviceContainer.Register<ISlackConnector, SlackConnector.SlackConnector>();
            serviceContainer.Register<ISlackIntegration, SlackIntegration>();
            serviceContainer.Register<ProfilebotImplmentation>();

            return serviceContainer;
        }
    }
}