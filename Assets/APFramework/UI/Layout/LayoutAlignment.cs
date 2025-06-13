using System;
using System.Collections.Generic;
using ChosenConcept.APFramework.UI.Window;
using UnityEngine;
using UnityEngine.UI;
namespace ChosenConcept.APFramework.UI.Layout
{
    public class LayoutAlignment : MonoBehaviour
    {
        [SerializeField] HorizontalOrVerticalLayoutGroup _layoutGroup;
        [SerializeField] LayoutSetup _layoutSetup;
        [SerializeField] List<WindowUI> _windows = new();
        static Vector2Int _referenceResolution = new(1920, 1080);
        public void Initialize(HorizontalOrVerticalLayoutGroup layoutGroup, LayoutSetup layoutSetup)
        {
            _layoutGroup = layoutGroup;
            _layoutSetup = layoutSetup;
            UpdateLayout();
        }
        public void UpdateLayout()
        {
            if (_layoutSetup.offsetSource == OffsetSource.CenterOfScreen)
            {
                // consider the content size of all windows
                float accumulatedWidth = 0;
                float accumulatedHeight = 0;
                if (_layoutSetup.windowDirection == WindowDirection.Horizontal)
                {
                    foreach (WindowUI window in _windows)
                    {
                        accumulatedWidth += window.layout.minWidth;

                        if (window.layout.minHeight > accumulatedHeight)
                            accumulatedHeight = window.layout.minHeight;
                    }
                }
                else
                {
                    foreach (WindowUI window in _windows)
                    {
                        if (window.layout.minWidth > accumulatedWidth)
                            accumulatedWidth = window.layout.minWidth;

                        accumulatedHeight += window.layout.minHeight;
                    }
                }
                float referenceMultiplier = Screen.height / (float)_referenceResolution.y;
                float ratio = Screen.width / (float)Screen.height;
                // int width = (int)(accumulatedWidth * referenceMultiplier / 2f);
                int width = 0;
                // int height = (int)(accumulatedHeight * referenceMultiplier / 2f);
                int height = 0;

                _layoutGroup.padding.top = _layoutSetup.windowAlignment switch
                {
                    WindowAlignment.UpperLeft or
                        WindowAlignment.UpperCenter or
                        WindowAlignment.UpperRight => _referenceResolution.y / 2 - height,
                    _ => 0
                };
                _layoutGroup.padding.bottom = _layoutSetup.windowAlignment switch
                {
                    WindowAlignment.LowerLeft or
                        WindowAlignment.LowerCenter or
                        WindowAlignment.LowerRight => _referenceResolution.y / 2 - height,
                    _ => 0
                };
                _layoutGroup.padding.left = _layoutSetup.windowAlignment switch
                {
                    WindowAlignment.UpperLeft or
                        WindowAlignment.MiddleLeft or
                        WindowAlignment.LowerLeft => (int)(_referenceResolution.y * ratio / 2 - width),
                    _ => 0
                };
                _layoutGroup.padding.right = _layoutSetup.windowAlignment switch
                {
                    WindowAlignment.UpperRight or
                        WindowAlignment.MiddleRight or
                        WindowAlignment.LowerRight => (int)(_referenceResolution.y * ratio / 2 - width),
                    _ => 0
                };

                Vector2 multiplier = _layoutSetup.offsetType switch
                {
                    OffsetType.Percentage => _referenceResolution / 2,
                    OffsetType.Pixel => Vector2.one * referenceMultiplier,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (_layoutSetup.offset.x > 0)
                    _layoutGroup.padding.top -= (int)(_layoutSetup.offset.x * multiplier.y);
                if (_layoutSetup.offset.y > 0)
                    _layoutGroup.padding.bottom -= (int)(_layoutSetup.offset.y * multiplier.y);
                if (_layoutSetup.offset.z > 0)
                    _layoutGroup.padding.left -= (int)(_layoutSetup.offset.z * multiplier.x);
                if (_layoutSetup.offset.w > 0)
                    _layoutGroup.padding.right -= (int)(_layoutSetup.offset.w * multiplier.x);
            }
            else
            {
                float referenceMultiplier = Screen.height / (float)_referenceResolution.y;
                Vector2 multiplier = _layoutSetup.offsetType switch
                {
                    OffsetType.Percentage => _referenceResolution / 2,
                    OffsetType.Pixel => Vector2.one * referenceMultiplier,
                    _ => throw new ArgumentOutOfRangeException()
                };

                _layoutGroup.padding.top = (int)(_layoutSetup.offset.x * multiplier.y);
                _layoutGroup.padding.bottom = (int)(_layoutSetup.offset.y * multiplier.y);
                _layoutGroup.padding.left = (int)(_layoutSetup.offset.z * multiplier.x);
                _layoutGroup.padding.right = (int)(_layoutSetup.offset.w * multiplier.x);
            }
            _layoutGroup.spacing = _layoutSetup.spacing;
            _layoutGroup.enabled = false;
            _layoutGroup.enabled = true;
        }
        public void RegisterWindow(WindowUI window)
        {
            _windows.Add(window);
        }
        public void UnregisterWindow(WindowUI window)
        {
            _windows.Remove(window);
        }
        public void ContextResolutionChange()
        {
            UpdateLayout();
        }
        public void MoveWindowToIndex(WindowUI window, int index)
        {
            window.transform.SetSiblingIndex(index);
        }
    }
}
