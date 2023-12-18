using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct WindowInit
{
    public WindowStyle Style;
    public WindowTransition TransitionIn;
    public WindowTransition TransitionOut;
    public WindowGroup Group;
    public BackgroundStyle Background;
    public static WindowInit Default => new WindowInit()
    {
        Style = WindowStyle.SingleCornerOnly,
        TransitionIn = WindowTransition.Glitch,
        TransitionOut = WindowTransition.Glitch,
        Group = WindowGroup.MiddleCenter,
        Background = BackgroundStyle.None
    };
}
public class GeneralUISystem : MonoBehaviour
{
    [SerializeField] protected List<WindowUI> instanceWindows = new List<WindowUI>();
    protected Dictionary<string, TextUI> texts = new Dictionary<string, TextUI>();
    protected Dictionary<string, string> delayedContent = new Dictionary<string, string>();
    [SerializeField] protected WindowInit init = WindowInit.Default;
    protected WindowSetup DefaultSetup => new WindowSetup(0, 0, init.Style, init.TransitionIn, init.TransitionOut, background: init.Background);
    [SerializeField] protected bool active = false;
    public bool IsActive => active;
    [SerializeField] protected bool valueDirty = false;
    public bool InDebug = false;
    protected bool initialized = false;
    public virtual void StartUp()
    {
        if (initialized)
            return;
        InitializeUI();
        initialized = true;
        DelayedContentSetting();
    }
    
    public virtual void ToggleDisplay()
    {
        active = !active;
        foreach (WindowUI window in instanceWindows) { window.SetActive(active); }
    }
    public void UpdateDisplayContent()
    {
        foreach (WindowUI window in instanceWindows)
        {
            window.InvokeUpdate();
        }
    }
    protected virtual void ValueUpdate() => _ = 0;
    public void TriggerValueUpdate()
    {
        valueDirty = true;
    }
    protected void MarkValueDirty() => valueDirty = true;
    protected void MarkValueDirty(int _) => valueDirty = true;
    public virtual bool DisplayUI(bool active)
    {
        this.active = active;
        valueDirty = true;
        foreach (WindowUI window in instanceWindows) { window.SetActive(active); }
        return active;
    }
    public void SetOpacity(float opacity, bool animated)
    {
        foreach (WindowUI window in instanceWindows)
        {
            window.SetOpacity(opacity, animated);
        }
    }
    /// <summary>
    /// A simple method to spawn window
    /// </summary>
    protected WindowUI NewWindow(string name, WindowSetup setup)
    {
        WindowUI window = UIManager.Instance.NewWindow(name, init.Group, setup, this);
        instanceWindows.Add(window);
        return window;
    }
    /// <summary>
    /// A simple method to spawn window with designated layout
    /// </summary>
    protected WindowUI NewWindow(string name, Transform layout, WindowSetup setup)
    {
        WindowUI window = UIManager.Instance.NewWindow(name, layout, setup, this);
        instanceWindows.Add(window);
        return window;
    }
    /// <summary>
    /// A simple method to spawn single text UI element window
    /// </summary>
    protected TextUI AddText(string name, int length = 0, float font = 30f)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, setup);
        TextUI text;
        if (length == 0)
            text = window.AddText(name);
        else
            text = window.AddText(TextUtility.PlaceHolder(length));
        texts.Add(name, text);
        window.AutoResize();
        return text;
    }
    /// <summary>
    /// A simple method to spawn text UI to pre-initialized Window
    /// </summary>
    protected TextUI AddGap(WindowUI window)
    {
        TextUI text = AddText("Blank" + texts.Count, window);
        text.SetContent("　");
        return text;
    }
    protected TextUI AddText(string name, WindowUI window, int length = 0)
    {
        TextUI text;
        if (length == 0)
            text = window.AddText(name);
        else
            text = window.AddText(TextUtility.PlaceHolder(length));
        texts.Add(name, text);
        return text;
    }
    /// <summary>
    /// A simple method to spawn single text UI element window on pre-initialized Layout
    /// </summary>
    protected TextUI AddText(string name, Transform layout, int length = 0, float font = 30f)
    {
        WindowSetup setup = DefaultSetup;
        setup.FontSize = font;
        WindowUI window = NewWindow(name, layout, setup);
        TextUI text;
        if (length == 0)
            text = window.AddText(name);
        else
            text = window.AddText(TextUtility.PlaceHolder(length));
        texts.Add(name, text);
        window.AutoResize();
        return text;
    }
    protected virtual void InitializeUI() => _ = 0;
    protected virtual void RemoveElement(WindowElement element, string name = "")
    {
        element.Remove();
        if (element is TextUI)
        {
            if (name != "")
                texts.Remove(name);
#if UNITY_EDITOR
            else
                Debug.LogWarning("TextUI name is empty, might lead to unexpected behavior");
#endif
        }
    }
    /// <summary>
    /// this method is called to reassign unassigned content set before initializing
    /// </summary>
    protected virtual void DelayedContentSetting()
    {
        if (delayedContent.Count == 0)
            return;
        Dictionary<string, string> contentToSet = new Dictionary<string, string>(delayedContent);
        delayedContent = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> kvp in contentToSet)
        {
            SafeSetContent(kvp.Key, kvp.Value, false);
        }
    }
    /// <summary>
    /// this method is used to prevent setting Text UI subscript before initialization
    /// </summary>
    /// <param name="triggerVFX">To trigger the content update vfx or not</param>
    protected void SafeSetSubscript(string key, string value, bool triggerVFX = false)
    {
        if (texts.ContainsKey(key))
        {
            texts[key].SetParentSubscript(value);
            if (triggerVFX)
                texts[key].TriggerGlitch();
        }
        // else
        //     delayedContent[key] = value;
    }
    /// <summary>
    /// this method is used to prevent setting Text UI before initialization
    /// </summary>
    /// <param name="triggerVFX">To trigger the content update vfx or not</param>
    protected void SafeSetContent(string key, string value, bool triggerVFX = false)
    {
        if (texts.ContainsKey(key))
        {
            texts[key].SetContent(value);
            if (triggerVFX)
                texts[key].TriggerGlitch();
        }
        else
        {
            if (delayedContent.ContainsKey(key))
                delayedContent[key] = value;
            else
                delayedContent.Add(key, value);
        }
    }
    /// <summary>
    /// this method is used to prevent accessing uninitialized Text UI
    /// </summary>
    protected string SafeGetContent(string key)
    {
        if (texts.ContainsKey(key))
            return texts[key].FormattedContent;
        else
            return null;
    }
    protected virtual void ClearWindows(bool removeWindow = false)
    {
        initialized = false;
        foreach (WindowUI window in instanceWindows)
        {
            ClearWindow(window, false);
            if (removeWindow)
                UIManager.Instance.CloseWindow(window);
        }
        delayedContent.Clear();
        instanceWindows.Clear();
    }
    protected void ClearWindow(WindowUI window, bool removeWindow = false)
    {
        List<string> toUnlink = new List<string>();
        foreach (KeyValuePair<string, TextUI> pair in texts)
        {
            if (window.Elements.Contains(pair.Value))
                toUnlink.Add(pair.Key);
        }
        foreach (string key in toUnlink)
        {
            texts.Remove(key);
        }
        window.ClearElements();
        if (removeWindow)
        {
            instanceWindows.Remove(window);
            UIManager.Instance.CloseWindow(window);
        }
    }
}
