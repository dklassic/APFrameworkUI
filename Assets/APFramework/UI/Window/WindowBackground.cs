using UnityEngine;
using UnityEngine.UI;

namespace ChosenConcept.APFramework.Interface.Framework
{
    public class WindowBackground : MonoBehaviour
    {
        [SerializeField] RawImage _background;
        [SerializeField] Color _bgColor = Color.clear;
        public RawImage background => _background;

        internal void SetColor(Color color)
        {
            _bgColor = color;
        }
        public void SetColor(Color color, bool active)
        {
            _bgColor = color;
            if (!active)
                return;
            _background.color = _bgColor;
        }
        internal void SetActive(bool v)
        {
            if (_bgColor == Color.clear)
                return;
            _background.color = v ? _bgColor : Color.clear;
        }
    }
}