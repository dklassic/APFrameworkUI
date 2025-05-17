using UnityEngine;
using TMPro;
using Cysharp.Text;
using Unity.Collections;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class WindowMask : MonoBehaviour
    {
        enum FadeType
        {
            FadeIn,
            FadeOut,
            GlitchVFX,
            DamageGlitchVFX,
        }

        [SerializeField] [ReadOnly] WindowTransition _windowTransitionIn = WindowTransition.Full;
        [SerializeField] [ReadOnly] WindowTransition _windowTransitionOut = WindowTransition.FromLeftLagged;
        [SerializeField] [ReadOnly] FadeType _currentFadeType = FadeType.FadeIn;
        [SerializeField] TextMeshProUGUI _mask;
        public TextMeshProUGUI mask => _mask;
        string _maskText = TextUtility.FADE_IN;
        float _maskAnimationStep = 0.005f;
        int[,] _maskIndex;
        string _maskString = TextUtility.FADE_IN;
        int _fillLine = 0;
        [SerializeField] [ReadOnly] int _widthCount = 0;
        [SerializeField] [ReadOnly] int _heightCount = 0;
        [SerializeField] [ReadOnly] float _nextUpdate = Mathf.Infinity;
        [SerializeField] [ReadOnly] int _endStep = 0;
        [SerializeField] [ReadOnly] int _currentStep = -1;
        [SerializeField] [ReadOnly] bool _initialized = false;

        public void Initialize()
        {
            mask.color = Color.white * 1.5f;
        }

        public void ContextUpdate()
        {
            if (!_initialized || Time.unscaledTime < _nextUpdate || _currentStep == -1)
                return;
            _nextUpdate = Time.unscaledTime + _maskAnimationStep;
            UpdateMaskIndex();
            SetMaskIndex();
            _currentStep++;
            if (_currentStep <= _endStep)
                return;
            _endStep = 0;
            _currentStep = -1;
            mask.SetText(string.Empty);
            _nextUpdate = Mathf.Infinity;
        }

        void UpdateMaskIndex()
        {
            for (int i = 0; i < _maskIndex.GetLength(0); i++)
            {
                for (int j = 0; j < _maskIndex.GetLength(1); j++)
                {
                    switch (CurrentTransition)
                    {
                        case WindowTransition.Noise:
                            _maskIndex[i, j] = Random.Range(0, TextUtility.FADE_IN.Length - 1);
                            break;
                        case WindowTransition.Glitch:
                            if (Random.value > 0.5f)
                                _maskIndex[i, j] +=
                                    Mathf.FloorToInt(Time.unscaledDeltaTime / _maskAnimationStep);
                            break;
                        case WindowTransition.DamageGlitch:
                            if (Random.value > 0.5f)
                                _maskIndex[i, j] +=
                                    Mathf.FloorToInt(Time.unscaledDeltaTime / _maskAnimationStep);
                            break;
                        case WindowTransition.Random:
                            if (Random.value > 0.25f)
                                _maskIndex[i, j] += Mathf.FloorToInt(Random.Range(1, TextUtility.FADE_IN.Length - 1) *
                                    Time.unscaledDeltaTime / _maskAnimationStep);
                            break;
                        default:
                            _maskIndex[i, j] += Mathf.FloorToInt(Time.unscaledDeltaTime / _maskAnimationStep);
                            break;
                    }
                }
            }
        }

        WindowTransition CurrentTransition => _currentFadeType switch
        {
            FadeType.FadeIn => _windowTransitionIn,
            FadeType.FadeOut => _windowTransitionOut,
            FadeType.GlitchVFX => WindowTransition.Glitch,
            FadeType.DamageGlitchVFX => WindowTransition.DamageGlitch,
            _ => _windowTransitionIn
        };

        void SetMaskIndex()
        {
            using (var windowStringBuilder = ZString.CreateStringBuilder())
            {
                for (int j = 0; j < _maskIndex.GetLength(1); j++)
                {
                    for (int i = 0; i < _maskIndex.GetLength(0); i++)
                    {
                        if (j < _fillLine || j >= _maskIndex.GetLength(1) - _fillLine)
                            windowStringBuilder.Append(' ');
                        else
                        {
                            int targetIndex = _maskIndex[i, j];
                            targetIndex = Mathf.Clamp(targetIndex, 0, _maskString.Length - 1);
                            windowStringBuilder.Append(_maskString[targetIndex]);
                        }
                    }

                    windowStringBuilder.Append(TextUtility.LineBreaker);
                }

                mask.SetText(windowStringBuilder);
            }
        }

        public void Setup(int widthCount, int heightCount, WindowSetup setup)
        {
            _windowTransitionIn = setup.transitionIn;
            _windowTransitionOut = setup.transitionOut;
            _widthCount = widthCount;
            _heightCount = heightCount;
            using (var windowStringBuilder = ZString.CreateStringBuilder())
            {
                for (int i = 0; i < heightCount; i++)
                {
                    windowStringBuilder.Append(LineFill(TextUtility.FADE_IN[0], _widthCount));
                }

                _maskText = windowStringBuilder.ToString();
            }

            _maskIndex = new int[_widthCount, _heightCount];
            for (int j = 0; j < _maskIndex.GetLength(1); j++)
            {
                for (int i = 0; i < _maskIndex.GetLength(0); i++)
                {
                    _maskIndex[i, j] = -1;
                }
            }

            if (setup.titleStyle != WindowTitleStyle.TitleBar)
                _fillLine = 1;
            if (setup.outlineStyle == WindowOutlineStyle.None)
                _fillLine = 2;
            SetActive(false);
        }


        public void FadeIn()
        {
            if (_windowTransitionIn == WindowTransition.None)
                return;
            _currentFadeType = FadeType.FadeIn;
            _maskString = TextUtility.FADE_IN;
            SetupTransition(CurrentTransition);
            _initialized = true;
        }

        public float FadeOut()
        {
            if (!_initialized || _windowTransitionOut == WindowTransition.None)
                return 0f;
            _currentFadeType = FadeType.FadeOut;
            return SetupTransition(CurrentTransition);
        }

        float SetupTransition(WindowTransition transitionSetup, bool toSyncGameObject = false)
        {
            _nextUpdate = Mathf.NegativeInfinity;
            _currentStep = 0;
            int counter = 0;
            switch (transitionSetup)
            {
                case WindowTransition.Noise:
                    for (int j = 0; j < _maskIndex.GetLength(1); j++)
                    {
                        for (int i = 0; i < _maskIndex.GetLength(0); i++)
                        {
                            _maskIndex[i, j] = Random.Range(0, TextUtility.FADE_IN.Length - 1);
                        }
                    }

                    _endStep = Mathf.CeilToInt(0.02f / _maskAnimationStep);
                    break;
                case WindowTransition.FromLeft:
                    for (int i = 0; i < _maskIndex.GetLength(0); i++)
                    {
                        for (int j = 0; j < _maskIndex.GetLength(1); j++)
                        {
                            _maskIndex[i, j] = counter;
                        }

                        counter--;
                    }

                    _endStep = _maskIndex.GetLength(0);
                    break;
                case WindowTransition.FromLeftLagged:
                    _maskString = TextUtility.FADE_IN;
                    for (int i = 0; i < _maskIndex.GetLength(0); i++)
                    {
                        for (int j = 0; j < _maskIndex.GetLength(1); j++)
                        {
                            _maskIndex[i, j] = counter - j;
                        }

                        counter--;
                    }

                    _endStep = _maskIndex.GetLength(0) + _maskIndex.GetLength(1);
                    break;
                case WindowTransition.FromRight:
                    for (int i = _maskIndex.GetLength(0) - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < _maskIndex.GetLength(1); j++)
                        {
                            _maskIndex[i, j] = counter;
                        }

                        counter--;
                    }

                    _endStep = _maskIndex.GetLength(0);
                    break;
                case WindowTransition.FromRightLagged:
                    _maskString = TextUtility.FADE_IN;
                    for (int i = _maskIndex.GetLength(0) - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < _maskIndex.GetLength(1); j++)
                        {
                            _maskIndex[i, j] = counter - j;
                        }

                        counter--;
                    }

                    _endStep = _maskIndex.GetLength(0) + _maskIndex.GetLength(1);
                    break;
                case WindowTransition.Full:
                    for (int j = 0; j < _maskIndex.GetLength(1); j++)
                    {
                        for (int i = 0; i < _maskIndex.GetLength(0); i++)
                        {
                            _maskIndex[i, j] = 0;
                        }
                    }

                    _endStep = Mathf.CeilToInt(0.3f / _maskAnimationStep);
                    break;
                case WindowTransition.Random:
                    for (int j = 0; j < _maskIndex.GetLength(1); j++)
                    {
                        for (int i = 0; i < _maskIndex.GetLength(0); i++)
                        {
                            _maskIndex[i, j] = 0;
                        }
                    }

                    _endStep = 10;
                    break;
                case WindowTransition.Glitch:
                    for (int j = 0; j < _maskIndex.GetLength(1); j++)
                    {
                        for (int i = 0; i < _maskIndex.GetLength(0); i++)
                        {
                            if (Random.value > 0.8f)
                                _maskIndex[i, j] = Random.Range(0, TextUtility.FADE_IN.Length - 1);
                            else
                                _maskIndex[i, j] = TextUtility.FADE_IN.Length - 1;
                        }
                    }

                    _endStep = Mathf.CeilToInt(0.05f / _maskAnimationStep);
                    break;
                case WindowTransition.DamageGlitch:
                    for (int j = 0; j < _maskIndex.GetLength(1); j++)
                    {
                        for (int i = 0; i < _maskIndex.GetLength(0); i++)
                        {
                            if (Random.value > 0.95f)
                                _maskIndex[i, j] = Random.Range(0, TextUtility.FADE_IN.Length - 1);
                        }
                    }

                    _endStep = Mathf.CeilToInt(0.05f / _maskAnimationStep);
                    break;
            }

            return _endStep * _maskAnimationStep;
        }

        string LineFill(char pattern, int count) => TextUtility.Repeat(pattern, count) + TextUtility.LineBreaker;

        public void SetActive(bool active)
        {
            if (active)
            {
                mask.SetText(_maskText);
                _nextUpdate = Mathf.NegativeInfinity;
            }
            else
            {
                mask.SetText(string.Empty);
                _nextUpdate = Mathf.Infinity;
            }
        }

        public void SetColor(ColorCode code)
        {
            mask.color = StyleUtility.ColorSetting(code);
        }


        public void TriggerGlitch()
        {
            _currentFadeType = FadeType.GlitchVFX;
            SetupTransition(CurrentTransition);
        }

        public void TriggerEffect(WindowTransition type)
        {
            SetupTransition(type);
        }

        public void TriggerDamageGlitch()
        {
            _currentFadeType = FadeType.DamageGlitchVFX;
            SetupTransition(CurrentTransition);
        }
    }
}