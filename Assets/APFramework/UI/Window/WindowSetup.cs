using System;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface.Framework
{
    [Serializable]
    public struct WindowSetup
    {
        [SerializeField] int _width;
        [SerializeField] int _height;
        [SerializeField] WindowTitleStyle _titleStyle;
        [SerializeField] WindowOutlineStyle _outlineStyle;
        [SerializeField] WindowOutlineDisplayStyle _outlineDisplayStyle;
        [SerializeField] WindowThickenStyle _thickenStyle;
        [SerializeField] WindowLabelStyle _labelStyle;
        [SerializeField] WindowTransition _transitionIn;
        [SerializeField] WindowTransition _transitionOut;
        [SerializeField] float _fontSize;
        [SerializeField] Color _backgroundColor;
        [SerializeField] float _functionStringUpdateInterval;
        public float functionStringUpdateInterval => _functionStringUpdateInterval; 
        public int width => _width;
        public int height => _height;
        public WindowTitleStyle titleStyle => _titleStyle;
        public WindowOutlineStyle outlineStyle => _outlineStyle;
        public WindowOutlineDisplayStyle outlineDisplayStyle => _outlineDisplayStyle;
        public WindowThickenStyle thickenStyle => _thickenStyle;
        public WindowLabelStyle labelStyle => _labelStyle;
        public WindowTransition transitionIn => _transitionIn;
        public WindowTransition transitionOut => _transitionOut;
        public float fontSize => _fontSize;
        public Color backgroundColor => _backgroundColor;

        public static WindowSetup defaultSetup => new()
        {
            _width = 0,
            _height = 0,
            _titleStyle = WindowTitleStyle.None,
            _outlineStyle = WindowOutlineStyle.CornerOnly,
            _outlineDisplayStyle = WindowOutlineDisplayStyle.WhenSelected,
            _thickenStyle = WindowThickenStyle.None,
            _labelStyle = WindowLabelStyle.None,
            _transitionIn = WindowTransition.Glitch,
            _transitionOut = WindowTransition.Glitch,
            _fontSize = 30,
            _backgroundColor = Color.clear,
            _functionStringUpdateInterval = 0,
        };
        
        public WindowSetup(WindowSetup setup)
        {
            _width = setup._width;
            _height = setup._height;
            _titleStyle = setup._titleStyle;
            _outlineStyle = setup._outlineStyle;
            _outlineDisplayStyle = setup._outlineDisplayStyle;
            _thickenStyle = setup._thickenStyle;
            _labelStyle = setup._labelStyle;
            _transitionIn = setup._transitionIn;
            _transitionOut = setup._transitionOut;
            _fontSize = setup._fontSize;
            _backgroundColor = setup._backgroundColor;
            _functionStringUpdateInterval = setup.functionStringUpdateInterval;
        }

        public void SetSize(int w, int h = 0)
        {
            _width = w;
            _height = h;
        }

        public void SetWidth(int targetWidth)
        {
            _width = targetWidth;
        }

        public void SetHeight(int targetHeight)
        {
            _height = targetHeight;
        }

        public void SetTitleStyle(WindowTitleStyle style)
        {
            _titleStyle = style;
        }

        public void SetFontSize(float font)
        {
            _fontSize = font;
        }

        public void SetOutlineDisplayStyle(WindowOutlineDisplayStyle style)
        {
            _outlineDisplayStyle = style;
        }
    }
}