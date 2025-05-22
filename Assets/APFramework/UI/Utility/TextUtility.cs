using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.Burst;
using Cysharp.Text;

namespace ChosenConcept.APFramework.UI.Utility
{
    public static class TextUtility
    {
        public const string NA = "N/A";
        public const string COLUMN = ": ";
        public const char FULL_WIDTH_SPACE = '　';
        public const char LINE_BREAK = '\n';
        public static string LineBreaker = FULL_WIDTH_SPACE + "\n";
        public static string TitleOpener = LineBreaker + FULL_WIDTH_SPACE;
        public const char BLOCK = '█';
        public const string FADE_IN = "█▓▒░ ";
        public const string FADE_OUT = " ░▒▓█";
        public const string FADE_IN_OUT = " ░▒▓██▓▒░ ";
        public const string FADE_OUT_IN = "█▓▒░  ░▒▓█";
        public const char UNDERSCORE = '_';

        [BurstCompile]
        public static bool IsFullWidth(char c)
        {
            return c >= 0x1100 &&
                   (c <= 0x115f || // Hangul Jamo init. consonants
                    c == 0x2329 || c == 0x232a ||
                    (c >= 0x2e80 && c <= 0xa4cf &&
                     c != 0x303f) || // CJK ... Yi
                    (c >= 0xac00 && c <= 0xd7a3) || // Hangul Syllables
                    (c >= 0xf900 && c <= 0xfaff) || // CJK Compatibility Ideographs
                    (c >= 0xfe10 && c <= 0xfe19) || // Vertical forms
                    (c >= 0xfe30 && c <= 0xfe6f) || // CJK Compatibility Forms
                    (c >= 0xff00 && c <= 0xff60) || // Fullwidth Forms
                    (c >= 0xffe0 && c <= 0xffe6) ||
                    (c >= 0x20000 && c <= 0x2fffd) ||
                    (c >= 0x30000 && c <= 0x3fffd));
        }

        public static int CalcCharLength(char c)
        {
            return c switch
            {
                '\0' => 0,
                _ => IsFullWidth(c) ? 2 : 1
            };
        }

        [BurstCompile]
        public static bool AllHalfWidth(string text)
        {
            foreach (char c in text)
            {
                if (IsFullWidth(c))
                    return false;
            }

            return true;
        }

        [BurstCompile]
        public static bool IsSingleControlCode(string text)
        {
            int upper = 0;
            int lower = 0;
            foreach (var c in text)
            {
                if (c == '<')
                {
                    upper++;
                    if (upper > 1)
                        break;
                }
                else if (c == '>')
                {
                    lower++;
                    if (lower > 1)
                        break;
                }
            }

            return text.StartsWith("<") && text.EndsWith(">") && upper == 1 &&
                   lower == 1;
        }

        [BurstCompile]
        public static string Repeat(char c, int count)
        {
            if (count <= 0)
                return string.Empty;
            using (Utf16ValueStringBuilder internalStringBuilder = ZString.CreateStringBuilder())
            {
                for (int i = 0; i < count; i++)
                {
                    internalStringBuilder.Append(c);
                }

                return internalStringBuilder.ToString();
            }
        }

        [BurstCompile]
        public static string PlaceHolder(int count) => Repeat(' ', count);

