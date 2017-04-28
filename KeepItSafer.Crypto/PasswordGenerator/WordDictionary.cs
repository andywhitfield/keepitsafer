using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        private ICollection<string> words = new string[0];
        private readonly ManualResetEvent loadedEvent = new ManualResetEvent(false);
        
        public WordDictionary()
        {
        }

        public EventWaitHandle DictionaryLoadedWaitHandle { get { return loadedEvent; } }

        public ICollection<string> Words { get { return words; }}

        public async Task<int> LoadAsync()
        {
            if (loadedEvent.WaitOne(0))
            {
                return words.Count;
            }
            
            var dictWords = new List<string>(150000);
            var assem = typeof(WordDictionary).GetTypeInfo().Assembly;
            var word = new StringBuilder();
            foreach (var dict in DictionaryFiles)
            {
                using (var dictStream = new StreamReader(assem.GetManifestResourceStream(dict)))
                {
                    string line;
                    while ((line = await dictStream.ReadLineAsync()) != null)
                    {
                        // lots of magic numbers...
                        // the dictionary line has 16 characters of preamble, followed
                        // by a space (index 16), then the word followed by a space.
                        if (line.Length < 18 || line[0] == ' ' || line[16] != ' ')
                        {
                            continue;
                        }
                        word.Clear();
                        foreach (var c in line.Skip(17))
                        {
                            if (c == ' ')
                            {
                                break;
                            }
                            word.Append(c == '_' ? ' ' : c);
                        }
                        dictWords.Add(word.ToString());
                    }
                }
            }
            words = dictWords;
            loadedEvent.Set();
            return words.Count;
        }
    }
}