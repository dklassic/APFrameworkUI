using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

public class ChineseDisplay : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("中文顯示", WindowSetup.defaultSetup);
        AddText("這個介面系統對中文顯示有一定程度的支援 Chinese along with English and everything in between 來進行自動斷行其實也是做得到的。",
            systemWindow);
        systemWindow.Resize(50);
    }
}