        // pasted from https://forum.unity.com/threads/getting-the-text-without-tags.851455/
        [BurstCompile]
        public static string StripRichTagsFromStr(string richStr)
        {
            try
            {
                using (Utf16ValueStringBuilder internalStringBuilder = ZString.CreateStringBuilder())
                {
                    bool tag = false;
                    for (int index = 0; index < richStr.Length; index++)
                    {
                        char c = richStr[index];
                        if (tag)
                        {
                            if (c == '>')
                            {
                                tag = false;
                            }
                        }
                        else
                        {
                            if (c == '<')
                            {
                                tag = true;
                            }
                            else
                            {
                                internalStringBuilder.Append(c);
                            }
                        }
                    }

                    string strippedStr = internalStringBuilder.ToString();
                    return strippedStr;
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError("[Common]**ERR @ StripRichTagsFromStr: " + e);
#endif
                return "";
            }
        }

        [BurstCompile]
        public static int RichTagsStrippedLength(string richStr)
        {
            try
            {
                int count = 0;
                bool tag = false;
                for (int index = 0; index < richStr.Length; index++)
                {
                    char c = richStr[index];
                    if (tag)
                    {
                        if (c == '>')
                        {
                            tag = false;
                        }
                    }
                    else
                    {
                        if (c == '<')
                        {
                            tag = true;
                        }
                        else
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError("[Common]**ERR @ StripRichTagsFromStr: " + e);
#endif
                return 0;
            }
        }

        public static int RichTagsStrippedLength(Utf16ValueStringBuilder builder)
        {
            ReadOnlySpan<char> builderString = builder.AsSpan();
            try
            {
                int count = 0;
                bool tag = false;
                for (int index = 0; index < builder.Length; index++)
                {
                    char c = builderString[index];
                    if (tag)
                    {
                        if (c == '>')
                        {
                            tag = false;
                        }
                    }
                    else
                    {
                        if (c == '<')
                        {
                            tag = true;
                        }
                        else
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError("[Common]**ERR @ StripRichTagsFromStr: " + e);
#endif
                return 0;
            }
        }

        [BurstCompile]
        // This count with control code stripped
        public static int WidthSensitiveLength(string text)
        {
            if (IsSingleControlCode(text))
                return 0;
            if (string.IsNullOrEmpty(text))
                return 0;
            try
            {
                int count = 0;
                bool tag = false;
                for (int index = 0; index < text.Length; index++)
                {
                    char c = text[index];
                    if (tag)
                    {
                        if (c == '>')
                        {
                            tag = false;
                        }
                    }
                    else
                    {
                        if (c == '<')
                        {
                            tag = true;
                        }
                        else
                        {
                            count += CalcCharLength(text[index]);
                        }
                    }
                }

                return count;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError("[Common]**ERR @ StripRichTagsFromStr: " + e);
#endif
                return 0;
            }
        }

        [BurstCompile]
        public static List<string> SplitStringByControlCode(string input)
        {
            // Define a regex pattern to match the control codes and text outside of them
            string pattern = @"(<[^>]+>)|([^<]+)";

            List<string> result = new List<string>();
            Regex regex = new(pattern);

            // Find matches in the input string
            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                if (match.Groups[1].Success) // Match for control codes
                {
                    result.Add(match.Groups[1].Value);
                }
                else if (match.Groups[2].Success) // Match for regular text
                {
                    result.Add(match.Groups[2].Value);
                }
            }

            return result;
        }

        /// <summary>
        /// This method will return properly sliced string considering length limit
        /// <param name="text">The text to be sliced</param>
        /// <param name="limit">The limit of the text</param>
        /// </summary>
        [BurstCompile]
        public static List<string> StringCutter(string text, int limit)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>
                {
                    ""
                };
            List<string> results = new List<string>();
            List<string> initialSplit = SplitStringByControlCode(text);
            List<string> finalSplit = new();
            foreach (string s in initialSplit)
            {
                string[] words = s.Split(' ');
                finalSplit.AddRange(words);
            }

            List<string> candidates = new();
            foreach (string word in finalSplit)
            {
                if (word == string.Empty)
                    continue;
                if (AllHalfWidth(word))
                {
                    candidates.Add(word);
                }
                else
                {
                    foreach (char character in word)
                    {
                        candidates.Add(character.ToString());
                    }
                }
            }

            string lastCharacter = string.Empty;
            string lastWord = string.Empty;
            int accumulatedLength = 0;
            using (Utf16ValueStringBuilder internalStringBuilder = ZString.CreateStringBuilder())
            {
                foreach (string candidate in candidates)
                {
                    if (accumulatedLength + WidthSensitiveLength(candidate) + 1 > limit)
                    {
                        results.Add(internalStringBuilder.ToString());
                        internalStringBuilder.Clear();
                        lastCharacter = string.Empty;
                        lastWord = string.Empty;
                        accumulatedLength = 0;
                    }

                    if (internalStringBuilder.Length > 0 && lastCharacter != string.Empty &&
                        !IsSingleControlCode(candidate) &&
                        !IsSingleControlCode(lastWord) &&
                        (AllHalfWidth(lastCharacter) || AllHalfWidth(candidate)))
                    {
                        internalStringBuilder.Append(' ');
                        accumulatedLength++;
                    }

                    lastCharacter = candidate != string.Empty
                        ? candidate.Substring(candidate.Length - 1)
                        : string.Empty;
                    lastWord = candidate;
                    internalStringBuilder.Append(candidate);
                    accumulatedLength += WidthSensitiveLength(candidate);
                }

                if (internalStringBuilder.Length > 0)
                {
                    results.Add(internalStringBuilder.ToString());
                    internalStringBuilder.Clear();
                }
            }

            return results;
        }

        /// <summary>
        /// This method will return properly sliced string considering length limit
        /// <param name="text">The text to be sliced</param>
        /// <param name="limit">The limit of the text</param>
        /// </summary>
        [BurstCompile]
        public static int StringCutterLineCount(string text, int limit)
        {
            if (string.IsNullOrEmpty(text))
                return 1;
            List<string> initialSplit = SplitStringByControlCode(text);
            List<string> finalSplit = new();
            foreach (string s in initialSplit)
            {
                string[] words = s.Split(' ');
                finalSplit.AddRange(words);
            }

            List<string> candidates = new();
            foreach (string word in finalSplit)
            {
                if (word == string.Empty)
                    continue;
                if (AllHalfWidth(word))
                {
                    candidates.Add(word);
                }
                else
                {
                    foreach (char character in word)
                    {
                        candidates.Add(character.ToString());
                    }
                }
            }

            string lastCharacter = string.Empty;
            string lastWord = string.Empty;
            int accumulatedLength = 0;
            int lineCount = 0;

            foreach (string candidate in candidates)
            {
                if (accumulatedLength + WidthSensitiveLength(candidate) + 1 > limit)
                {
                    lastCharacter = string.Empty;
                    lastWord = string.Empty;
                    accumulatedLength = 0;
                    lineCount++;
                }

                if (accumulatedLength > 0 && lastCharacter != string.Empty &&
                    !IsSingleControlCode(candidate) &&
                    !IsSingleControlCode(lastWord) &&
                    (AllHalfWidth(lastCharacter) || AllHalfWidth(candidate)))
                {
                    accumulatedLength++;
                }

                lastCharacter = candidate != string.Empty ? candidate.Substring(candidate.Length - 1) : string.Empty;
                lastWord = candidate;
                accumulatedLength += WidthSensitiveLength(candidate);
            }

            if (accumulatedLength > 0)
            {
                lineCount++;
            }


            return lineCount;
        }
    }
}