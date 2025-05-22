
using ChosenConcept.APFramework.Interface;
using ChosenConcept.APFramework.UI.Menu;
using UnityEngine;

namespace ChosenConcept.APFramework.UI.Layout
{
    [System.Serializable]
    public struct LayoutSetup
    {
        [SerializeField] MenuLayer _menuLayer;
        [SerializeField] WindowAlignment _windowAlignment;
        [SerializeField] WindowDirection _windowDirection;
        [SerializeField] OffsetSource _offsetSource;
        [SerializeField] OffsetType _offsetType;
        [SerializeField] Vector4 _offset;
        [SerializeField] int _spacing;
        public MenuLayer MenuLayer => _menuLayer;
        public WindowAlignment windowAlignment => _windowAlignment;
        public WindowDirection windowDirection => _windowDirection;
        public OffsetSource offsetSource => _offsetSource;
        public OffsetType offsetType => _offsetType;
        public Vector4 offset => _offset;
        public int spacing => _spacing;
        public static LayoutSetup defaultLayout => new()
        {
            _menuLayer = MenuLayer.LayerOne,
            _windowAlignment = WindowAlignment.MiddleCenter,
            _windowDirection = WindowDirection.Horizontal,
            _offsetSource = OffsetSource.Fullscreen,
            _offsetType = OffsetType.Pixel,
            _offset = Vector4.zero,
            _spacing = -10
        };
        public LayoutSetup SetLayer(MenuLayer layer)
        {
            _menuLayer = layer;
            return this;
        }

        public LayoutSetup SetWindowAlignment(WindowAlignment alignment)
        {
            _windowAlignment = alignment;
            return this;
        }

        public LayoutSetup SetWindowDirection(WindowDirection direction)
        {
            _windowDirection = direction;
            return this;
        }

        public LayoutSetup SetOffsetType(OffsetType offsetType)
        {
            _offsetType = offsetType;
            return this;
        }

        public LayoutSetup SetOffset(Vector4 offset)
        {
            _offset = offset;
            return this;
        }

        public LayoutSetup SetSpacing(int spacing)
        {
            _spacing = spacing;
            return this;
        }

        public LayoutSetup SetOffsetSource(OffsetSource offsetSource)
        {
            _offsetSource = offsetSource;
            return this;
        }
    }
}