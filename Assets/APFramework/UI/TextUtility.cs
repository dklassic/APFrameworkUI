using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public static class TextUtility
{
    public static string ColumnWithSpace = ": ";
    public static char FullsizeSpace = '　';
    public static string LineBreaker = FullsizeSpace + "\n";
    public static string TitleOpener = LineBreaker + FullsizeSpace;
    public static string FadeIn = "█▓▒░ ";
    public static string FadeOut = " ░▒▓█";
    public static string FadeInOut = " ░▒▓██▓▒░ ";
    public static string FadeOutIn = "█▓▒░  ░▒▓█";
    public static string CharacterASCII = "\"\'!#$%&()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ ‐‑–—―‗‘’‚‛“”„†‡•․…‰′″‹›‼‾⁄€™≡─┌┐└┘├┬┴┼═║╔╗╚╝╠╣╦╩╬▀▄█░▒▓■□ ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσςΤτΥυΦφΧχΨψΩω";
    public static StringBuilder StringBuilder = new StringBuilder();
    static StringBuilder internalStringBuilder = new StringBuilder();
    public static bool IsASCII(char text)
    {
        if (CharacterASCII.IndexOf(text) >= 0)
            return true;
        else
            return false;
    }
    public static bool AllASCII(string text)
    {
        foreach (char c in text)
        {
            if (!IsASCII(c))
                return false;
        }
        return true;
    }
    public static string Repeat(char c, int count)
    {
        if (count <= 0)
            return string.Empty;
        internalStringBuilder.Clear();
        for (int i = 0; i < count; i++)
        {
            internalStringBuilder.Append(c);
        }
        return internalStringBuilder.ToString();
    }
    public static string PlaceHolder(int count) => Repeat(' ', count);
    // pasted from https://forum.unity.com/threads/getting-the-text-without-tags.851455/
    public static string StripRichTagsFromStr(string richStr)
    {
        try
        {
            internalStringBuilder.Clear();
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
            //Debug.Log(strippedStr);

            return strippedStr;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError("[Common]**ERR @ StripRichTagsFromStr: " + e);
#endif
            return "";
        }
    }
    public static int ActualLength(string text)
    {
        if (text == null || text == string.Empty)
            return 0;
        float count = 0;
        float fullSizeCount = 0;
        string parsedText = StripRichTagsFromStr(text);
        for (int i = 0; i < parsedText.Length; i++)
        {
            if (IsASCII(parsedText[i]))
                count++;
            else
            {
                count += 2.5f - Mathf.Clamp(fullSizeCount * .075f, 0, .5f);
                fullSizeCount++;
            }
        }
        return (int)count;
    }
    public static int UnbiasedLength(string text)
    {
        if (text == null || text == string.Empty)
            return 0;
        float count = 0;
        string parsedText = StripRichTagsFromStr(text);
        for (int i = 0; i < parsedText.Length; i++)
        {
            if (IsASCII(parsedText[i]))
                count++;
            else
            {
                count += 2f;
            }
        }
        return (int)count;
    }
    /// <summary>
    /// AutoSplit is a utility that hopefully can give a good split point for a string.
    /// </summary>
    public static int AutoSplit(string text, int targetLength)
    {
        bool isAllASCII = AllASCII(text);
        if (isAllASCII)
        {
            float difference = Mathf.Infinity;
            string[] words = text.Split(' ');
            int accumulatedLength = 0;
            for (int i = 0; i < words.Length; i++)
            {
                int delta = i == 0 ? words[i].Length : words[i].Length + 1;
                int activeDifference = targetLength - (accumulatedLength + delta);
                if (activeDifference >= 0 && activeDifference < difference)
                    difference = activeDifference;
                else
                {
                    return accumulatedLength;
                }
                accumulatedLength += delta;
            }
        }
        return targetLength;
    }
    /// <summary>
    /// This method will return properly sliced string considering length limit
    /// </summary>
    internal static List<string> StringCutter(string text, int limit)
    {
        if (text == null || text == string.Empty)
            return new List<string>();
        int actualLimit = limit;
        if (AllASCII(text))
        {
            List<string> result = new List<string>();
            string[] split;
            split = text.Split(' ');
            int lineIndex = 0;
            for (int i = 0; i < split.Length; i++)
            {
                if (result.Count == lineIndex)
                    result.Add(split[i]);
                else if (ActualLength(result[lineIndex]) + ActualLength(split[i]) + 1 <= actualLimit)
                    result[lineIndex] = result[lineIndex] + " " + split[i];
                else
                {
                    lineIndex++;
                    result.Add(split[i]);
                }
            }
            return result;
        }
        else
        {
            List<string> result = new List<string>();
            string[] split;
            split = text.Split(' ');
            int lineIndex = 0;
            bool controlCodeDetected = false;
            for (int i = 0; i < split.Length; i++)
            {
                if (result.Count == lineIndex)
                    result.Add(string.Empty);
                if (AllASCII(split[i]) && ActualLength(result[lineIndex]) + ActualLength(split[i]) + 1 <= actualLimit)
                    result[lineIndex] = result[lineIndex] + " " + split[i];
                else if (!AllASCII(split[i]))
                {
                    for (int j = 0; j < split[i].Length; j++)
                    {
                        if (split[i][j] == '<')
                            controlCodeDetected = true;
                        if (controlCodeDetected)
                        {
                            result[lineIndex] = result[lineIndex] + split[i][j];
                            if (split[i][j] == '>')
                                controlCodeDetected = false;
                            continue;
                        }
                        if (ActualLength(result[lineIndex]) + (IsASCII(split[i][j]) ? 1 : 2) <= actualLimit)
                        {
                            if (result[lineIndex].Length != 0 && IsASCII(result[lineIndex][result[lineIndex].Length - 1]))
                                result[lineIndex] = result[lineIndex] + ' ' + split[i][j];
                            else
                                result[lineIndex] = result[lineIndex] + split[i][j];
                        }
                        else
                        {
                            lineIndex++;
                            result.Add(split[i][j].ToString());
                        }
                    }
                }
                else
                {
                    lineIndex++;
                    result.Add(split[i]);
                }
            }
            return result;
        }
    }
    public static string NAString => "<color=\"black\">Not Available</color>";
    public static int SubscriptCompensation(string text)
    {
        if (AllASCII(text))
            return 2;
        else
            return 0;
    }
    public static int TitleCompensation = 1;
}