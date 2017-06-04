using System;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Configuration;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using SlackConnector.Models;

namespace Hjerpbakk.ProfileBot.FaceDetection {
    public class FaceDetectionClient : IFaceDetectionClient {
        readonly FaceServiceClient faceServiceClient;

        public FaceDetectionClient(FaceDetectionAPI faceDetectionAPI) {
            // TODO: Dummy dersom Key er null. Test med en config som ikke inneholder keyen.
            faceServiceClient = new FaceServiceClient(faceDetectionAPI.Key, "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
        }

        public async Task<(bool IsValid, string Errors)> ValidateProfileImage(SlackUser user) {
            Face[] faces;
            try {
                faces = await faceServiceClient.DetectAsync(user.Image, false);
            }
            catch (Exception e) {
                Console.WriteLine(e);
                return (true, "");
            }

            switch (faces.Length) {
                case 0:
                    return (false, "Kunne ikke se et ansikt i bildet ditt. Last opp et profilbilde av deg selv.");
                case 1:
                    return (true, "");
                default:
                    return (false, "Fant flere ansikter i bildet litt. Last opp et profilbilde av deg selv.");
            }
        }

        public void Dispose() {
            faceServiceClient.Dispose();
        }
    }

    public interface IFaceDetectionClient : IDisposable {
        Task<(bool IsValid, string Errors)> ValidateProfileImage(SlackUser user);
    }
}