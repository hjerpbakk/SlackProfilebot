using System;
using SlackConnector.Models;

namespace Hjerpbakk.Profilebot.Contracts {
    public static class SlackUserExtensions {
        public static void Guard(this SlackUser slackUser) {
            if (slackUser == null) {
                throw new ArgumentNullException(nameof(slackUser));
            }

            if (string.IsNullOrEmpty(slackUser.Id)) {
                throw new ArgumentException(nameof(slackUser));
            }
        }
    }
}