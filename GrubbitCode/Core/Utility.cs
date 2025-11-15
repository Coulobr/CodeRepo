using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Cinemachine;
using Unity.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

#region Resharper Comments
// ReSharper disable Unity.InefficientPropertyAccess
// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
#endregion

namespace Grubbit
{
	public static class Utility
	{
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			var comp = go.GetComponent<T>();
			return comp != null ? comp : go.AddComponent<T>();
		}

		public static T GetOrAddCinemachineComponent<T>(this CinemachineVirtualCamera cam)
			where T : CinemachineComponentBase
		{
			var comp = cam.GetCinemachineComponent<T>();
			return comp != null ? comp : cam.AddCinemachineComponent<T>();
		}

		public static void UnityEditor_InsertSpaces(int num)
		{
#if UNITY_EDITOR
			for (var i = 0; i < num; ++i)
			{
				EditorGUILayout.Space();
			}
#endif
		}

		public static Image CreateImageObject(Transform parent = null)
		{
			var newImageObject = new GameObject().AddComponent<Image>();
			newImageObject.transform.SetParent(parent);
			newImageObject.transform.localPosition = Vector3.zero;
			return newImageObject;
		}

		public static TextMeshProUGUI CreateTMPObject(Transform parent = null)
		{
			var newTMPObject = new GameObject().AddComponent<TextMeshProUGUI>();
			newTMPObject.transform.SetParent(parent);
			newTMPObject.transform.localPosition = Vector3.zero;
			return newTMPObject;
		}

		/// <summary>
		/// Checks to see if the given gameobject exists, and if it does change it's active state.
		/// </summary>
		public static void SetActive(MonoBehaviour desiredObject, bool active, bool showWarning = false)
		{
			if (desiredObject != null)
			{
				desiredObject.gameObject.SetActive(active);
			}
			else if (showWarning)
			{
				Debug.LogWarning("GameObject is null or invalid...");
			}
		}

		/// <summary>
		/// Checks to see if the given gameobject exists, and if it does change it's active state.
		/// </summary>
		public static void SetActive(GameObject desiredObject, bool active, bool showWarning = false)
		{
			if (desiredObject != null)
			{
				desiredObject.SetActive(active);
			}
			else if (showWarning)
			{
				Debug.LogWarning("GameObject is null or invalid...");
			}
		}

		/// <summary>
		/// Checks to see if a given TMP text exists, and if it does change it's text.
		/// </summary>
		public static void SetText(TextMeshProUGUI textObject, string desiredText, bool showWarning = false)
		{
			if (textObject != null)
			{
				textObject.text = desiredText;
			}
			else if (showWarning)
			{
				Debug.LogWarning("TextObject is null or invalid...");
			}
		}

		public static bool String_ContainsAny(this string source, params string[] values)
		{
			return values.Any(source.Contains);
		}

		public static bool String_ContainsAll(this string source, params string[] values)
		{
			return values.All(source.Contains);
		}

		public static float ParseFloat(string desiredText, float defaultValue)
		{
			if (!float.TryParse(desiredText, out float value))
				value = defaultValue;
			return value;
		}

		public static int ParseInt(string desiredText, int defaultValue)
		{
			if (!int.TryParse(desiredText, out int value))
				value = defaultValue;
			return value;
		}

		public static float RoundValToNearestMultiple(float val, float multiple, bool isRoundUp,
			bool isTrendsSlider = false)
		{
			if (isRoundUp)
			{
				if (!isTrendsSlider)
				{
					return val % multiple == 0 ? val + multiple : val + (multiple - (val % multiple));
				}

				return val % multiple == 0 ? val : val + (multiple - (val % multiple));
			}
			else
			{
				if (!isTrendsSlider)
				{
					return val % multiple == 0 ? val - multiple
						: val > 0 ? val - (val % multiple)
						: (val - multiple) + ((Mathf.Abs(val) + multiple) % multiple);
				}

				return val % multiple == 0 ? val
					: val > 0 ? val - (val % multiple) : (val - multiple) + ((Mathf.Abs(val) + multiple) % multiple);
			}
		}

