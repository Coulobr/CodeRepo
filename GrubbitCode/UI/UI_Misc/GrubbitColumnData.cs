using System;
using TMPro;
using UnityEngine;

namespace Grubbit
{
	[Serializable]
	public class ProColumnData
	{
		public string text;
		public Color textColor = Color.white;

		public bool overrideColumnSize;
		public float columnWidth;
		public float columnHeight;

		public bool overrideColumnIndex;
		public int columnIndex;

		public GrubbitUIListItem parent;
		public TextMeshProUGUI textObject;
	}
}


