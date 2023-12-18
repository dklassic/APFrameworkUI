using UnityEngine;
public class SliderUI : InputUI
{
    public override int GetLength => TextUtility.ActualLength(content + TextUtility.ColumnWithSpace + MaxLength) + 3;
    public override int Count
    {
        get => count; set
        {
            count = (int)Mathf.Clamp(value, min, max);
            parentWindow?.InvokeUpdate();
            TriggerAction();
        }
    }
    public void SetCountNoAction(int count)
    {
        this.count = (int)Mathf.Clamp(count, min, max);
        parentWindow?.InvokeUpdate();
    }
    public virtual int MaxContentLength
    {
        get
        {
            int minCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(min))) + 1;
            int maxCount = Mathf.FloorToInt(Mathf.Log10(Mathf.Abs(max))) + 1;
            if (minCount > maxCount)
            {
                if (min < 0)
                    return minCount + 1;
                else
                    return minCount;
            }
            else
            {
                if (max < 0)
                    return maxCount + 1;
                else
                    return maxCount;
            }
        }
    }
    System.Action<int> action = null;
    public void SetAction(System.Action<int> action) => this.action = action;
    public new void TriggerAction()
    {
        if (action == null)
            return;
        action.Invoke(count);
    }
    public virtual string OptionFillString(string activeOption)
    {
        int totalLengthRequired = MaxContentLength - TextUtility.UnbiasedLength(activeOption);
        return TextUtility.Repeat(' ', totalLengthRequired - totalLengthRequired / 2) + activeOption + TextUtility.Repeat(' ', totalLengthRequired / 2);
    }
    protected int min = 0;
    protected int max = 10;
    public virtual int FirstSliderArrowIndex => FirstCharacterIndex + content.Length + TextUtility.ColumnWithSpace.Length;
    public virtual int LastSliderArrowIndex => LastCharacterIndex;
    (Vector2, Vector2) cachedArrowPosition = (Vector2.zero, Vector2.zero);
    public (Vector2, Vector2) CachedArrowPosition => cachedArrowPosition;
    public void SetCachedArrowPosition((Vector2, Vector2) position) => cachedArrowPosition = position;
    public void ClearCachedArrowPosition() => cachedArrowPosition = (Vector2.zero, Vector2.zero);
    public SliderUI(string name) : base(name)
    {
        SetContent(name);
        ElementType = WindowElementType.Slider;
    }
    public SliderUI(string name, int min, int max) : base(name)
    {
        this.min = min;
        this.max = max;
        SetContent(name);
        ElementType = WindowElementType.Slider;
    }
    public virtual string SliderText()
    {
        if (count == min)
            return " " + StyleUtility.StringColored(OptionFillString(count.ToString()) + "›", StyleUtility.Selected);
        else if (count == max)
            return StyleUtility.StringColored("‹" + OptionFillString(count.ToString()), StyleUtility.Selected) + " ";
        else
            return StyleUtility.StringColored("‹" + OptionFillString(count.ToString()) + "›", StyleUtility.Selected);
    }
    public override string ToDisplay
    {
        get
        {
            if (inFocus && inputMode)
                return content + TextUtility.ColumnWithSpace + SliderText();
            else
                return base.ToDisplay;
        }
    }
    public bool SetInput(bool value)
    {
        if (!available)
            return false;
        inputMode = value;
        parentWindow.InvokeUpdate();
        return true;
    }
    public void SetLimit(int min, int max)
    {
        this.min = min;
        this.max = max;
        Count = Mathf.Clamp(count, min, max);
    }
    int MaxLength => Mathf.Max(min.ToString().Length, max.ToString().Length);

    public override string FormattedContent { get => content + TextUtility.ColumnWithSpace + count; }
}