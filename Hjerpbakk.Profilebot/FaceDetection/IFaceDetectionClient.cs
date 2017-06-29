using System.Threading.Tasks;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.FaceDetection {
    /// <summary>
    ///     Client for recognizing faces i Slack profile images.
    /// </summary>
    public interface IFaceDetectionClient {
        /// <summary>
        ///     Validates a Slack user's profile image.
        ///     The image is valid if it's recognized as an
        ///     image of a single human.
        /// </summary>
        /// <param name="user">The Slack user to validate.</param>
        /// <returns>The result of the face detection.</returns>
        Task<FaceDetectionResult> ValidateProfileImage(SlackUser user);
    }
}