using System.Collections;
using System.Collections.Generic;

public class ExampleMenu : GeneralUISystemWithNavigation
{
    IEnumerator Start()
    {
        while (!UIManager.Instance.DefaultUIStarted)
            yield return null;
        yield return CoroutineUtility.WaitRealtime(1f);
        OpenMenu();
    }

    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Example Menu", DefaultSetup);
        AddButton("All window elements available", systemWindow, () => OpenSubMenu(0));
        AddButton("Multiple window example", systemWindow, () => OpenSubMenu(1));
        AddButton("中文顯示", systemWindow, () => OpenSubMenu(2));
        AddButton("Setting", systemWindow, () => OpenSubMenu(3));
        AddGap(systemWindow);
        AddButton("Quit", systemWindow, Quit);
        systemWindow.AutoResize();
    }
    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
