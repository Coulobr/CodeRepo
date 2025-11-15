using System.Linq;
using UnityEngine;

namespace Grubbit
{
	public static class RectTransformExtensions
	{
		/// <summary>
		/// Counts the bounding box corners of the given RectTransform that are visible in screen space.
		/// </summary>
		private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera = null)
		{
			var screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
			var objectCorners = new Vector3[4];
			rectTransform.GetWorldCorners(objectCorners);
			return objectCorners.Select(corner => camera != null ? camera.WorldToScreenPoint(corner) : corner).Count(tempScreenSpaceCorner => screenBounds.Contains(tempScreenSpaceCorner));
		}

		/// <summary>
		/// Counts the bounding box corners of the given RectTransform that are visible in screen space.
		/// </summary>
		private static int CountCornersVisibleFrom(this RectTransform rectTransform, Canvas canvas)
		{
			if (canvas == null)
			{
				Debug.LogWarning("The given canvas is null, cannot check to see if rect transform is in bounds.");
				return 0;
			}

			var pixelRect = canvas.pixelRect;
			var canvasBounds = new Rect(0f, 0f, pixelRect.width, pixelRect.height);
			var objectCorners = new Vector3[4];
			rectTransform.GetWorldCorners(objectCorners);
			return objectCorners.Count(corner => canvasBounds.Contains(corner));
		}

		/// <summary>
		/// Determines if this RectTransform is fully visible.
		/// Works by checking if each bounding box corner of this RectTransform is inside the desired canvas.
		/// </summary>
		public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Canvas canvas)
		{
			return CountCornersVisibleFrom(rectTransform, canvas) == 4;
		}

		/// <summary>
		/// Determines if this RectTransform is at least partially visible.
		/// Works by checking if each bounding box corner of this RectTransform is inside the desired canvas
		/// </summary>
		public static bool IsVisibleFrom(this RectTransform rectTransform, Canvas canvas)
		{
			return CountCornersVisibleFrom(rectTransform, canvas) > 0;
		}

		/// <summary>
		/// Determines if this RectTransform is fully visible.
		/// Works by checking if each bounding box corner of this RectTransform is inside the screen space view frustrum.
		/// </summary>
		public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera = null)
		{
			return CountCornersVisibleFrom(rectTransform, camera) == 4;
		}

		/// <summary>
		/// Determines if this RectTransform is at least partially visible.
		/// Works by checking if any bounding box corner of this RectTransform is inside the screen space view frustrum.
		/// </summary>
		public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera = null)
		{
			return CountCornersVisibleFrom(rectTransform, camera) > 0;
		}

		public static void SetLeft(this RectTransform rectTransform, float left)
		{
			rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
		}

		public static void SetRight(this RectTransform rectTransform, float right)
		{
			rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
		}

		public static void SetWidth(this RectTransform rectTransform, float newWidth)
		{
			rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y, newWidth, rectTransform.rect.height);
		}

		public static void SetHeight(this RectTransform rectTransform, float newHeight)
		{
			rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y, rectTransform.rect.width, newHeight);
		}

		public static void SetTop(this RectTransform rectTransform, float top)
		{
			rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
		}

		public static void SetBottom(this RectTransform rectTransform, float bottom)
		{
			rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
		}

		public static float GetWidth(this RectTransform rectTransform)
		{
			return rectTransform.rect.width;
		}

		public static float GetHeight(this RectTransform rectTransform)
		{
			return rectTransform.rect.height;
		}
	}
}
