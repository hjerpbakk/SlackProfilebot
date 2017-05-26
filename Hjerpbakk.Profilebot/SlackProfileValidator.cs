using System;
using System.Text;
using SlackConnector.Models;
using Hjerpbakk.ProfileBot.Contracts;

namespace Hjerpbakk.ProfileBot {
    /// <summary>
    /// Validates the profile of a Slack user according to your team's rules.
    /// </summary>
    public class SlackProfileValidator : ISlackProfileValidator {
        readonly string adminUserId;

        /// <summary>
        /// Creates the SlackProfileValidator.
        /// </summary>
        /// <param name="adminUser">The admin of this Slack team.</param>
        public SlackProfileValidator(AdminUser adminUser) {
            if (string.IsNullOrEmpty(adminUser.Id)) {
                throw new ArgumentException(nameof(adminUser.Id));
            }

            adminUserId = adminUser.Id;
        }

        /// <summary>
        /// Validates that a user profile is complete.
        /// </summary>
        /// <param name="user">The user to be validated.</param>
        /// <returns>The result of the validation.</returns>
        public ProfileValidationResult ValidateProfile(SlackUser user) {
            Verify(user);

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

            ValidateProfileImage(user, errors);

            var actualErrors = errors.ToString();
            return actualErrors.Length == 0 || user.IsBot || user.Deleted || user.Name == "slackbot"
                ? new ProfileValidationResult(true, null, null)
                : new ProfileValidationResult(false, user.Id,
                    $"Hei <@{user.Id}>, jeg har sett gjennom profilen din og den har følgende mangler:" +
                    $"{Environment.NewLine}{actualErrors}{Environment.NewLine}" +
                    "Se https://utvikling/t/slack/1822 for hva som kreves av en fullt utfylt profil." +
                    $"{Environment.NewLine}Ta kontakt med <@{adminUserId}> dersom du har spørsmål.");
        }

        static void Verify(SlackUser user) {
            if (user == null) {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Id)) {
                throw new ArgumentException("Id cannot be empty.", nameof(user));
            }

            if (string.IsNullOrEmpty(user.Name)) {
                throw new ArgumentException("Name cannot be empty.", nameof(user));
            }
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

        static void ValidateProfileImage(SlackUser user, StringBuilder errors) {
            if (string.IsNullOrEmpty(user.Image)) {
                errors.AppendLine("Legg inn et profilbilde slik at folk kjenner deg igjen.");
            }
        }
    }
}