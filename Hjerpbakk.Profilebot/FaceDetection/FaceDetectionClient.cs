using System;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Configuration;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using Microsoft.ProjectOxford.Face;
using NLog;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.FaceDetection {
    /// <summary>
    ///     Client for recognizing faces i Slack profile images.
    /// </summary>
    public class FaceDetectionClient : IFaceDetectionClient {
        static readonly Logger logger;
        readonly int delay;

        readonly IFaceServiceClient faceServiceClient;
        readonly IFaceWhitelist faceWhitelist;

        /// <summary>
        ///     Initializes the logger once.
        /// </summary>
        static FaceDetectionClient() {
            logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="faceServiceClient">Client for calling Azure Cognitive Services.</param>
        /// <param name="faceWhitelist">Knows about whitelisted users.</param>
        /// <param name="configuration">The configuration to use.</param>
        public FaceDetectionClient(IFaceServiceClient faceServiceClient, IFaceWhitelist faceWhitelist, FaceDetectionConfiguration configuration) {
            this.faceServiceClient = faceServiceClient ?? throw new ArgumentNullException(nameof(faceServiceClient));
            this.faceWhitelist = faceWhitelist ?? throw new ArgumentNullException(nameof(faceWhitelist));
            delay = configuration.Delay.Milliseconds;
        }

        /// <summary>
        ///     Validates a Slack user's profile image.
        ///     The image is valid if it's recognized as an
        ///     image of a single human.
        /// </summary>
        /// <param name="user">The Slack user to validate.</param>
        /// <returns>The result of the face detection.</returns>
        public async Task<FaceDetectionResult> ValidateProfileImage(SlackUser user) {
            user.Guard();
            if (string.IsNullOrEmpty(user.Image)) {
                throw new ArgumentException(nameof(user));
            }

            try {
                if (await faceWhitelist.IsUserWhitelisted(user)) {
                    return FaceDetectionResult.Valid;
                }

                const int Tries = 9;
                for (var i = 0; i < Tries; ++i) {
                    try {
                        var faces = await faceServiceClient.DetectAsync(user.Image, false);
                        switch (faces.Length) {
                            case 0:
                                return new FaceDetectionResult("Kunne ikke se et ansikt i bildet ditt. Last opp et profilbilde av deg selv.");
                            case 1:
                                return FaceDetectionResult.Valid;
                            default:
                                return new FaceDetectionResult("Fant flere ansikter i bildet litt. Last opp et profilbilde av deg selv.");
                        }
                    }
                    catch (FaceAPIException) {
                        await Task.Delay(delay * i);
                    }
                }

                logger.Error("Did not complete image validation for " + user.Name);
                return FaceDetectionResult.Valid;
            }
            catch (Exception e) {
                logger.Error(e, "Face detection failed");
                return FaceDetectionResult.Valid;
            }
        }
    }
}