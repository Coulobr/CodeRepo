using System.Collections.Generic;
using System;
using UnityEngine;

namespace Grubbit
{
	public static class ListExtensions
	{
		public static void MoveIndex<T>(this List<T> desiredList, int startingIndex, int newIndex)
		{
			try
			{
				var itemCopy = desiredList[startingIndex];
				desiredList.RemoveAt(startingIndex);
				desiredList.Insert(newIndex, itemCopy);
			}
			catch (Exception)
			{
				Debug.LogError("Could not move item in the list.");
				throw;
			}
		}

		public static void MoveIndex<T>(this List<T> desiredList, T item, int newIndex)
		{
			try
			{
				var currentIndex = desiredList.IndexOf(item);
				var itemCopy = desiredList[currentIndex];
				desiredList.RemoveAt(currentIndex);
				desiredList.Insert(newIndex, itemCopy);
			}
			catch (Exception)
			{
				Debug.LogError("Could not move item in the list.");
				throw;
			}
		}
	}

}

