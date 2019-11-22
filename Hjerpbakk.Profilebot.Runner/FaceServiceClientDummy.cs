using System.Threading.Tasks;
using Hjerpbakk.Profilebot.FaceDetection;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Runner {
    internal class FaceServiceClientDummy : IFaceDetectionClient {
        public async Task<FaceDetectionResult> ValidateProfileImage(SlackUser user) {
            return await Task.FromResult(FaceDetectionResult.Valid);
        }
    }
}