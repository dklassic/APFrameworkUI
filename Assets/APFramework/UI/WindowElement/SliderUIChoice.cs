using System.Collections.Generic;

public class SliderUIChoice : SliderUI
{
    public override int GetLength
    {
        get
        {
            if (choiceList.Count == 0)
                return TextUtility.ActualLength(FormattedMainContent) + 2;
            else
                return TextUtility.ActualLength(FormattedMainContent) + MaxContentLength + 2;
        }
    }
    List<string> choiceList = new List<string>();
    public override int MaxContentLength
    {
        get
        {
            if (choiceList.Count == 0)
            {
                return 0;
            }
            else
            {
                int count = 0;
                foreach (string s in choiceList)
                {
                    int choiceLength = TextUtility.ActualLength(s);
                    if (choiceLength > count)
                    {
                        count = choiceLength;
                    }
                }
                return count;
            }
        }
    }
    public string CurrentChoice => choiceList.Count > 0 ? choiceList[count] : "N/A";
    public SliderUIChoice(string name) : base(name)
    {
        SetContent(name);
        ElementType = WindowElementType.Slider;
        SetLimit(0, 0);
    }
    public SliderUIChoice(string name, List<string> choice) : base(name)
    {
        SetContent(name);
        this.choiceList = choice;
        SetLimit(0, choice.Count - 1);
        ElementType = WindowElementType.Slider;
    }
    public void SetChoice(List<string> choice)
    {
        this.choiceList = choice;
        SetLimit(0, choice.Count - 1);
    }
    public void AddChoice(string choice)
    {
        choiceList.Add(choice);
        max = choiceList.Count - 1;
    }
    public void RemoveChoiceAt(int index)
    {
        choiceList.RemoveAt(index);
        max = choiceList.Count - 1;
    }
    public override string SliderText()
    {
        string optionString = CurrentChoice;
        if (count == min)
            return " " + StyleUtility.StringColored(OptionFillString(optionString) + "›", StyleUtility.Selected);
        else if (count == max)
            return StyleUtility.StringColored("‹" + OptionFillString(optionString), StyleUtility.Selected) + " ";
        else
            return StyleUtility.StringColored("‹" + OptionFillString(optionString) + "›", StyleUtility.Selected);
    }
    public override string FormattedContent => content + TextUtility.ColumnWithSpace + CurrentChoice;
    public override string FormattedMainContent => content + TextUtility.ColumnWithSpace;
}