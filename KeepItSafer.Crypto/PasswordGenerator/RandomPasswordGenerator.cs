using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KeepItSafer.Crypto.PasswordGenerator
{
    public class RandomPasswordGenerator
    {
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private readonly WordDictionary dictionary;
        private readonly ISet<string> endPunctuation = new HashSet<string> {",", ".", "!", "?", "$", "*", ":", ";"};
        private readonly ISet<string> middlePunctuation = new HashSet<string> {" ", ",", "-", "/", "'", "\"", "$", "%", "@", "(", ")", "&", "*", ":", ";"};
        private readonly ISet<string> allNumbers = new HashSet<string>(Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList());

        public RandomPasswordGenerator(WordDictionary dictionary)
        {
            this.dictionary = dictionary;
        }

        public bool AllowNumbers { get; set; } = true;
        public bool AllowPunctuation { get; set; } = true;
        public int MinimumLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 0;

        public string Generate()
        {
            var password = new StringBuilder();
            if (AllowNumbers)
            {
                password.Append(WordFrom(allNumbers));

                if (AllowPunctuation)
                {
                    password.Append(WordFrom(middlePunctuation));
                }
            }

            var addNumber = true;
            var wordsAdded = 0;
            var minPasswordLength = Math.Max(5, MinimumLength);
            if (MaximumLength > 0 && minPasswordLength > MaximumLength)
            {
                minPasswordLength = MaximumLength;
            }
            var maxPasswordLength = MaximumLength <= 0 ? int.MaxValue : Math.Max(MaximumLength, minPasswordLength);
            if (AllowPunctuation)
            {
                maxPasswordLength--;
            }

            dictionary.DictionaryLoadedWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            do
            {
                if (!addNumber)
                {
                    if (AllowPunctuation)
                    {
                        password.Append(wordsAdded % 2 == 0 ? WordFrom(middlePunctuation) : " ");
                    }
                    else if (AllowNumbers && wordsAdded % 2 == 0)
                    {
                        password.Append(WordFrom(allNumbers));
                    }
                }

                if (password.Length >= maxPasswordLength) break;

                string wordToAdd;
                while ((wordToAdd = WordFrom(dictionary.Words, maxWordLength: maxPasswordLength - password.Length)) == null) { }
                password.Append(wordToAdd);

                if (password.Length >= maxPasswordLength) break;

                addNumber = false;
                wordsAdded++;
            } while (password.Length < minPasswordLength);

            if (AllowPunctuation && password.Length < maxPasswordLength + 1)
            {
                password.Append(WordFrom(endPunctuation));
            }

            return password.ToString();
        }
        private string WordFrom(ICollection<string> words, bool checkFirstCharacter = true, string excluding = null, int? maxWordLength = null)
        {
            IEnumerable<string> eligibleWords = words;
            if (maxWordLength.HasValue && maxWordLength > 0)
            {
                eligibleWords = words.Where(w => w.Length <= maxWordLength).ToList();
            }

            if (!eligibleWords.Any())
            {
                return null;
            }

            string word;
            do
            {
                word = eligibleWords.ElementAt(NextRandom(eligibleWords.Count()));
                if (word.Length == 0) continue;

                if (AllowPunctuation)
                {
                    word = word.Replace('_', ' ');
                }
                else
                {
                    var wordSb = new StringBuilder(word);
                    var idx = 0;
                    while (idx < wordSb.Length)
                    {
                        if (!Char.IsLetterOrDigit(wordSb[idx]))
                            wordSb.Remove(idx, 1);
                        else idx++;
                    }
                    word = wordSb.ToString();
                }

                if (Char.IsLetter(word[0]))
                {
                    word = Char.ToUpper(word[0]) + word.Substring(1);
                }
            } while (word.Length == 0 && word != excluding); 

            return word;
        }

        private static int NextRandom(int setSize)
        {
            var randomBuffer = new byte[4];
            rng.GetBytes(randomBuffer);
            var val = BitConverter.ToInt32(randomBuffer, 0) & 0x7fffffff;
            return val % setSize;
        }
    }
}