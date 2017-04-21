using KeepItSafer.Crypto.PasswordGenerator;
using Xunit;

namespace KeepItSafer.Crypto.Tests.PasswordGenerator
{
    public class WordDictionaryTest
    {
        [Fact]
        public async void LoadDictionaryFilesAsync()
        {
            var dict = new WordDictionary();
            Assert.Equal(0, dict.Words.Count);
            Assert.False(dict.DictionaryLoadedWaitHandle.WaitOne(0));

            var numberOfDictionaryWords = await dict.LoadAsync();

            Assert.True(dict.DictionaryLoadedWaitHandle.WaitOne(0));
            Assert.True(numberOfDictionaryWords > 0);

            var numberOfDictionaryWords2 = await dict.LoadAsync();
            Assert.True(dict.DictionaryLoadedWaitHandle.WaitOne(0));
            Assert.Equal(numberOfDictionaryWords, numberOfDictionaryWords2);
        }
    }
}