		public static Vector3 GetMouseWorldPosition()
		{
			var vector = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
			vector.z = 0f;
			return vector;
		}

		public static Vector3 GetMouseWorldPositionWithZ()
		{
			return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
		}

		public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
		{
			return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
		}

		public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
		{
			var worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
			return worldPosition;
		}

		/// <summary>
		/// Returns 00-FF, value 0->255
		/// </summary>
		public static string DecimalToHex(int value)
		{
			return value.ToString("X2");
		}

		/// <summary>
		/// Returns 0-255
		/// </summary>
		public static int HexToDecimal(string hex)
		{
			return Convert.ToInt32(hex, 16);
		}

		/// <summary>
		/// Returns a hex string based on a number between 0->1
		/// </summary>
		public static string Dec01ToHex(float value)
		{
			return DecimalToHex((int)Mathf.Round(value * 255f));
		}

		/// <summary>
		/// Returns a float between 0->1
		/// </summary>
		public static float HexToDec01(string hex)
		{
			return HexToDecimal(hex) / 255f;
		}

		/// <summary>
		/// Converts a Color into a Color32.
		/// </summary>
		public static Color ConvertColor32ToColor(Color32 givenColor)
		{
			Color color = new Color((givenColor.r / 255f), (givenColor.g / 255f), (givenColor.b / 255f),
				(givenColor.a / 255f));
			return color;
		}

		/// <summary>
		/// Converts a Color32 into a Color.
		/// </summary>
		public static Color32 ConvertColorToColor32(Color givenColor)
		{
			Color32 color = new Color32((byte)(givenColor.r * 255f), (byte)(givenColor.g * 255f),
				(byte)(givenColor.b * 255f), (byte)(givenColor.a * 255f));
			return color;
		}

		/// <summary>
		/// Get Hex Color FF00FF
		/// </summary>
		public static string GetStringFromColor(Color color)
		{
			string red = Dec01ToHex(color.r);
			string green = Dec01ToHex(color.g);
			string blue = Dec01ToHex(color.b);
			return red + green + blue;
		}

		/// <summary>
		/// Get Hex Color FF00FFAA
		/// </summary>
		public static string GetStringFromColorWithAlpha(Color color)
		{
			string alpha = Dec01ToHex(color.a);
			return GetStringFromColor(color) + alpha;
		}

		/// <summary>
		/// Sets out values to Hex String 'FF'
		/// </summary>
		public static void GetStringFromColor(Color color, out string red, out string green, out string blue,
			out string alpha)
		{
			red = Dec01ToHex(color.r);
			green = Dec01ToHex(color.g);
			blue = Dec01ToHex(color.b);
			alpha = Dec01ToHex(color.a);
		}

		/// <summary>
		/// Get Hex Color FF00FF
		/// </summary>
		public static string GetStringFromColor(float r, float g, float b)
		{
			string red = Dec01ToHex(r);
			string green = Dec01ToHex(g);
			string blue = Dec01ToHex(b);
			return red + green + blue;
		}

		/// <summary>
		/// Get Hex Color FF00FFAA
		/// </summary>
		public static string GetStringFromColor(float r, float g, float b, float a)
		{
			string alpha = Dec01ToHex(a);
			return GetStringFromColor(r, g, b) + alpha;
		}

		/// <summary>
		/// Get Color from Hex string FF00FFAA
		/// </summary>
		public static Color GetColorFromString(string color)
		{
			float red = HexToDec01(color.Substring(0, 2));
			float green = HexToDec01(color.Substring(2, 2));
			float blue = HexToDec01(color.Substring(4, 2));
			float alpha = 1f;

			if (color.Length >= 8)
			{
				// Color string contains alpha
				alpha = HexToDec01(color.Substring(6, 2));
			}

			return new Color(red, green, blue, alpha);
		}

		public static Vector3 GetRandomDirection()
		{
			return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
		}

		public static Vector3 GetVectorFromAngle(float desiredAngle)
		{
			var angleRad = desiredAngle * (Mathf.PI / 180f);
			return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
		}

