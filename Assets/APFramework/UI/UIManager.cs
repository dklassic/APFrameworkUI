using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public WindowUI windowInstance;
    List<WindowUI> windows = new List<WindowUI>();
    [SerializeField] UIInput uiInput;
    public static UIInput UIInput => Instance.uiInput;
    public WindowSetting WindowSetting;
    [SerializeField] List<Canvas> canvasList = new List<Canvas>();
    [SerializeField] GameObject canvasTemplate;
    [SerializeField] GameObject layoutTemplate;
    public bool InMenu { get => inMenu; set => inMenu = value; }
    bool inMenu = false;
    [SerializeField] bool overlayMode = false;
    public bool OverlayMode => overlayMode;
    [SerializeField] Camera uiCamera;
    bool defaultUIStarted = false;
    public bool DefaultUIStarted => defaultUIStarted;
    List<GeneralUISystem> uiSystems;
    public void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        uiSystems = new List<GeneralUISystem>(FindObjectsByType<GeneralUISystem>(FindObjectsSortMode.None));
        foreach (GeneralUISystem system in uiSystems)
        {
            if (!system.enabled)
                continue;
            system.StartUp();
        }
        SetOverlayMode(overlayMode);
        defaultUIStarted = true;
    }
    void Update()
    {
        if (!defaultUIStarted)
            return;
        foreach (GeneralUISystem system in uiSystems)
        {
            if (!system.enabled)
                continue;
            system.UpdateSystem();
        }
    }
    public void SetOverlayMode(bool enable)
    {
        overlayMode = enable;
        if (!overlayMode)
        {
            uiCamera ??= Camera.main;
            if (uiCamera == null)
            {
                Debug.LogError("UI camera not assigned and no main camera found!");
                overlayMode = false;
                return;
            }
        }
        foreach (Canvas canvas in canvasList)
        {
            canvas.renderMode = enable ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
            if (!overlayMode && canvas.worldCamera == null)
            {
                canvas.worldCamera = uiCamera;
            }
        }
        ClearAllWindowLocation();
    }
    public Vector2 UIBoundRetriever(Transform windowTransform, Vector3 elementPosition)
    {
        if (!overlayMode)
        {
            return RectTransformUtility.WorldToScreenPoint(uiCamera, windowTransform.TransformPoint(elementPosition));
        }
        else
        {
            Vector3 result = windowTransform.TransformPoint(elementPosition);
            return new Vector2(result.x, result.y);
        }
    }
    public void LanguageUpdate(int foo)
    {
        WindowRefresh();
        ClearAllWindowLocation();
    }
    public void WindowRefresh()
    {
        foreach (var window in windows)
        {
            window.RefreshSize();
            window.InvokeUpdate();
        }
    }
    internal void CloseWindow(WindowUI window)
    {
        window.Close();
    }

    public void DelistWindow(WindowUI window)
    {
        windows.Remove(window);
        Destroy(window.gameObject);
    }
    public WindowUI NewWindow(string name, LayoutPreset preset, WindowSetup setup, GeneralUISystem masterUI)
    {
        WindowUI window = InstanceWindow(preset, name);
        window.Initialize(name, setup, masterUI);
        windows.Add(window);
        return window;
    }
    public WindowUI NewWindow(string name, Transform group, WindowSetup setup, GeneralUISystem masterUI)
    {
        WindowUI window = InstanceWindow(group, name);
        window.Initialize(name, setup, masterUI);
        windows.Add(window);
        return window;
    }
    public void InstantiateCanvas(int sortingOrder = -1)
    {
        Canvas canvas = Instantiate(canvasTemplate, transform).GetComponent<Canvas>();
        canvas.gameObject.SetActive(true);
        canvas.gameObject.name = "Canvas " + canvasList.Count;
        if (sortingOrder != -1)
            canvas.sortingOrder = sortingOrder;
        else
            canvas.sortingOrder = 10 + canvasList.Count; // No real reason for 10, just feel like UI should be on top of everything else
        canvasList.Add(canvas);
    }
    public Transform InstantiateLayout(LayoutPreset preset, string name = "", int canvasIndex = 0)
    {
        LayoutSetting setting = LayoutUtility.GetSettingByPreset(preset);
        return InstantiateLayout(setting, name, canvasIndex);
    }
    public Transform InstantiateLayout(LayoutSetting setting, string name = "", int canvasIndex = 0)
    {
        while (canvasIndex >= canvasList.Count)
        {
            InstantiateCanvas();
        }
        Canvas canvas = canvasList[canvasIndex];
        GameObject newLayout = Instantiate(layoutTemplate, canvas.transform);
        newLayout.SetActive(true);
        if (name != string.Empty)
            newLayout.name = name;
        switch (setting.Direction)
        {
            case LayoutDirection.Horizontal:
                HorizontalLayoutGroup hGroup = newLayout.AddComponent<HorizontalLayoutGroup>();
                hGroup.childAlignment = (TextAnchor)setting.Alignment;
                hGroup.padding = new RectOffset((int)setting.Padding.X, (int)setting.Padding.Y, (int)setting.Padding.Z, (int)setting.Padding.W);
                hGroup.spacing = setting.Spacing;
                hGroup.childForceExpandHeight = false;
                hGroup.childForceExpandWidth = false;
                hGroup.childControlWidth = true;
                hGroup.childControlHeight = true;
                break;
            case LayoutDirection.Vertical:
                VerticalLayoutGroup vGroup = newLayout.AddComponent<VerticalLayoutGroup>();
                vGroup.childAlignment = (TextAnchor)setting.Alignment;
                vGroup.padding = new RectOffset((int)setting.Padding.X, (int)setting.Padding.Y, (int)setting.Padding.Z, (int)setting.Padding.W);
                vGroup.spacing = setting.Spacing;
                vGroup.childForceExpandHeight = false;
                vGroup.childForceExpandWidth = false;
                vGroup.childControlWidth = true;
                vGroup.childControlHeight = true;
                break;
        }
        return newLayout.transform;
    }
    WindowUI InstanceWindow(LayoutPreset preset, string name)
    {
        Transform layout = InstantiateLayout(preset);
        WindowUI window = Instantiate(windowInstance, layout.transform);
        layout.gameObject.name = name;
        window.gameObject.name = name;
        return window;
    }
    WindowUI InstanceWindow(Transform layout, string name)
    {
        WindowUI window = Instantiate(windowInstance, layout.transform);
        window.gameObject.name = name;
        return window;
    }
    public void RegisterUISystem(GeneralUISystem uiSystem)
    {
        if (uiSystems.Contains(uiSystem))
            return;
        uiSystems.Add(uiSystem);
    }
    public void ClearAllWindowLocation()
    {
        foreach (GeneralUISystem ui in uiSystems)
        {
            if (ui is not GeneralUISystemWithNavigation)
                continue;
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
                ((GeneralUISystemWithNavigation)ui).ClearWindowLocation(1.5f);
            else
                ((GeneralUISystemWithNavigation)ui).ClearWindowLocation(.5f);
        }
    }
}
