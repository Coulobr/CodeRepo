using System;
using TMPro;
using DG.Tweening;

using UnityEngine;

namespace Grubbit
{
	[Serializable]
	public class RectTransformPositionData
	{
		public Vector2 anchorMin;
		public Vector2 anchorMax;
		public Vector2 anchorPos;
		public Vector2 pivot;
		public Vector3 localScale;
		public Vector2 offsetMin;
		public Vector2 offsetMax;

		public RectTransformPositionData(Vector2 desiredAnchorPos, Vector2 desiredAnchorMin, Vector2 desiredAnchorMax, Vector2 desiredPivot, Vector3 desiredLocalScale)
		{
			anchorPos = desiredAnchorPos;
			anchorMin = desiredAnchorMin;
			anchorMax = desiredAnchorMax;
			pivot = desiredPivot;
			localScale = desiredLocalScale;
		}

		public RectTransformPositionData(Vector2 desiredAnchorPos, Vector2 desiredAnchorMin, Vector2 desiredAnchorMax, Vector2 desiredPivot, Vector3 desiredLocalScale, Vector2 desiredOffsetMin, Vector2 desiredOffsetMax)
		{
			anchorPos = desiredAnchorPos;
			anchorMin = desiredAnchorMin;
			anchorMax = desiredAnchorMax;
			pivot = desiredPivot;
			localScale = desiredLocalScale;
			offsetMin = desiredOffsetMin;
			offsetMax = desiredOffsetMax;
		}
	}
}


