using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Cysharp.Text;
using Unity.Collections;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class WindowManager : MonoBehaviour, IMenuInputTarget
    {
        static WindowManager _instance;
        public static WindowManager instance => _instance ??= FindAnyObjectByType<WindowManager>();
        [SerializeField] WindowUI _windowTemplate;
        [SerializeField] Canvas _layerTemplate;
        [SerializeField] GameObject _layoutTemplate;
        [SerializeField] RawImage _backgroundTemplate;
        [SerializeField] TextInputProvider _textInputProvider;
        [SerializeField] SelectionProvider _selectionProvider;
        [SerializeField] ConfirmationProvider _confirmationProvider;
        [SerializeField] Material _blurMaterial;
        [SerializeField] Material _transparentMaterial;
        [SerializeField] Camera _interfaceCamera;
        [SerializeField] [ReadOnly] List<WindowUI> _windows = new();
        [SerializeField] [ReadOnly] List<CompositeMenuMono> _compositeMenuMonos = new();
        [SerializeField] [ReadOnly] List<CompositeMenu> _compositeMenus = new();
        [SerializeField] [ReadOnly] List<SimpleMenu> _simpleMenus = new();
        [SerializeField] [ReadOnly] Vector2 _lastMousePosition = Vector2.negativeInfinity;
        [SerializeField] [ReadOnly] RenderMode _overlayMode = RenderMode.ScreenSpaceOverlay;
        Dictionary<WindowLayer, Canvas> _layers = new();
        Dictionary<WindowLayer, Canvas> _backgroundLayers = new();
        IMenuInputTarget _activeMenuTarget;
        IInputProvider _inputProvider;
        Vector2Int _lastResolution = Vector2Int.zero;
        public IInputProvider inputProvider => _inputProvider;

        public void LinkInputTarget(IMenuInputTarget menuInputTarget)
        {
            _activeMenuTarget = menuInputTarget;
        }

        public void UnlinkInput(IMenuInputTarget target)
        {
            if (_activeMenuTarget == target)
                LinkInputTarget(null);
        }

        public RenderMode overlayMode => _overlayMode;

        public void GetTextInput(ITextInputTarget sourceUI, TextInputUI text)
        {
            _textInputProvider.GetTextInput(sourceUI, text);
        }

        public void GetSingleSelectionInput(ISelectionInputTarget sourceUI, List<string> choices,
            int currentChoice)
        {
            _selectionProvider.GetSingleSelection(sourceUI, choices, currentChoice);
        }

        void TriggerResolutionChange()
        {
            foreach (CompositeMenuMono menu in _compositeMenuMonos)
            {
                menu.TriggerResolutionChange();
            }

            foreach (CompositeMenu menu in _compositeMenus)
            {
                menu.TriggerResolutionChange();
            }

            foreach (SimpleMenu menu in _simpleMenus)
            {
                menu.TriggerResolutionChange();
            }

            ClearAllWindowLocation();
        }

        void Awake()
        {
            _instance ??= this;
            _inputProvider = new UnityInputProvider();
            _inputProvider.SetTarget(this);
            _inputProvider.EnableInput(true);
            SetOverlayMode(_overlayMode);
        }

        void Update()
        {
            _inputProvider.Update();
            if (_lastResolution.x != Screen.width || _lastResolution.y != Screen.height)
            {
                _lastResolution = new Vector2Int(Screen.width, Screen.height);
                TriggerResolutionChange();
            }

            foreach (CompositeMenuMono system in _compositeMenuMonos)
            {
                if (!system.enabled)
                    continue;
                system.UpdateMenu();
            }

            foreach (CompositeMenu system in _compositeMenus)
            {
                system.UpdateMenu();
            }

            UpdateSimpleMenuMouseFocus();
            foreach (SimpleMenu system in _simpleMenus)
            {
                system.UpdateMenu();
            }

            foreach (WindowUI window in _windows)
            {
                if (!window.isActive)
                    continue;
                window.ContextUpdate();
            }
        }

        void LateUpdate()
        {
            foreach (WindowUI window in _windows)
            {
                if (!window.isActive)
                    continue;
                window.ContextLateUpdate();
            }
        }

        void UpdateSimpleMenuMouseFocus()
        {
            if (!_inputProvider.hasMouse)
                return;
            if (_lastMousePosition == _inputProvider.mousePosition)
                return;
            _lastMousePosition = _inputProvider.mousePosition;
            if (_simpleMenus.Count == 0 || _simpleMenus.Any(x => x.focused && x.IsMouseInWindow(_lastMousePosition)))
                return;
            foreach (SimpleMenu menu in _simpleMenus)
            {
                if (!menu.isDisplayActive || !menu.isNavigationActive)
                    continue;
                menu.SetFocused(menu.IsMouseInWindow(_inputProvider.mousePosition));
            }
        }

        public void SetOverlayMode(RenderMode mode)
        {
            _overlayMode = mode;
            foreach (Canvas canvas in _layers.Values)
            {
                canvas.renderMode = mode;
            }

            // foreach (RawImage background in blurBackgrounds)
            // {
            //     background.material = overlayMode ? transparentMaterial : blurMaterial;
            // }
            ClearAllWindowLocation();
        }

        public Vector2 UIBoundRetriever(Transform windowTransform, Vector3 elementPosition)
        {
            switch (_overlayMode)
            {
                case RenderMode.ScreenSpaceCamera:
                    return RectTransformUtility.WorldToScreenPoint(_interfaceCamera,
                        windowTransform.TransformPoint(elementPosition));
                case RenderMode.ScreenSpaceOverlay:
                    Vector3 result = windowTransform.TransformPoint(elementPosition);
                    return new Vector2(result.x, result.y);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void WindowRefresh()
        {
            foreach (CompositeMenuMono ui in _compositeMenuMonos)
            {
                ui.ContextLanguageChange();
                ui.RefreshWindows();
            }

            foreach (CompositeMenu menu in _compositeMenus)
            {
                menu.ContextLanguageChange();
                menu.Refresh();
            }

            foreach (SimpleMenu menu in _simpleMenus)
            {
                menu.ContextLanguageChange();
                menu.Refresh();
            }
        }

        public Canvas InstantiateBackgroundLayer(WindowLayer layer)
        {
            if (!_backgroundLayers.TryGetValue(layer, out Canvas existingLayer))
            {
                Canvas newLayer = Instantiate(_layerTemplate, transform);
                newLayer.sortingOrder = (int)layer * 2;
                newLayer.name = ZString.Concat(layer, " BG");
                newLayer.worldCamera = _interfaceCamera;
                newLayer.gameObject.SetActive(true);
                _backgroundLayers.Add(layer, newLayer);
                return newLayer;
            }

            return existingLayer;
        }

        public Canvas InstantiateLayer(WindowLayer layer)
        {
            if (!_layers.TryGetValue(layer, out Canvas existingLayer))
            {
                Canvas newLayer = Instantiate(_layerTemplate, transform);
                newLayer.sortingOrder = (int)layer * 2 + 1;
                newLayer.name = layer.ToString();
                newLayer.worldCamera = _interfaceCamera;
                newLayer.gameObject.SetActive(true);
                _layers.Add(layer, newLayer);
                return newLayer;
            }

            return existingLayer;
        }

        public LayoutAlignment InstantiateLayout(LayoutSetup layoutSetup, string layoutName = "")
        {
            Transform targetLayer = InstantiateLayer(layoutSetup.windowLayer).transform;
            GameObject newLayout = Instantiate(_layoutTemplate, targetLayer);
            LayoutAlignment layoutAlignment = newLayout.AddComponent<LayoutAlignment>();
            HorizontalOrVerticalLayoutGroup layoutGroup = layoutSetup.windowDirection switch
            {
                WindowDirection.Vertical => newLayout.AddComponent<VerticalLayoutGroup>(),
                WindowDirection.Horizontal => newLayout.AddComponent<HorizontalLayoutGroup>(),
                _ => throw new System.NotImplementedException(),
            };
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childAlignment = layoutSetup.windowAlignment switch
            {
                WindowAlignment.UpperLeft => TextAnchor.UpperLeft,
                WindowAlignment.UpperCenter => TextAnchor.UpperCenter,
                WindowAlignment.UpperRight => TextAnchor.UpperRight,
                WindowAlignment.MiddleLeft => TextAnchor.MiddleLeft,
                WindowAlignment.MiddleCenter => TextAnchor.MiddleCenter,
                WindowAlignment.MiddleRight => TextAnchor.MiddleRight,
                WindowAlignment.LowerLeft => TextAnchor.LowerLeft,
                WindowAlignment.LowerCenter => TextAnchor.LowerCenter,
                WindowAlignment.LowerRight => TextAnchor.LowerRight,
                _ => throw new System.NotImplementedException(),
            };
            layoutAlignment.Initialize(layoutGroup, layoutSetup);
            if (layoutName != string.Empty)
                newLayout.name = layoutName;
            newLayout.SetActive(true);
            newLayout.transform.localScale = Vector3.one;
            return layoutAlignment;
        }

        internal void CloseWindow(WindowUI window)
        {
            window.Close();
        }

        public void DelistWindow(WindowUI window)
        {
            _windows.Remove(window);
            Destroy(window.gameObject);
        }

        public WindowUI NewWindow(string windowName, LayoutSetup layoutSetup, WindowSetup setup,
            string menuName)
        {
            WindowUI window = InstantiateWindow(windowName, layoutSetup);
            window.Initialize(windowName, menuName, setup);
            _windows.Add(window);
            window.gameObject.SetActive(false);
            return window;
        }

        public WindowUI NewWindow(string windowName, LayoutAlignment layout, WindowSetup setup,
            string menuName)
        {
            WindowUI window = InstantiateWindow(windowName, layout);
            window.Initialize(windowName, menuName, setup);
            _windows.Add(window);
            window.gameObject.SetActive(false);
            return window;
        }


        WindowUI InstantiateWindow(string windowName, LayoutSetup layoutSetup)
        {
            LayoutAlignment layout = InstantiateLayout(layoutSetup);
            WindowUI window = Instantiate(_windowTemplate, layout.transform);
            layout.RegisterWindow(window);
            layout.gameObject.name = windowName;
            window.gameObject.name = windowName;
            window.transform.localScale = Vector3.one;
            window.RegisterLayout(layout);
            return window;
        }

        WindowUI InstantiateWindow(string windowName, LayoutAlignment layout)
        {
            WindowUI window = Instantiate(_windowTemplate, layout.transform);
            layout.RegisterWindow(window);
            window.gameObject.name = windowName;
            window.transform.localScale = Vector3.one;
            window.RegisterLayout(layout);
            return window;
        }

        public void RegisterMenu(CompositeMenuMono menu)
        {
            if (_compositeMenuMonos.Contains(menu))
                return;
            _compositeMenuMonos.Add(menu);
        }

        public void RegisterMenu(CompositeMenu menu)
        {
            if (_compositeMenus.Contains(menu))
                return;
            _compositeMenus.Add(menu);
        }

        public void RegisterMenu(SimpleMenu menu)
        {
            if (_simpleMenus.Contains(menu))
                return;
            _simpleMenus.Add(menu);
        }

        public void ClearAllWindowLocation()
        {
            float inputDelayDuration = 1.5f;
            foreach (CompositeMenuMono menu in _compositeMenuMonos)
            {
                menu.ClearWindowLocation(inputDelayDuration);
            }

            foreach (CompositeMenu menu in _compositeMenus)
            {
                menu.ClearWindowLocation(inputDelayDuration);
            }

            foreach (SimpleMenu menu in _simpleMenus)
            {
                menu.ClearWindowLocation(inputDelayDuration);
            }
        }

        public void GetConfirm(string title, string message, string confirm,
            string cancel = null, Action onConfirm = null, Action onCancel = null,
            ConfirmDefaultChoice defaultChoice = ConfirmDefaultChoice.None)
        {
            _confirmationProvider.GetConfirm(title, message, confirm, cancel, onConfirm, onCancel,
                defaultChoice);
        }

        public void GetConfirm(string title, string message, string confirm,
            Action onConfirm = null, ConfirmDefaultChoice defaultChoice = ConfirmDefaultChoice.None)
        {
            _confirmationProvider.GetConfirm(title, message, confirm, null, onConfirm, null, defaultChoice);
        }

        public bool CheckClosestDirectionalMatch(SimpleMenu source, Vector2 currentPosition, Vector2 inputDirection)
        {
            float minDistance = Mathf.Infinity;
            SimpleMenu nearestMenu = source;
            int nearestInteractableIndex = -1;
            foreach (SimpleMenu menu in _simpleMenus)
            {
                if (menu == source || menu.instanceWindow.interactables.Count == 0)
                    continue;
                for (int i = 0; i < menu.instanceWindow.interactables.Count; i++)
                {
                    (Vector2 position1, _) = menu.instanceWindow.interactables[i].cachedPosition;
                    Vector2 selectableLocation = position1;
                    Vector2 direction = selectableLocation - currentPosition;
                    float distance = direction.sqrMagnitude;
                    Vector2 directionNormalized = direction.normalized;
                    if (distance < minDistance && Vector2.Dot(directionNormalized, inputDirection) > .5f)
                    {
                        minDistance = distance;
                        nearestMenu = menu;
                        nearestInteractableIndex = i;
                    }
                }
            }

            if (nearestMenu != source)
            {
                source.SetFocused(false);
                nearestMenu.SetFocused(true);
                nearestMenu.SetCurrentSelection(nearestInteractableIndex);
                return true;
            }

            return false;
        }

        void IMenuInputTarget.OnConfirm()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnConfirm();
        }

        void IMenuInputTarget.OnCancel()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnCancel();
        }

        void IMenuInputTarget.OnMove(Vector2 move)
        {
            if (_activeMenuTarget != null)
            {
                _activeMenuTarget.OnMove(move);
                return;
            }

            if (_simpleMenus.Count == 0 || Mathf.Approximately(0, move.sqrMagnitude))
                return;
            _simpleMenus[0].SetFocused(true);
            _simpleMenus[0].SetCurrentSelection(0);
        }

        void IMenuInputTarget.OnScroll(Vector2 scroll)
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnScroll(scroll);
        }

        void IMenuInputTarget.OnMouseConfirm()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnMouseConfirm();
        }

        void IMenuInputTarget.OnMouseCancel()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnMouseCancel();
        }

        void IMenuInputTarget.OnKeyboardEscape()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnKeyboardEscape();
        }

        public T GetMenu<T>()
        {
            foreach (CompositeMenuMono system in _compositeMenuMonos)
            {
                if (system is T t)
                {
                    return t;
                }
            }

            throw new Exception($"Menu {typeof(T)} not found");
        }
    }
}