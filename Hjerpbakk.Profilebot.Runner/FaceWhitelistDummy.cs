using System.Threading.Tasks;
using Hjerpbakk.Profilebot.FaceDetection;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Runner {
    internal class FaceWhitelistDummy : IFaceWhitelist {
        public Task<bool> IsUserWhitelisted(SlackUser user) {
            return Task.FromResult(true);
        }

        public Task WhitelistUser(SlackUser user) {
            return Task.CompletedTask;
        }

        public Task UploadReport(ValidationReport report) {
            return Task.CompletedTask;
        }
    }
}