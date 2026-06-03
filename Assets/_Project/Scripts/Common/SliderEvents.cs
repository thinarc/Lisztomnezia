using _Project.Scripts.Common.EntryPoints;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Common
{
    public class SliderEvents : Slider
    {
        private int _index;
        
        protected override void Awake()
        {
            base.Awake();
            if (name.Contains("Bg")) _index = 0;
            else if (name.Contains("Sfx")) _index = 1;
            else if (name.Contains("Voice")) _index = 2;
        }
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            LogoEntryPoint.StartOptions.OnPointDown(_index);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            LogoEntryPoint.StartOptions.OnPointUp(_index);
        }
    }
}