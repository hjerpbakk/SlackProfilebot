using System;

namespace Hjerpbakk.ProfileBot {
    internal struct SlackStringUser {
        public SlackStringUser(string slackUserIdAsString) {
            SlackUserIdAsString = slackUserIdAsString ?? throw new ArgumentNullException(nameof(slackUserIdAsString));
            if (slackUserIdAsString[0] != '<' || slackUserIdAsString[1] != '@' ||
                slackUserIdAsString[slackUserIdAsString.Length - 1] != '>') {
                throw new ArgumentException("Slack user id given in wrong format. The correct format is <@SLACK_ID>.",
                    nameof(slackUserIdAsString));
            }

            UserId = slackUserIdAsString.Substring(2, slackUserIdAsString.Length - 3);
        }

        public string SlackUserIdAsString { get; }
        public string UserId { get; }
    }
}