		public static float GetFloatAngleFromVector(Vector3 desiredDirection)
		{
			desiredDirection = desiredDirection.normalized;
			var n = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
			if (n < 0)
				n += 360;
			return n;
		}

		public static int GetIntAngleFromVector(Vector3 desiredDirection)
		{
			desiredDirection = desiredDirection.normalized;
			var n = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
			if (n < 0)
				n += 360;
			var angle = Mathf.RoundToInt(n);
			return angle;
		}

		public static int GetIntAngleFromVector180(Vector3 desiredDirection)
		{
			desiredDirection = desiredDirection.normalized;
			var n = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
			var angle = Mathf.RoundToInt(n);
			return angle;
		}

		public static Vector3 ApplyRotationToVector(Vector3 desiredVector, Vector3 vectorRotation)
		{
			return ApplyRotationToVector(desiredVector, GetFloatAngleFromVector(vectorRotation));
		}

		public static Vector3 ApplyRotationToVector(Vector3 desiredVector, float desiredAngle)
		{
			return Quaternion.Euler(0, 0, desiredAngle) * desiredVector;
		}

		public static Vector2 GetWorldUIPosition(Vector3 worldPosition, Transform parent, Camera uiCamera,
			Camera worldCamera)
		{
			var screenPosition = worldCamera.WorldToScreenPoint(worldPosition);
			var uiCameraWorldPosition = uiCamera.ScreenToWorldPoint(screenPosition);
			var localPos = parent.InverseTransformPoint(uiCameraWorldPosition);
			return new Vector2(localPos.x, localPos.y);
		}

		public static Vector3 GetWorldPositionFromUIZeroZ()
		{
			var vector = GetWorldPositionFromUI(Input.mousePosition, Camera.main);
			vector.z = 0f;
			return vector;
		}

		public static Vector3 GetWorldPositionFromUI()
		{
			return GetWorldPositionFromUI(Input.mousePosition, Camera.main);
		}

		public static Vector3 GetWorldPositionFromUI(Camera worldCamera)
		{
			return GetWorldPositionFromUI(Input.mousePosition, worldCamera);
		}

		public static Vector3 GetWorldPositionFromUI(Vector3 screenPosition, Camera worldCamera)
		{
			var worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
			return worldPosition;
		}

		public static Vector3 GetWorldPositionFromUI_Perspective()
		{
			return GetWorldPositionFromUI_Perspective(Input.mousePosition, Camera.main);
		}

		public static Vector3 GetWorldPositionFromUI_Perspective(Camera worldCamera)
		{
			return GetWorldPositionFromUI_Perspective(Input.mousePosition, worldCamera);
		}

		public static Vector3 GetWorldPositionFromUI_Perspective(Vector3 screenPosition, Camera worldCamera)
		{
			var ray = worldCamera.ScreenPointToRay(screenPosition);
			var xy = new Plane(Vector3.forward, new Vector3(0, 0, 0f));
			float distance;
			xy.Raycast(ray, out distance);
			return ray.GetPoint(distance);
		}

		public static string Log(string desiredMessage, Type type = null, bool showTimeStamp = true)
		{
			var message =
				$"[LOG] {(type != null ? $"[{type.Name}]" : "")} {(showTimeStamp ? $"[{DateTime.Now}]" : "")}: {desiredMessage}";
			Debug.Log(message);
			return message;
		}

		public static string LogWarning(string desiredMessage, Type type = null, bool showTimeStamp = true,
			[CallerLineNumber] int lineNumber = 0)
		{
			var message =
				$"[WARNING] {(type != null ? $"[{type.Name}.{lineNumber}]" : "")} {(showTimeStamp ? $"[{DateTime.Now}]" : "")}: {desiredMessage}";
			Debug.LogWarning(message);
			return message;
		}

		public static string LogWarning(Exception givenException, Type type = null, bool showTimeStamp = true,
			[CallerLineNumber] int lineNumber = 0)
		{
			var message =
				$"[WARNING] {(type != null ? $"[{type.Name}.{lineNumber}]" : "")} {(showTimeStamp ? $"[{DateTime.Now}]" : "")}: {givenException.Message} \n {givenException.StackTrace}";
			Debug.LogWarning(message);
			return message;
		}

