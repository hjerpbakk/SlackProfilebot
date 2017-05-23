namespace Hjerpbakk.ProfileBot.Contracts {
    public struct ProfileValidationResult {
        public ProfileValidationResult(bool isValid, string userId, string errors) {
            IsValid = isValid;
            UserId = userId;
            Errors = errors;
        }

        public bool IsValid { get; }
        public string UserId { get; }
        public string Errors { get; }

        public static ProfileValidationResult CreateValid() =>
            new ProfileValidationResult(true, "", "");
    }
}