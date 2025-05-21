using ChosenConcept.APFramework.Interface.Framework;
using UnityEngine;

namespace ChosenConcept.APFramework.Interface
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
    }
}