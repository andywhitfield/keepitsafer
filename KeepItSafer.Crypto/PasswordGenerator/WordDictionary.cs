using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KeepItSafer.Crypto.PasswordGenerator
{
    public class WordDictionary
    {
        private const string BaseResourcePath = "KeepItSafer.Crypto.Dictionary.";
        private static readonly string[] DictionaryFiles = new[] {
            BaseResourcePath + "data.adj",
            BaseResourcePath + "data.adv",
            BaseResourcePath + "data.noun",
            BaseResourcePath + "data.verb"
        };
        private static Regex WordPattern = new Regex("^[0-9]{8} [0-9]{2} . [0-9]{2} (?<word>[^\\s]+) .*$", RegexOptions.Compiled | RegexOptions.Singleline);

        private ISet<string> words = new HashSet<string>();
        private readonly ManualResetEvent loadedEvent = new ManualResetEvent(false);
        
        public WordDictionary()
        {
        }

        public EventWaitHandle DictionaryLoadedWaitHandle { get { return loadedEvent; } }

        public ISet<string> Words { get { return words; }}

        public async Task<int> LoadAsync()
        {
            if (loadedEvent.WaitOne(0))
            {
                return words.Count;
            }
            
            var dictWords = new HashSet<string>();
            var assem = typeof(WordDictionary).GetTypeInfo().Assembly;
            foreach (var dict in DictionaryFiles)
            {
                using (var dictStream = new StreamReader(assem.GetManifestResourceStream(dict)))
                {
                    string line;
                    while ((line = await dictStream.ReadLineAsync()) != null)
                    {
                        var matches = WordPattern.Matches(line);
                        if (matches.Count != 1)
                        {
                            continue;
                        }
                        dictWords.Add(matches[0].Groups["word"].Value.Replace('_', ' '));
                    }
                }
            }
            words = dictWords;
            loadedEvent.Set();
            return words.Count;
        }
    }
}