		public static string LogError(string desiredMessage, Type type = null, bool showTimeStamp = true,
			[CallerLineNumber] int lineNumber = 0)
		{
			var message =
				$"[ERROR] {(type != null ? $"[{type.Name}.{lineNumber}]" : "")} {(showTimeStamp ? $"[{DateTime.Now}]" : "")}: {desiredMessage}";
			Debug.LogError(message);
			return message;
		}

		public static string LogError(Exception givenException, Type type = null, bool showTimeStamp = true,
			[CallerLineNumber] int lineNumber = 0)
		{
			var message =
				$"[ERROR] {(type != null ? $"[{type.Name}.{lineNumber}]" : "")} {(showTimeStamp ? $"[{DateTime.Now}]" : "")}: {givenException.Message} \n {givenException.StackTrace}";
			Debug.LogError(message);
			return message;
		}

		public static HttpStatusCode ConvertToHTTPStatusCode(long givenResponseCode)
		{
			switch (givenResponseCode)
			{
				case 100:
					return HttpStatusCode.Continue;
				case 101:
					return HttpStatusCode.SwitchingProtocols;
				case 200:
					return HttpStatusCode.OK;
				case 201:
					return HttpStatusCode.Created;
				case 202:
					return HttpStatusCode.Accepted;
				case 203:
					return HttpStatusCode.NonAuthoritativeInformation;
				case 204:
					return HttpStatusCode.NoContent;
				case 205:
					return HttpStatusCode.ResetContent;
				case 206:
					return HttpStatusCode.PartialContent;
				case 300:
					return HttpStatusCode.Ambiguous;
				case 301:
					return HttpStatusCode.Moved;
				case 302:
					return HttpStatusCode.Redirect;
				case 303:
					return HttpStatusCode.RedirectMethod;
				case 304:
					return HttpStatusCode.NotModified;
				case 305:
					return HttpStatusCode.UseProxy;
				case 306:
					return HttpStatusCode.Unused;
				case 307:
					return HttpStatusCode.TemporaryRedirect;
				case 400:
					return HttpStatusCode.BadRequest;
				case 401:
					return HttpStatusCode.Unauthorized;
				case 402:
					return HttpStatusCode.PaymentRequired;
				case 403:
					return HttpStatusCode.Forbidden;
				case 404:
					return HttpStatusCode.NotFound;
				case 405:
					return HttpStatusCode.MethodNotAllowed;
				case 407:
					return HttpStatusCode.ProxyAuthenticationRequired;
				case 408:
					return HttpStatusCode.RequestTimeout;
				case 409:
					return HttpStatusCode.Conflict;
				case 410:
					return HttpStatusCode.Gone;
				case 411:
					return HttpStatusCode.LengthRequired;
				case 412:
					return HttpStatusCode.PreconditionFailed;
				case 413:
					return HttpStatusCode.RequestEntityTooLarge;
				case 414:
					return HttpStatusCode.RequestUriTooLong;
				case 415:
					return HttpStatusCode.UnsupportedMediaType;
				case 416:
					return HttpStatusCode.RequestedRangeNotSatisfiable;
				case 417:
					return HttpStatusCode.ExpectationFailed;
				case 426:
					return HttpStatusCode.UpgradeRequired;
				case 500:
					return HttpStatusCode.InternalServerError;
				case 501:
					return HttpStatusCode.NotImplemented;
				case 502:
					return HttpStatusCode.BadGateway;
				case 503:
					return HttpStatusCode.ServiceUnavailable;
				case 504:
					return HttpStatusCode.GatewayTimeout;
				case 505:
					return HttpStatusCode.HttpVersionNotSupported;
				default:
					return HttpStatusCode.NotAcceptable;
			}
		}

		public static string Truncate(this string desiredString, int maxLength)
		{
			return desiredString.Length > maxLength ? desiredString.Substring(0, maxLength) : desiredString;
		}

