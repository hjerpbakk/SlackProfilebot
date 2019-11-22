using System;
using Hjerpbakk.Profilebot.FaceDetection.Report;
using Hjerpbakk.Profilebot.Contracts;
using SlackConnector.Models;
using Xunit;

namespace Test.Hjerpbakk.Profilebot.Report {
    public class ValidationReportTests {
        [Fact]
        public void AsText_NoProfileErrors() {
            var report = new ValidationReport();

            Assert.Equal("No profiles contain errors :)", report.ToString());
        }

        [Fact]
        public void AsText_ProfilesWithErrors() {
            var report = new ValidationReport(new ProfileValidationResult(new SlackUser {Id = "User1", Name = "User 1"}, "errors"), new ProfileValidationResult(new SlackUser {Id = "User2", Name = "User 2"}, "errors"));

            Assert.Equal("2 users have bad profiles:\r\n<@User1>, <@User2>", report.ToString());
        }

        [Fact]
        public void Constructor_NoResult_DoesNotFail() {
            var exception = Record.Exception(() => new ValidationReport());

            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_Null_Fails() {
            var exception = Record.Exception(() => new ValidationReport(null));

            Assert.IsType<ArgumentNullException>(exception);
        }

        [Fact]
        public void CreateHTMLReport_2ValidationsWithBadImages_TheTwoUsersAreIncludedInReport() {
            var report = new ValidationReport(
                new ProfileValidationResult(new SlackUser {Id = "User1", Name = "User 1"}, "errors", new Uri("http://suspect1.jpg")),
                new ProfileValidationResult(new SlackUser {Id = "User2", Name = "User 2"}, "errors"),
                new ProfileValidationResult(new SlackUser {Id = "User3", Name = "User 3"}, "errors", new Uri("http://suspect3.jpg")));

            var html = report.CreateHTMLReport();

            Assert.Contains("User1", html);
            Assert.Contains("User 1", html);
            Assert.Contains("http://suspect1.jpg", html);
            Assert.DoesNotContain("User2", html);
            Assert.DoesNotContain("User 2", html);
            Assert.DoesNotContain("http://suspect2.jpg", html);
            Assert.Contains("User3", html);
            Assert.Contains("User 3", html);
            Assert.Contains("http://suspect3.jpg", html);
        }

        [Fact]
        public void ToString_2ValidationsWithBadImages_TheTwoUsersAreIncludedInReport() {
            var report = new ValidationReport(
                new ProfileValidationResult(new SlackUser {Id = "User1", Name = "User 1"}, "errors", new Uri("http://suspect1.jpg")),
                new ProfileValidationResult(new SlackUser {Id = "User2", Name = "User 2"}, "errors"),
                new ProfileValidationResult(new SlackUser {Id = "User3", Name = "User 3"}, "errors", new Uri("http://suspect3.jpg")));

            var text = report.ToString();

            Assert.Equal($"3 users have bad profiles:{Environment.NewLine}<@User1> 🌅, <@User2>, <@User3> 🌅", text);
        }
    }
}