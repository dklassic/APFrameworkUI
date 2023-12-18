using UnityEngine;
using TMPro;

public enum WindowTransition
{
    Full,
    Noise,
    FromLeft,
    FromLeftLagged,
    FromRight,
    FromRightLagged,
    Random,
    None,
    Glitch,
    DamageGlitch
}
public class WindowMask : MonoBehaviour
{
    enum FadeType
    {
        FadeIn,
        FadeOut,
        GlitchVFX,
        DamageGlitchVFX,
    }
    [SerializeField] WindowTransition windowTransitionIn = WindowTransition.Full;
    [SerializeField] WindowTransition windowTransitionOut = WindowTransition.FromLeftLagged;
    [SerializeField] FadeType currentFadeType = FadeType.FadeIn;
    public TextMeshProUGUI Mask;
    string maskText = TextUtility.FadeIn;
    float maskAnimationStep = 0.005f;
    int[,] maskIndex;
    string maskString = TextUtility.FadeIn;
    int fillLine = 0;
    [SerializeField] int widthCount = 0;
    [SerializeField] int heightCount = 0;
    [SerializeField] float nextUpdate = Mathf.Infinity;
    [SerializeField] int endStep = 0;
    [SerializeField] int currentStep = -1;
    [SerializeField] bool initialized = false;
    void Awake()
    {
        Mask.color = Color.white * 1.5f;
    }
    void Update()
    {
        if (!initialized || Time.unscaledTime < nextUpdate || currentStep == -1)
            return;
        nextUpdate = Time.unscaledTime + maskAnimationStep;
        UpdateMaskIndex();
        SetMaskIndex();
        currentStep++;
        if (currentStep <= endStep)
            return;
        endStep = 0;
        currentStep = -1;
        Mask.SetText(string.Empty);
        nextUpdate = Mathf.Infinity;
    }
    void UpdateMaskIndex()
    {
        for (int i = 0; i < maskIndex.GetLength(0); i++)
        {
            for (int j = 0; j < maskIndex.GetLength(1); j++)
            {
                switch (CurrentTransition)
                {
                    case WindowTransition.Noise:
                        maskIndex[i, j] = UnityEngine.Random.Range(0, TextUtility.FadeIn.Length - 1);
                        break;
                    case WindowTransition.Glitch:
                        if (UnityEngine.Random.value > 0.5f)
                            maskIndex[i, j] += Mathf.FloorToInt(Time.unscaledDeltaTime / maskAnimationStep);
                        break;
                    case WindowTransition.DamageGlitch:
                        if (UnityEngine.Random.value > 0.5f)
                            maskIndex[i, j] += Mathf.FloorToInt(Time.unscaledDeltaTime / maskAnimationStep);
                        break;
                    case WindowTransition.Random:
                        if (UnityEngine.Random.value > 0.25f)
                            maskIndex[i, j] += Mathf.FloorToInt(UnityEngine.Random.Range(1, TextUtility.FadeIn.Length - 1) * Time.unscaledDeltaTime / maskAnimationStep);
                        break;
                    default:
                        maskIndex[i, j] += Mathf.FloorToInt(Time.unscaledDeltaTime / maskAnimationStep);
                        break;
                }
            }
        }
    }
    WindowTransition CurrentTransition => currentFadeType switch
    {
        FadeType.FadeIn => windowTransitionIn,
        FadeType.FadeOut => windowTransitionOut,
        FadeType.GlitchVFX => WindowTransition.Glitch,
        FadeType.DamageGlitchVFX => WindowTransition.DamageGlitch,
        _ => windowTransitionIn
    };
    void SetMaskIndex()
    {
        TextUtility.StringBuilder.Clear();
        for (int j = 0; j < maskIndex.GetLength(1); j++)
        {

            for (int i = 0; i < maskIndex.GetLength(0); i++)
            {
                if (j < fillLine || j >= maskIndex.GetLength(1) - fillLine)
                    TextUtility.StringBuilder.Append(' ');
                else
                {
                    int targetIndex = maskIndex[i, j];
                    targetIndex = Mathf.Clamp(targetIndex, 0, maskString.Length - 1);
                    TextUtility.StringBuilder.Append(maskString[targetIndex]);
                }
            }
            TextUtility.StringBuilder.Append(TextUtility.LineBreaker);
        }
        maskText = TextUtility.StringBuilder.ToString();
        Mask.SetText(maskText);
    }
    public void Setup(int widthCount, int heightCount, WindowSetup setup)
    {
        this.windowTransitionIn = setup.TransitionIn;
        this.windowTransitionOut = setup.TransitionOut;
        this.widthCount = widthCount;
        this.heightCount = heightCount;
        TextUtility.StringBuilder.Clear();
        for (int i = 0; i < heightCount; i++)
        {
            TextUtility.StringBuilder.Append(LineFill(TextUtility.FadeIn[0], widthCount));
        }
        maskText = TextUtility.StringBuilder.ToString();
        maskIndex = new int[widthCount, heightCount];
        for (int j = 0; j < maskIndex.GetLength(1); j++)
        {
            for (int i = 0; i < maskIndex.GetLength(0); i++)
            {
                maskIndex[i, j] = -1;
            }
        }
        if (!UIManager.Instance.WindowSetting.HasTitlebar(setup.Style))
            fillLine = 1;
        if (!UIManager.Instance.WindowSetting.HasOutline(setup.Style))
            fillLine = 2;
        SetActive(false);
    }
    
