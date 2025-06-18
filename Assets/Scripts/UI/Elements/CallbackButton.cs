using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Elements
{
    public class CallbackButton : Button
    {
        private UnityAction _hoverCallBack;
        private UnityAction _selectCallBack;
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            Select();
            _hoverCallBack?.Invoke();
        }

        public void SetText(string text)
        {
            TextMeshProUGUI textComponent = GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = text;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            _selectCallBack?.Invoke();
        }

        public void SetHoverCallBack(UnityAction callback)
        {
            _hoverCallBack = callback;
        }
    
        public void SetSelectCallBack(UnityAction callback)
        {
            _selectCallBack = callback;
        }
    
    }
}