		public static string GetJSONFromFile(string dir, bool checkIfFileExists = false, Type desiredType = null)
		{
			var fileExists = !checkIfFileExists || File.Exists(dir);

			if (fileExists)
			{
				try
				{
					string json;

					using (var f = new FileStream(dir, FileMode.Open, FileAccess.Read))
					{
						using (var s = new StreamReader(f, Encoding.UTF8))
						{
							json = s.ReadToEnd();
							s.Close();
							f.Close();
						}
					}

					return json.Replace("%", "");
				}
				catch (Exception ex)
				{
					LogError(ex, desiredType);
					return "";
				}
			}

			return "";
		}
	}

	public static class GrubbitCardUtility
	{
		public static Dictionary<string, CardData> loadedCardAssets = new Dictionary<string, CardData>();

		private static readonly Dictionary<string, object> cardDictionary = new Dictionary<string, object>()
		{
			{"1", typeof(CardABigSword)},
			{"2", typeof(CardADarkChoice)},
			{"3", typeof(CardAFriendlyWager)},
			{"4", typeof(CardAHardSwing)},
			{"5", typeof(CardANewUpgrade)},
			{"6", typeof(CardAWorthySacrifice)},
			{"7", typeof(CardAccountant)},
			{"8", typeof(CardAcidBlast)},
			{"9", typeof(CardAcidSpitter)},
			{"10", typeof(CardAcrobat)},
			{"11", typeof(CardAkimbo)},
			{"12", typeof(CardAlesiaTheAssassin)},
			{"13", typeof(CardAmbush)},
			{"14", typeof(CardAnkleBiter)},
			{"15", typeof(CardAnnoyingFly)},
			{"16", typeof(CardAntiAcidCoatingSpray)},
			{"17", typeof(CardApprenticeForger)},
			{"18", typeof(CardArenaChampion)},
			{"19", typeof(CardArmsDealer)},
			{"20", typeof(CardArtificialIntelligence)},
			{"21", typeof(CardAssistant)},
			{"22", typeof(CardAuction)},
			{"23", typeof(CardBackfire)},
			{"24", typeof(CardBackpacker)},
			{"25", typeof(CardBagOfTricks)},
			{"26", typeof(CardBargainDealer)},
			{"27", typeof(CardBarrelOfBooze)},
		};

		public static void GetOrLoadCardDataById(string cardId, Action<CardData> onCompleteAction)
		{
			if (loadedCardAssets != null && loadedCardAssets.Count != 0)
			{
				onCompleteAction.Invoke(loadedCardAssets.GetValueOrDefault(cardId));
				return; 
			}

			LoadAllCards(cardId, onCompleteAction);
		}

		public static CardData GetCardDataById(string cardId)
		{
			if (loadedCardAssets != null && loadedCardAssets.Count != 0)
			{
				return loadedCardAssets.GetValueOrDefault(cardId);
			}
			return null;
		}

		public static void LoadAllCards(string cardId, Action<CardData> onCompleteAction = null)
		{
			var handle = Addressables.LoadAssetsAsync<CardData>("CardData");
			handle.ReleaseHandleOnCompletion();
			handle.Completed += (opHandle) =>
			{
				OnCardLoadingComplete(opHandle);
				onCompleteAction?.Invoke(loadedCardAssets.GetValueOrDefault(cardId));
			};
		}

		public static void LoadAllCards()
		{
			var handle = Addressables.LoadAssetsAsync<CardData>("CardData");
			handle.ReleaseHandleOnCompletion();
			handle.Completed += OnCardLoadingComplete;

		}

		private static void OnCardLoadingComplete(AsyncOperationHandle<IList<CardData>> asyncResult)
		{
			foreach (var cardData in asyncResult.Result)
			{
				loadedCardAssets.TryAdd(cardData.cardId, cardData);
			}

			Debug.Log($"Loaded {loadedCardAssets.Count} Cards");
		}

		public static object GetCardTypeById(string cardId)
		{
			cardDictionary.TryGetValue(cardId, out var value);
			return value;
		}

		public static string GetCardIdByType(object cardType)
		{
			return cardDictionary.FirstOrDefault(x => x.Value == cardType).Key;
		}
	}
}