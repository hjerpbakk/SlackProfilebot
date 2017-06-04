namespace Hjerpbakk.Profilebot.Configuration {
    public struct FaceDetectionAPI {
        public FaceDetectionAPI(string key) {
            Key = key;
        }

        public string Key { get; }
    }
}