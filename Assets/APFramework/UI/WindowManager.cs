using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ChosenConcept.APFramework.Interface.Framework.Element;
using Cysharp.Text;

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
        [SerializeField] ContextMenuProvider _contextMenuProvider;
        [SerializeField] Camera _interfaceCamera;
        [SerializeField] List<WindowUI> _windows = new();
        [SerializeField] List<CompositeMenuMono> _compositeMenuMonos = new();
        [SerializeField] List<SimpleMenu> _simpleMenus = new();
        [SerializeField] List<LayoutAlignment> _layoutAlignments = new();
        [SerializeField] Vector2 _lastMousePosition = Vector2.negativeInfinity;
        [SerializeField] RenderMode _overlayMode = RenderMode.ScreenSpaceOverlay;
        Dictionary<MenuLayer, Canvas> _layers = new();
        Dictionary<MenuLayer, Canvas> _backgroundLayers = new();
        IMenuInputTarget _activeMenuTarget;
        IInputProvider _inputProvider;
        Vector2Int _lastResolution = Vector2Int.zero;
        public IInputProvider inputProvider => _inputProvider;
        public RenderMode overlayMode => _overlayMode;

        public bool providerActive => _selectionProvider.active || _textInputProvider.active ||
                                      _confirmationProvider.active ||
                                      _contextMenuProvider.active;


        public void EnableGlobalVisibility(bool enable)
        {
            float opacity = enable ? 1f : 0.05f;
            foreach (SimpleMenu simpleMenu in _simpleMenus)
            {
                if (!simpleMenu.isDisplayActive)
                    continue;
                simpleMenu.SetOpacity(opacity);
            }

            foreach (CompositeMenuMono compositeMenuMono in _compositeMenuMonos)
            {
                if (!compositeMenuMono.isDisplayActive)
                    continue;
                compositeMenuMono.SetOpacity(opacity);
            }
        }

        public void LinkInputTarget(IMenuInputTarget menuInputTarget)
        {
            _activeMenuTarget = menuInputTarget;
        }

        public void UnlinkInput(IMenuInputTarget target)
        {
            if (_activeMenuTarget == target)
                LinkInputTarget(null);
        }

        public void GetContextMenu(List<string> choices, List<Action> actions, Vector2 position, Action onClose)
        {
            EnableGlobalVisibility(false);
            _contextMenuProvider.SetupMenu(choices, actions, position, onClose);
        }

        public void EndContextMenu()
        {
            EnableGlobalVisibility(true);
        }

        public void GetTextInput(IMenuInputTarget sourceUI, TextInputUI text)
        {
            _textInputProvider.GetTextInput(sourceUI, text);
        }

        public void GetSelectionInput(IMenuInputTarget sourceUI, List<string> choices,
            int currentChoice)
        {
            EnableGlobalVisibility(false);

            _selectionProvider.GetSelection(sourceUI, choices, currentChoice);
        }

        public void EndSelectionInput()
        {
            EnableGlobalVisibility(true);
        }

        void TriggerResolutionChange()
        {
            foreach (CompositeMenuMono menu in _compositeMenuMonos)
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
            _contextMenuProvider.Initialize();
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

            foreach (WindowUI window in _windows)
            {
                window.UpdateWindow();
            }

            if (providerActive)
            {
                _confirmationProvider.UpdateMenu();
                _contextMenuProvider.UpdateMenu();
                _selectionProvider.UpdateMenu();
                return;
            }

            // When any provider is active, disable interaction of menus
            foreach (CompositeMenuMono system in _compositeMenuMonos)
            {
                if (!system.enabled)
                    continue;
                system.UpdateMenu();
            }

            UpdateSimpleMenuMouseFocus();

            foreach (SimpleMenu system in _simpleMenus)
            {
                system.UpdateMenu();
            }
        }

        void LateUpdate()
        {
            foreach (WindowUI window in _windows)
            {
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
            if (_simpleMenus.Count == 0 ||
                _simpleMenus.Any(x =>
                    x.focused && x.windowInstance.canNavigate && x.IsMouseInWindow(_lastMousePosition) ||
                    x.movingWindow || x.inElementInputMode))
                return;
            foreach (SimpleMenu menu in _simpleMenus)
            {
                if (!menu.isDisplayActive || !menu.isNavigationActive || !menu.windowInstance.canNavigate)
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
                ui.RefreshWindows();
            }

            foreach (SimpleMenu menu in _simpleMenus)
            {
                menu.Refresh();
            }
        }

        public Canvas InstantiateBackgroundLayer(MenuLayer layer)
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

        public Canvas InstantiateLayer(MenuLayer layer)
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
            Transform targetLayer = InstantiateLayer(layoutSetup.MenuLayer).transform;
            GameObject newLayout = Instantiate(_layoutTemplate, targetLayer);
            LayoutAlignment layoutAlignment = newLayout.AddComponent<LayoutAlignment>();
            _layoutAlignments.Add(layoutAlignment);
            HorizontalOrVerticalLayoutGroup layoutGroup = layoutSetup.windowDirection switch
            {
                WindowDirection.Vertical => newLayout.AddComponent<VerticalLayoutGroup>(),
                WindowDirection.Horizontal => newLayout.AddComponent<HorizontalLayoutGroup>(),
                _ => throw new NotImplementedException(),
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

        public void RegisterMenu(SimpleMenu menu)
        {
            if (_simpleMenus.Contains(menu))
                return;
            _simpleMenus.Add(menu);
        }

        public void ClearAllWindowLocation()
        {
            float inputDelayDuration = Screen.fullScreenMode is FullScreenMode.ExclusiveFullScreen ? 1.5f : 0.5f;
            foreach (CompositeMenuMono menu in _compositeMenuMonos)
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
            ConfirmationDefaultChoice defaultChoice = ConfirmationDefaultChoice.None)
        {
            EnableGlobalVisibility(false);

            _confirmationProvider.GetConfirm(title, message, confirm, cancel, onConfirm, onCancel,
                defaultChoice);
        }

        public void GetConfirm(string title, string message, string confirm,
            Action onConfirm = null, ConfirmationDefaultChoice defaultChoice = ConfirmationDefaultChoice.None)
        {
            EnableGlobalVisibility(false);

            _confirmationProvider.GetConfirm(title, message, confirm, null, onConfirm, null, defaultChoice);
        }

        public void EndConfirm()
        {
            EnableGlobalVisibility(true);
        }

        public bool CheckClosestDirectionalMatch(SimpleMenu source, Vector2 currentPosition, Vector2 inputDirection,
            bool allowCycleBetweenWindows)
        {
            float minScore = Mathf.Infinity;
            SimpleMenu nearestMenu = source;
            foreach (SimpleMenu menu in _simpleMenus)
            {
                if (menu == source || !menu.isDisplayActive || !menu.isNavigationActive ||
                    !menu.windowInstance.canNavigate)
                    continue;
                Vector2 windowCenter =
                    (menu.windowInstance.cachedPosition.Item1 + menu.windowInstance.cachedPosition.Item2) / 2f;
                Vector2 direction = windowCenter - currentPosition;
                float distance = direction.sqrMagnitude;
                Vector2 directionNormalized = direction.normalized;
                float dotProduct = Vector2.Dot(inputDirection, directionNormalized);
                if (dotProduct < .3f)
                    continue;
                // Favoring both shorter distance and better directional match
                float score = distance * (2 - dotProduct);
                if (score < minScore)
                {
                    minScore = score;
                    nearestMenu = menu;
                }
            }

            if (nearestMenu != source)
            {
                int nearestInteractableIndex = -1;
                minScore = Mathf.Infinity;
                for (int i = 0; i < nearestMenu.windowInstance.interactables.Count; i++)
                {
                    // Using only the starting position for consistency
                    Vector2 position1 = nearestMenu.windowInstance.interactables[i].cachedPosition.Item1;
                    Vector2 selectableLocation = position1;
                    Vector2 direction = selectableLocation - currentPosition;
                    float distance = direction.sqrMagnitude;
                    if (distance < minScore)
                    {
                        minScore = distance;
                        nearestInteractableIndex = i;
                    }
                }

                source.SetFocused(false);
                nearestMenu.SetFocused(true);
                nearestMenu.SetCurrentSelection(nearestInteractableIndex);
                return true;
            }

            if (allowCycleBetweenWindows)
            {
                // TODO: maybe?
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
            foreach (SimpleMenu menu in _simpleMenus)
            {
                if (!menu.isDisplayActive || !menu.isNavigationActive)
                    continue;
                menu.SetFocused(true);
                menu.SetCurrentSelection(0);
                break;
            }
        }

        void IMenuInputTarget.OnScroll(Vector2 scroll)
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnScroll(scroll);
        }

        void IMenuInputTarget.OnMouseConfirmPressed()
        {
            IEnumerable<SimpleMenu> list = _simpleMenus.Where(x =>
                x.canBeClosedByOutOfFocusClick && x.isDisplayActive && x.isNavigationActive && !x.focused);
            foreach (SimpleMenu menu in list)
            {
                menu.CloseMenu();
            }

            if (_activeMenuTarget != null)
                _activeMenuTarget.OnMouseConfirmPressed();
        }

        void IMenuInputTarget.OnMouseConfirmReleased()
        {
            if (_activeMenuTarget != null)
                _activeMenuTarget.OnMouseConfirmReleased();
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

        void IMenuInputTarget.SetSelection(int i)
        {
        }

        void IMenuInputTarget.SetTextInput(string inputFieldText)
        {
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

        public string ExportLocalizationTag()
        {
            List<string> tags = new List<string>();
            foreach (SimpleMenu simpleMenu in _simpleMenus)
            {
                tags.AddRange(simpleMenu.ExportLocalizationTag());
            }

            foreach (CompositeMenuMono system in _compositeMenuMonos)
            {
                tags.AddRange(system.ExportLocalizationTag());
            }

            return string.Join("\n", tags);
        }
    }
}