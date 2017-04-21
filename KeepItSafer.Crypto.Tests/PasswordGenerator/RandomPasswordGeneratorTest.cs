using System.Collections.Generic;
using System.Linq;
using KeepItSafer.Crypto.PasswordGenerator;
using Xunit;

namespace KeepItSafer.Crypto.Tests.PasswordGenerator
{
    public class RandomPasswordGeneratorTest
    {
        [Fact]
        public async void GeneratePasswordsWithExpectedLengths()
        {
            var dict = new WordDictionary();
            await dict.LoadAsync();
            var randomPasswordGenerator = new RandomPasswordGenerator(dict);

            randomPasswordGenerator.MaximumLength = 0;
            randomPasswordGenerator.MinimumLength = 12;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.True(password.Length >= 12);
            }

            randomPasswordGenerator.MinimumLength = 24;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.True(password.Length >= 24);
            }

            randomPasswordGenerator.MinimumLength = 10;
            randomPasswordGenerator.MaximumLength = 15;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.InRange(password.Length, 10, 15);
            }

            randomPasswordGenerator.MinimumLength = 20;
            randomPasswordGenerator.MaximumLength = 30;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.InRange(password.Length, 20, 30);
            }
        }

        [Fact]
        public async void GeneratePasswordsWithAllowedValues()
        {
            var dict = new WordDictionary();
            await dict.LoadAsync();
            var randomPasswordGenerator = new RandomPasswordGenerator(dict);

            randomPasswordGenerator.AllowNumbers = false;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.False(password.Any(c => char.IsNumber(c)));
            }

            randomPasswordGenerator.AllowNumbers = true;
            randomPasswordGenerator.AllowPunctuation = false;
            foreach (var password in GeneratePasswords(randomPasswordGenerator))
            {
                Assert.True(password.All(c => char.IsLetterOrDigit(c)));
            }
        }

        private IEnumerable<string> GeneratePasswords(RandomPasswordGenerator generator)
        {
            return Enumerable.Range(1, 100).Select(_ => generator.Generate());
        }
    }
}