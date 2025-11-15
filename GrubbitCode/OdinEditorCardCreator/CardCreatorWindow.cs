using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Overlays;
using UnityEditor.Rendering;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using static GrubbitEnums;
using static UnityEngine.Rendering.GPUSort;

namespace Grubbit
{
	public class CardAssetFolderViewWindow : OdinMenuEditorWindow
	{
		[HorizontalGroup("Assets", 200)]
		protected override OdinMenuTree BuildMenuTree()
		{
			var menuTree = new OdinMenuTree();
			menuTree.Config.DrawSearchToolbar = true;
			menuTree.AddAllAssetsAtPath("CardAssets", "Assets/Grubbit/Resources/CardAssets", typeof(CardData), false,
				false);
			return menuTree;
		}
	}

	public class CardSplashArtFolderViewWindow : OdinMenuEditorWindow
	{
		[HorizontalGroup("Assets", 200)]
		protected override OdinMenuTree BuildMenuTree()
		{
			var menuTree = new OdinMenuTree();
			menuTree.Config.DrawSearchToolbar = true;
			menuTree.AddAllAssetsAtPath("CardAssets", "Assets/Grubbit/Art/Textures/CardSplashArt", typeof(Sprite), false, false);
			return menuTree;
		}
	}

	public class CardCreatorWindow : OdinEditorWindow
	{
		[MenuItem("Grubbit Tools/Card Asset Editor")]
		private static void OpenWindow()
		{
			GetWindow<CardCreatorWindow>().Show();
			GetWindow<CardAssetFolderViewWindow>().Show();
			//GetWindow<CardSplashArtFolderViewWindow>().Show();
		}

		protected override void Initialize()
		{
			base.Initialize();
			originalCards = new List<CardData>();
			newCards = new List<CardData>();
			cardsTable = new List<CardData>();

			var allCardData = Resources.LoadAll<CardData>("CardAssets");
			originalCards.AddRange(allCardData);
			cardsTable.AddRange(originalCards);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Save();
		}

		private List<CardData> originalCards;
		private List<CardData> newCards;

		[HorizontalGroup("cardImgs")]
		[TableList(AlwaysExpanded = true, CellPadding = 0, DefaultMinColumnWidth = 75,  IsReadOnly = false)]
		public List<CardData> cardsTable;
		
		[Button(ButtonSizes.Large)]
		private void CreateNewCard()
		{
			var newCard = new CardData();
			cardsTable.Add(newCard);
			newCards.Add(newCard);
		}

		[Button(ButtonSizes.Large, ButtonStyle.Box)]
		private void Save()
		{
			if (newCards != null && newCards.Count != 0)
			{
				foreach (var newCard in newCards)
				{
					if (newCard != null && cardsTable.Contains(newCard))
					{
						if (!AssetDatabase.Contains(newCard))
						{
							//if (!string.IsNullOrWhiteSpace(newCard.description))
							//{
							//	//newCard.description = StringsToBold.ReplaceWithBold(newCard.description);
							//}

							AssetDatabase.CreateAsset(newCard, $"Assets/Grubbit/Resources/CardAssets/{newCard.cardName}.asset");
						}
					}
				}

				newCards.Clear();
			}

			CleanAssetFolder();
			Repaint();
			GetWindow<CardCreatorWindow>().Focus();
		}

		private void CleanAssetFolder()
		{
			var allCardData = Resources.LoadAll<CardData>("CardAssets");

			foreach (var cardData in allCardData)
			{
				if (!cardsTable.Contains(cardData))
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(cardData));
				}
			}
		}
	}
}