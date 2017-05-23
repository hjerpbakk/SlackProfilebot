using System;
using Hjerpbakk.ProfileBot;
using Xunit;

namespace Test.Hjerpbakk.Profilebot {
    public class AdminUserTests {
        [Fact]
        public void Constructor() {
            const string AdminUserId = "AdminId";

            var adminUser = new AdminUser(AdminUserId);

            Assert.Equal(AdminUserId, adminUser.Id);
        }

        [Fact]
        public void Constructor_Fails() {
            var exception = Record.Exception(() => new AdminUser(null));

            Assert.IsType<ArgumentException>(exception);
        }
    }
}