using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Grubbit
{
	[Serializable]
	public class ProColumnButtonData
	{
		public SmartButton smartButtonObject;
		public Action<ProColumnButtonData> OnButtonPressed;
		public bool toggleOnPress;
		public bool startingState;

		public bool overrideColumnSize;
		public float columnWidth;
		public float columnHeight;

		public bool overrideColumnIndex;
		public int columnIndex;

		public GrubbitUIListItem parent;

		public Sprite icon;
		public Color iconColor = Color.white;
		public Image imageObject;

		public Sprite fillIcon;
		public Color fillIconColor = Color.white;
		public Image fillImageObject;

		public string text;
		public TextMeshProUGUI textObject;
		public Color textColor = Color.white;
	}
}



