using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace Grubbit
{
    public class TooltipTemplate : MonoBehaviour
    {
        public TextMeshProUGUI baseText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subtitleText;
        public Image itemIcon;
        public Ease desiredEaseType = Ease.OutCubic;
    }
}