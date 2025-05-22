using System;
using System.Collections.Generic;
using System.Linq;
using ChosenConcept.APFramework.Interface.Framework;
using ChosenConcept.APFramework.Interface.Framework.Element;
using UnityEngine;

public class FunctionWindow : CompositeMenuMono
{
    protected override void InitializeUI()
    {
        WindowUI systemWindow = NewWindow("Function Window", WindowSetup.defaultSetup);
        systemWindow.AddText("Time.time")
            .SetContent(() => Time.time.ToString("F2"));
        systemWindow.AddText("Time.frameCount")
            .SetContent(() => Time.frameCount.ToString());
    }
}