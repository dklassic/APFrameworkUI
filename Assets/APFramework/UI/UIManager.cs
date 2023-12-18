using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum WindowGroup
{
    Uncategorized = 0,
    UpperLeft = 1,
    UpperCenter = 2,
    UpperRight = 3,
    MiddleLeft = 4,
    MiddleCenter = 5,
    LowerLeft = 6,
    LowerCenter = 7,
    LowerRight = 8,
    MiddleCenterHorizontal = 9,
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public WindowUI windowInstance;
    List<WindowUI> windows = new List<WindowUI>();
    [SerializeField] UIInput uiInput;
    public static UIInput UIInput => Instance.uiInput;
    public WindowSetting WindowSetting;
    [SerializeField] List<Canvas> uiCanvas = new List<Canvas>();
    [SerializeField] List<Transform> uiLayout = new List<Transform>();
    public bool InMenu { get => inMenu; set => inMenu = value; }
    bool inMenu = false;
    bool overlayMode = false;
    public bool OverlayMode => overlayMode;
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

        foreach (Canvas canvas in uiCanvas)
        {
            canvas.renderMode = overlayMode ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
        }
        defaultUIStarted = true;
    }
    public void SetOverlayMode(bool enable)
    {
        overlayMode = enable;
        foreach (Canvas canvas in uiCanvas)
        {
            canvas.renderMode = enable ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
        }
        ClearAllWindowLocation();
    }
    public Vector2 UIBoundRetriever(Transform windowTransform, Vector3 elementPosition)
    {
        if (!overlayMode)
        {
            return RectTransformUtility.WorldToScreenPoint(Camera.main, windowTransform.TransformPoint(elementPosition));
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
    public WindowUI NewWindow(string name, WindowGroup group, WindowSetup setup, GeneralUISystem masterUI)
    {
        WindowUI window = InstanceWindow(group, name);
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
    public Transform InstantiateLayout(WindowGroup group, string name = "")
    {
        Transform layout = group switch
        {
            WindowGroup.LowerLeft => uiLayout[0],
            WindowGroup.MiddleLeft => uiLayout[1],
            WindowGroup.MiddleCenter => uiLayout[2],
            WindowGroup.LowerCenter => uiLayout[3],
            WindowGroup.UpperLeft => uiLayout[4],
            WindowGroup.LowerRight => uiLayout[5],
            WindowGroup.UpperCenter => uiLayout[6],
            WindowGroup.UpperRight => uiLayout[7],
            WindowGroup.MiddleCenterHorizontal => uiLayout[8],
            _ => throw new System.NotImplementedException(),
        };
        GameObject newLayout = Instantiate(layout.gameObject, layout.parent.parent);
        if (name != string.Empty)
            newLayout.name = name;
        return newLayout.transform;
    }
    WindowUI InstanceWindow(WindowGroup group, string name)
    {
        Transform layout = InstantiateLayout(group);
        WindowUI window = Instantiate(windowInstance, layout.transform);
        layout.gameObject.name = name;
        window.gameObject.name = name;
        // This is to save up performance
        // window.gameObject.SetActive(true);
        return window;
    }
    WindowUI InstanceWindow(Transform layout, string name)
    {
        WindowUI window = Instantiate(windowInstance, layout.transform);
        window.gameObject.name = name;
        // This is to save up performance
        // window.gameObject.SetActive(true);
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
