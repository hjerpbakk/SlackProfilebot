using System;
using System.IO;
using System.Linq;
using System.Text;
using Hjerpbakk.Profilebot.Contracts;

namespace Hjerpbakk.Profilebot.FaceDetection.Report {
    /// <summary>
    ///     A report of multiple profile validations.
    /// </summary>
    public class ValidationReport {
        // ReSharper disable once InconsistentNaming
        readonly string HTMLLineTemplate;

        readonly ProfileValidationResult[] validationResults;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="validationResults">The validations from which to create the report.</param>
        public ValidationReport(params ProfileValidationResult[] validationResults) {
            this.validationResults = validationResults ?? throw new ArgumentNullException(nameof(validationResults));
            HTMLLineTemplate = @"<tr><td>[Profile]</td><td>[Name]</td><td><img src=""[ImageURL]"" width=""250""/></td></tr></table>";
        }

        /// <summary>
        ///     Generates an HTML report from the validations.
        /// </summary>
        /// <returns>The HTML report</returns>
        public string CreateHTMLReport() {
            const string ReportFilePath = "FaceDetection/Report/Report.html";
            var html = new StringBuilder(File.ReadAllText(ReportFilePath));
            foreach (var validationResult in validationResults) {
                if (validationResult.ImageURL == null) {
                    continue;
                }

                var instance = HTMLLineTemplate.Replace("[Profile]", validationResult.User.Id);
                instance = instance.Replace("[Name]", validationResult.User.Name);
                instance = instance.Replace("[ImageURL]", validationResult.ImageURL.AbsoluteUri);
                html.Replace("</table>", instance);
            }

            return html.ToString();
        }

        /// <summary>
        ///     Creates a plain text representation of the report.
        /// </summary>
        /// <returns>A plain text representation of the report.</returns>
        public override string ToString() {
            return validationResults.Length == 0
                ? "No profiles contain errors :)"
                : $"{validationResults.Length} users have bad profiles:{Environment.NewLine}" +
                  $"{string.Join(", ", validationResults.Select(error => $"{error.User.FormattedUserId}{(error.ImageURL == null ? "" : " 🌅")}"))}";
        }
    }
}