using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace HitTrax
{
    public class ButtonToggleGroup : MonoBehaviour
    {
        public int startingIndex;

        [Header("Selected References")] public Sprite selectedSprite;
        public Color selectedButtonColor;
        public Color selectedTextColor;

        [Header("Unselected References")] public Sprite unselectedSprite;
        public Color unselectedButtonColor;
        public Color unselectedTextColor;

        [Header("Uninteractable References")] public Sprite uninteractableSprite;
        public Color uninteractableButtonColor;
        public Color uninteractableTextColor;

        protected List<Button> buttons = new List<Button>();

        protected void Start()
        {
            FindButtons();

            if (transform.childCount > startingIndex)
            {
                transform.GetChild(startingIndex).GetComponent<Button>().onClick.Invoke();
            }
            else if (transform.childCount > 0)
            {
                transform.GetChild(0).GetComponent<Button>().onClick.Invoke();
            }
            else
            {
                Debug.LogWarning("There are no buttons, cannot initialize...");
            }
        }

        protected void Update()
        {
            if (buttons.Count < 1)
            {
                FindButtons();
            }
        }

        public void ToggleButtons(Button selectedButton)
        {
            FindButtons();

            for (var i = 0; i < buttons.Count; ++i)
            {
                // First we check to see if the button is not interactable, if that's the case use the uninteractable choices
                // Then we check to see if this button is the selected button or not, then apply the appropriate choice
                buttons[i].image.sprite = (!buttons[i].interactable ? uninteractableSprite : (buttons[i] == selectedButton ? selectedSprite : unselectedSprite));
                buttons[i].image.color = (!buttons[i].interactable ? uninteractableButtonColor : (buttons[i] == selectedButton ? selectedButtonColor : unselectedButtonColor));

                var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();

                if (tmp)
                {
                    tmp.color = (!buttons[i].interactable ? uninteractableTextColor : (buttons[i] == selectedButton ? selectedTextColor : unselectedTextColor));
                }
            }
        }

        protected void FindButtons()
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                if (!transform.GetChild(i).gameObject.activeSelf)
                {
                    continue;
                }

                var child = transform.GetChild(i).GetComponent<Button>();

                if (child != null && !buttons.Contains(child))
                {
                    buttons.Add(child);
                    child.onClick.AddListener(() => { ToggleButtons(child); });
                }
            }
        }
    }
}