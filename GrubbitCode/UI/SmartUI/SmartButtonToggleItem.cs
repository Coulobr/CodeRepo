using System;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Grubbit;
using System.Collections.Generic;

[Serializable]
public class SmartButtonToggleItem
{
	public GrubbitEnums.ToggleItemType type;
	public Image targetImage;
	public TextMeshProUGUI targetTMPText;
	public SmartButton targetSmartButton;
	public RectTransform rectTransform;
	public Color activeColor;
	public Color inactiveColor;
	public Color disabledColor;
	public float colorAlpha;
	public bool shouldColor;
	public bool overrideColorAlpha;
	public bool toggleGameObjectActiveState;
	public bool hideWhenInactive;
	public bool showWhenInactive;
	public bool hideWhenActive;
	public bool showWhenActive;
	public bool hideWhenDisabled;
	public bool showWhenDisabled;
	public bool uniqueDisabledColor;
	public bool uniqueDisabledText;
	public bool changeSpriteWhenStateToggled;
	public bool moveAnchorPos;
	public bool animateAnchorPosMovement;
	public bool changeText;
	public bool changeTargetSmartButtonStateWhenThisActivated;
	public bool changeTargetSmartButtonStateWhenThisDeactivated;
	public bool targetSmartButtonStateWhenThisActive;
	public bool targetSmartButtonStateWhenThisInactive;
	public float animSpeed;
	public string textWhenActive;
	public string textWhenInactive;
	public string textWhenDisabled;
	public Vector2 anchorMinActive;
	public Vector2 anchorMaxActive;
	public Vector2 anchorPosActive;
	public Vector2 anchorMinInactive;
	public Vector2 anchorMaxInactive;
	public Vector2 anchorPosInactive;
	public Vector2 pivotActive;
	public Vector2 pivotInactive;
	public Sprite normalSpriteWhenActive;
	public Sprite pressedSpriteWhenActive;
	public Sprite highlightedSpriteWhenActive;
	public Sprite disabledSpriteWhenActive;
	public Sprite disabledSpriteWhenInactive;
	public Sprite pressedSpriteWhenInactive;
	public Sprite highlightedSpriteWhenInactive;
	public Sprite inactiveSpriteWhenInactive;
	public bool showingItem;
	public List<Tween> animateTweens = new List<Tween>();
}


