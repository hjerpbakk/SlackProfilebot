using System;
using System.Text;
using System.Threading.Tasks;
using Hjerpbakk.Profilebot.Contracts;
using Hjerpbakk.Profilebot.FaceDetection;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot {
    /// <summary>
    ///     Validates the profile of a Slack user according to your team's rules.
    /// </summary>
    public class SlackProfileValidator : ISlackProfileValidator {
        readonly string adminUserId;
        readonly IFaceDetectionClient faceDetectionClient;

        /// <summary>
        ///     Creates the SlackProfileValidator.
        /// </summary>
        /// <param name="adminUser">The admin of this Slack team.</param>
        /// <param name="faceDetectionClient"></param>
        public SlackProfileValidator(SlackUser adminUser, IFaceDetectionClient faceDetectionClient) {
            adminUser.Guard();
            adminUserId = adminUser.Id;
            this.faceDetectionClient = faceDetectionClient ?? throw new ArgumentNullException(nameof(faceDetectionClient));
        }

        /// <summary>
        ///     Validates that a user profile is complete.
        /// </summary>
        /// <param name="user">The user to be validated.</param>
        /// <returns>The result of the validation.</returns>
        public async Task<ProfileValidationResult> ValidateProfile(SlackUser user) {
            user.Guard();
            if (string.IsNullOrEmpty(user.Name)) {
                throw new ArgumentException("Name cannot be empty.", nameof(user));
            }

            var errors = new StringBuilder();

            ValidateEmail(user, errors);

            if (string.IsNullOrEmpty(user.FirstName)) {
                errors.AppendLine("Fornavn må registreres slik at folk vet hvem du er.");
            }

            if (string.IsNullOrEmpty(user.LastName)) {
                errors.AppendLine("Etternavn må registreres slik at folk vet hvem du er.");
            }

            if (string.IsNullOrEmpty(user.WhatIDo)) {
                errors.AppendLine("Feltet \"What I do\" må inneholde team og hva du kan i DIPS.");
            }

            var imageWasSuspect = await ValidateProfileImage(user, errors);

            var actualErrors = errors.ToString();
            return actualErrors.Length == 0 || user.IsBot || user.Deleted || user.Name == "slackbot"
                ? ProfileValidationResult.Valid(user)
                : new ProfileValidationResult(user,
                    $"Hei <@{user.Id}>, jeg har sett gjennom profilen din og den har følgende mangler:" +
                    $"{Environment.NewLine}{Environment.NewLine}{actualErrors}{Environment.NewLine}" +
                    "Se https://utvikling/t/slack/1822 for hva som kreves av en fullt utfylt profil." +
                    $"{Environment.NewLine}Ta kontakt med <@{adminUserId}> dersom du har spørsmål.", imageWasSuspect ? new Uri(user.Image) : null);
        }

        static void ValidateEmail(SlackUser user, StringBuilder errors) {
            if (string.IsNullOrEmpty(user.Email)) {
                errors.AppendLine("Din DIPS-epost må være registrert på brukeren din.");
            }
            else {
                if (!user.Email.EndsWith("dips.no")) {
                    errors.AppendLine("Kun DIPS-epost skal benyttes.");
                }

                if (!user.Email.StartsWith(user.Name)) {
                    errors.AppendLine(
                        "Brukernavnet ditt skal kun være dine tre DIPS-bokstaver. Dette kan endres via https://dipsasa.slack.com/account/settings");
                }
            }
        }

        async Task<bool> ValidateProfileImage(SlackUser user, StringBuilder errors) {
            if (string.IsNullOrEmpty(user.Image)) {
                errors.AppendLine("Legg inn et profilbilde slik at folk kjenner deg igjen.");
                return false;
            }

            var imageValidationResult = await faceDetectionClient.ValidateProfileImage(user);
            if (imageValidationResult.IsValid) {
                return false;
            }

            errors.AppendLine(imageValidationResult.Errors);
            return true;
        }
    }
}