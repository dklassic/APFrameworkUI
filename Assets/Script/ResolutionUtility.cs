using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public static class ResolutionUtility
{
    public static List<(int, int)> AvailableResolutions()
    {
        var resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct();
        Resolution maxRes = resolutions.ToList()[resolutions.ToList().Count - 1];
        List<(int, int)> resolutionsList = new List<(int, int)>();
        if (maxRes.height % 10 != 0 && maxRes.height != 768)
        {
            foreach (var res in resolutions)
            {
                if (Mathf.Approximately((float)maxRes.height / (float)maxRes.width, (float)res.height / (float)res.width))
                {
                    // This is here to prevent too many resolutions from being offered.
                    if (res.height % 10 != 0 && res.height != 768)
                        continue;
                    resolutionsList.Add((res.width, res.height));
                }
            }
        }            // if max resolution is weird, do a 16 by 9 resolution check
        else if (resolutionsList.Count == 0 && Mathf.Abs(9f * (float)maxRes.width / 16f / (float)maxRes.height - 1) < .01f)
        {
            foreach (var res in resolutions)
            {
                if (Mathf.Approximately(9f / 16f, (float)res.height / (float)res.width))
                {
                    // This is here to prevent too many resolutions from being offered.
                    if (res.height % 10 != 0 && res.height != 768)
                        continue;
                    resolutionsList.Add((res.width, res.height));
                }
            }
        }
        // if above method failed, might as well just let it flow
        else
        {
            foreach (var res in resolutions)
            {
                if (Mathf.Approximately((float)maxRes.height / (float)maxRes.width, (float)res.height / (float)res.width))
                {
                    resolutionsList.Add((res.width, res.height));
                }
            }
        }
        // Should at least pity add the max resolution
        if (!resolutionsList.Contains((maxRes.width, maxRes.height)))
            resolutionsList.Add((maxRes.width, maxRes.height));
        // if there's only one max ratio resolution available, try to give the second largest as option if possible
        if (resolutionsList.Count <= 1 && resolutions.ToList().Count > 1)
        {
            Resolution secondRes = resolutions.ToList()[resolutions.ToList().Count - 2];
            bool found = false;
            for (int i = resolutions.ToList().Count - 2; i >= 0; i--)
            {
                Resolution candidateRes = resolutions.ToList()[i];
                if (Mathf.Approximately((float)candidateRes.height / (float)candidateRes.width, (float)maxRes.height / (float)maxRes.width))
                    continue;
                secondRes = candidateRes;
                found = true;
                break;
            }
            if (found)
            {
                foreach (var res in resolutions)
                {
                    if (!Mathf.Approximately((float)secondRes.height / (float)secondRes.width, (float)res.height / (float)res.width))
                        continue;
                    // This is here to prevent too many resolutions from being offered.
                    if (res.height % 10 != 0 && res.height != 768)
                        continue;
                    resolutionsList.Add((res.width, res.height));
                }
                // if no resolution was added, just let it go
                if (resolutionsList.Count <= 1)
                {
                    foreach (var res in resolutions)
                    {
                        if (!Mathf.Approximately((float)secondRes.height / (float)secondRes.width, (float)res.height / (float)res.width))
                            continue;
                        resolutionsList.Add((res.width, res.height));
                    }
                }
            }
        }
        resolutionsList.Sort((a, b) => b.Item1 - a.Item1);
        return resolutionsList;
    }
    public static void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreenMode);
    }

}