    public void FadeIn()
    {
        if (windowTransitionIn == WindowTransition.None)
            return;
        currentFadeType = FadeType.FadeIn;
        maskString = TextUtility.FadeIn;
        SetupTransition(CurrentTransition);
        initialized = true;
    }
    public float FadeOut()
    {
        if (!initialized || windowTransitionOut == WindowTransition.None)
            return 0f;
        currentFadeType = FadeType.FadeOut;
        return SetupTransition(CurrentTransition);
    }
    float SetupTransition(WindowTransition transitionSetup, bool toSyncGameObject = false)
    {
        nextUpdate = Mathf.NegativeInfinity;
        currentStep = 0;
        int counter = 0;
        switch (transitionSetup)
        {
            case WindowTransition.Noise:
                for (int j = 0; j < maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < maskIndex.GetLength(0); i++)
                    {
                        maskIndex[i, j] = UnityEngine.Random.Range(0, TextUtility.FadeIn.Length - 1);
                    }
                }
                endStep = Mathf.CeilToInt(0.02f / maskAnimationStep);
                break;
            case WindowTransition.FromLeft:
                for (int i = 0; i < maskIndex.GetLength(0); i++)
                {
                    for (int j = 0; j < maskIndex.GetLength(1); j++)
                    {
                        maskIndex[i, j] = counter;
                    }
                    counter--;
                }
                endStep = maskIndex.GetLength(0);
                break;
            case WindowTransition.FromLeftLagged:
                maskString = TextUtility.FadeIn;
                for (int i = 0; i < maskIndex.GetLength(0); i++)
                {
                    for (int j = 0; j < maskIndex.GetLength(1); j++)
                    {
                        maskIndex[i, j] = counter - j;
                    }
                    counter--;
                }
                endStep = maskIndex.GetLength(0) + maskIndex.GetLength(1);
                break;
            case WindowTransition.FromRight:
                for (int i = maskIndex.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = 0; j < maskIndex.GetLength(1); j++)
                    {
                        maskIndex[i, j] = counter;
                    }
                    counter--;
                }
                endStep = maskIndex.GetLength(0);
                break;
            case WindowTransition.FromRightLagged:
                maskString = TextUtility.FadeIn;
                for (int i = maskIndex.GetLength(0) - 1; i >= 0; i--)
                {
                    for (int j = 0; j < maskIndex.GetLength(1); j++)
                    {
                        maskIndex[i, j] = counter - j;
                    }
                    counter--;
                }
                endStep = maskIndex.GetLength(0) + maskIndex.GetLength(1);
                break;
            case WindowTransition.Full:
                for (int j = 0; j < maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < maskIndex.GetLength(0); i++)
                    {
                        maskIndex[i, j] = 0;
                    }
                }
                endStep = Mathf.CeilToInt(0.3f / maskAnimationStep);
                break;
            case WindowTransition.Random:
                for (int j = 0; j < maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < maskIndex.GetLength(0); i++)
                    {
                        maskIndex[i, j] = 0;
                    }
                }
                endStep = 10;
                break;
            case WindowTransition.Glitch:
                for (int j = 0; j < maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < maskIndex.GetLength(0); i++)
                    {
                        if (UnityEngine.Random.value > 0.8f)
                            maskIndex[i, j] = UnityEngine.Random.Range(0, TextUtility.FadeIn.Length - 1);
                        else
                            maskIndex[i, j] = TextUtility.FadeIn.Length - 1;
                    }
                }
                endStep = Mathf.CeilToInt(0.05f / maskAnimationStep);
                break;
            case WindowTransition.DamageGlitch:
                for (int j = 0; j < maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < maskIndex.GetLength(0); i++)
                    {
                        if (UnityEngine.Random.value > 0.95f)
                            maskIndex[i, j] = UnityEngine.Random.Range(0, TextUtility.FadeIn.Length - 1);
                    }
                }
                endStep = Mathf.CeilToInt(0.05f / maskAnimationStep);
                break;
        }
        return endStep * maskAnimationStep;
    }
    string LineFill(char pattern, int count) => TextUtility.Repeat(pattern, count) + TextUtility.LineBreaker;
    public void SetActive(bool active)
    {
        if (active)
        {
            Mask.SetText(maskText);
            nextUpdate = Mathf.NegativeInfinity;
        }
        else
        {
            Mask.SetText(string.Empty);
            nextUpdate = Mathf.Infinity;
        }
    }
    public void SetColor(ColorCode code)
    {
        Mask.color = StyleUtility.ColorSetting(code);
    }
    
    public void TriggerGlitch()
    {
        currentFadeType = FadeType.GlitchVFX;
        SetupTransition(CurrentTransition);
    }
    public void TriggerEffect(WindowTransition type)
    {
        SetupTransition(type);
    }
    public void TriggerDamageGlitch()
    {
        currentFadeType = FadeType.DamageGlitchVFX;
        SetupTransition(CurrentTransition);
    }

}