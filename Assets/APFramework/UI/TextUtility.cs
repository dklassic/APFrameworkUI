using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Unity.Burst;
using Cysharp.Text;

public static class TextUtility
{
    public const char column = ':';
    public const char FULL_SIZE_SPACE = '　';
    public static string LineBreaker = FULL_SIZE_SPACE + "\n";
    public static string TitleOpener = LineBreaker + FULL_SIZE_SPACE;
    public const string FADE_IN = "█▓▒░ ";
    public const string FADE_OUT = " ░▒▓█";
    public const string FADE_IN_OUT = " ░▒▓██▓▒░ ";
    public const string FADE_OUT_IN = "█▓▒░  ░▒▓█";
    public const char UNDERSCORE = '_';

    public const string ASCII_CHARACTERS =
        "\"\'!#$%&()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ ‐‑–—―‗‘’‚‛“”„†‡•․…‰′″‹›‼‾⁄€™≡─┌┐└┘├┬┴┼═║╔╗╚╝╠╣╦╩╬▀▄█░▒▓■□ ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσςΤτΥυΦφΧχΨψΩω";

    [BurstCompile]
    public static bool IsAscii(char text)
    {
        return ASCII_CHARACTERS.Contains(text);
    }

    [BurstCompile]
    public static bool AllAscii(string text)
    {
        foreach (char c in text)
        {
            if (!IsAscii(c))
                return false;
        }

        return true;
    }

    [BurstCompile]
    public static bool IsSingleControlCode(string text)
    {
        return text.StartsWith("<") && text.EndsWith(">") && text.Count(c => c == '<') == 1 &&
               text.Count(c => c == '>') == 1;
    }

    [BurstCompile]
    public static bool HasUnderScore(string text)
    {
        return text.Contains(UNDERSCORE);
    }

    [BurstCompile]
    public static string Repeat(char c, int count)
    {
        if (count <= 0)
            return string.Empty;
        using (var internalStringBuilder = ZString.CreateStringBuilder())
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
            using (var internalStringBuilder = ZString.CreateStringBuilder())
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

                // -----------------------------------
                string strippedStr = internalStringBuilder.ToString();
                //Context.Log(strippedStr);
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

    [BurstCompile]
    public static int ActualLength(string text)
    {
        if (IsSingleControlCode(text))
            return 0;
        if (string.IsNullOrEmpty(text))
            return 0;
        int count = 0;
        string parsedText = StripRichTagsFromStr(text);
        for (int i = 0; i < parsedText.Length; i++)
        {
            if (IsAscii(parsedText[i]))
                count++;
            else
            {
                count += 2;
            }
        }

        return count;
    }
    
    [BurstCompile]
    public static List<string> SplitStringByControlCode(string input)
    {
        // Define a regex pattern to match the control codes and text outside of them
        string pattern = @"(<[^>]+>)|([^<]+)";

        List<string> result = new List<string>();
        Regex regex = new Regex(pattern);

        // Find matches in the input string
        var matches = regex.Matches(input);
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
    /// When the text is all ASCII, it will split the text by space and try to fit the text into the limit.
    /// When the text is not all ASCII, it will split the text by space and try to fit the text into the limit.
    /// Extra care is needed as linebreak symbols will also count as "not ascii"
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
            if (AllAscii(word))
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
                if (accumulatedLength + ActualLength(candidate) + 1 > limit)
                {
                    results.Add(internalStringBuilder.ToString());
                    internalStringBuilder.Clear();
                    lastCharacter = string.Empty;
                    lastWord = string.Empty;
                    accumulatedLength = 0;
                }

                // if (internalStringBuilder.Length > 0)
                if (internalStringBuilder.Length > 0 && lastCharacter != string.Empty &&
                    !IsSingleControlCode(candidate) &&
                    !IsSingleControlCode(lastWord) &&
                    (AllAscii(lastCharacter) || AllAscii(candidate)))
                {
                    internalStringBuilder.Append(' ');
                    accumulatedLength++;
                }

                lastCharacter = candidate != string.Empty ? candidate.Substring(candidate.Length - 1) : string.Empty;
                lastWord = candidate;
                internalStringBuilder.Append(candidate);
                accumulatedLength += ActualLength(candidate);
            }

            if (internalStringBuilder.Length > 0)
            {
                results.Add(internalStringBuilder.ToString());
                internalStringBuilder.Clear();
            }
        }

        return results;
    }
}