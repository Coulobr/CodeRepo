using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine;

namespace Grubbit
{
    public class SmartInputField : TMP_InputField
    {
        public int CaretPosition => m_CaretPosition;
        public int CaretSelectionPosition => m_CaretSelectPosition;
        public Action OnBeginEditing;
        public Action OnEndEditing;
        public Action<string> OnTextChanged;

        protected override void Start()
        {
            base.Start();

            onSubmit.AddListener((givenString) =>
            {
                OnEndEditing?.Invoke();
                OnTextChanged?.Invoke(givenString);
            });
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            DoStateTransition(SelectionState.Normal, true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            // Leave this blank so it doesn't automatically deselect
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            DoStateTransition(SelectionState.Normal, true);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnBeginEditing?.Invoke();
        }
    }
}