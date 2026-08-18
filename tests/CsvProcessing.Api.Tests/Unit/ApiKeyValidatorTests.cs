using CsvProcessing.Api.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CsvProcessing.Api.Tests.Unit
{
    public class ApiKeyValidatorTests
    {
        private const string ValidKey = "valid-key-12345";
        private const string RevokedKey = "revoked-key-12345";

        private static ApiKeyOptions CreateOptions() => new()
        {
            HeaderName = "X-Api-Key",
            Keys =
            [
                new ApiKeyEntry { Key = ValidKey, Owner = "current-client", Enabled = true },
                new ApiKeyEntry { Key = RevokedKey, Owner = "old-client", Enabled = false }
            ]
        };

        private static IOptionsMonitor<ApiKeyOptions> CreateMonitor()
        {
            var monitor = new Mock<IOptionsMonitor<ApiKeyOptions>>();
            monitor.Setup(m => m.CurrentValue).Returns(CreateOptions());
            return monitor.Object;
        }

        private static ApiKeyValidator CreateValidator() => new(CreateMonitor());

        [Fact]
        public void TryValidate_AcceptsAConfiguredKey_AndReportsItsOwner()
        {
            var validator = CreateValidator();

            Assert.True(validator.TryValidate(ValidKey, out var matched));
            Assert.NotNull(matched);
            Assert.Equal("current-client", matched!.Owner);
        }

        [Fact]
        public void TryValidate_RejectsADisabledKey()
        {
            Assert.False(CreateValidator().TryValidate(RevokedKey, out _));
        }

        [Fact]
        public void TryValidate_RejectsAnUnknownKey()
        {
            Assert.False(CreateValidator().TryValidate("random-key-12345", out _));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryValidate_RejectsMissingKeys(string? presented)
        {
            Assert.False(CreateValidator().TryValidate(presented, out _));
        }

        [Fact]
        public void TryValidate_IsCaseSensitive()
        {
            Assert.False(CreateValidator().TryValidate(ValidKey.ToUpperInvariant(), out _));
        }
    }
}
