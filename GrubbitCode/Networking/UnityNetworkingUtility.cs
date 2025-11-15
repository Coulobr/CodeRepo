using System;
using System.Collections;
using System.Net;
using TMPro;
using DG.Tweening;

using Unity.Netcode;
using Unity.Networking;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Multiplayer;
using UnityEngine.AddressableAssets;
using UnityEngine.Analytics;
using UnityEngine.Jobs;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;

namespace Grubbit.NetworkingUtilities
{
	public class GetRequestPackage
	{
		public string baseUrl;
		public string requestUrl;
		public string bearerToken;
		public QueryParameter[] queryParameters;
		public Action<string, string, HttpStatusCode> callback;
	}

	public class PostRequestPackage
	{
		public string baseUrl;
		public string requestUrl;
		public string bearerToken;
		public string requestContentType;
		public string formContentType;
		public QueryParameter[] queryParameters;
		public string jsonData;
		public WWWForm form;
		public Action<string, string, HttpStatusCode> callback;
	}

	public class QueryParameter
	{
		public string paramName;
		public string paramValue;
		public string Param => $"{paramName}={paramValue}";
	}

	public class UnityNetworkingUtility : MonoBehaviour
	{
		private static UnityNetworkingUtility instance;

		public static UnityNetworkingUtility Instance
		{
			get
			{
				if (instance == null)
				{
					var obj = new GameObject { name = "UnityNetworkingUtility" };
					DontDestroyOnLoad(obj);
					instance = obj.AddComponent<UnityNetworkingUtility>();
				}

				return instance;
			}
		}

		public Coroutine AsyncGETRequest(GetRequestPackage package)
		{
			if (package == null)
			{
				Utility.LogError("The given package is null. Cannot create request...");
				return null;
			}

			if (string.IsNullOrEmpty(package.baseUrl) || string.IsNullOrEmpty(package.requestUrl))
			{
				Utility.LogError("The given package's URL is either null, empty, or incomplete. Cannot create request...");
				return null;
			}

			return StartCoroutine(AsyncGETRequestCR(package));
		}

		private IEnumerator AsyncGETRequestCR(GetRequestPackage package)
		{
			string returnResponse;
			var returnJSON = "";

			// First build the URI
			var fullUri = package.baseUrl.TrimEnd('/') + "/" + package.requestUrl.TrimStart('/');

			// Then add any query parameters if they're given
			if (package.queryParameters != null && package.queryParameters.Length > 0)
			{
				fullUri += $"?{package.queryParameters[0].Param}";

				if (package.queryParameters.Length > 1)
				{
					for (var i = 1; i < package.queryParameters.Length; ++i)
					{
						fullUri += $"&{package.queryParameters[i].Param}";
					}
				}
			}

			// Construct the unity web request
			var request = new UnityWebRequest(fullUri, UnityWebRequest.kHttpVerbGET) { timeout = 5000 };

			// Add a bearer token if it exists
			if (!string.IsNullOrEmpty(package.bearerToken))
			{
				request.SetRequestHeader("Authorization", $"Bearer {package.bearerToken}");
			}

			request.downloadHandler = new DownloadHandlerBuffer();

			// Send out the request and wait for the response
			yield return request.SendWebRequest();

			// Parse out the return status code
			var returnStatusCode = Utility.ConvertToHTTPStatusCode(request.responseCode);

			// Determine if there was an error, and if not gather the return data as a raw string
			if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
			{
				Utility.LogError(request.error, GetType());
				returnResponse = request.error;
			}
			else
			{
				var json = request.downloadHandler.text;
				returnResponse = "success";
				returnJSON = json;
			}

			// Initiate the callback
			package.callback?.Invoke(returnResponse, returnJSON, returnStatusCode);
		}

		public Coroutine AsyncPOSTRequest(PostRequestPackage package)
		{
			if (package == null)
			{
				Utility.LogError("The given package is null. Cannot create request...");
				return null;
			}

			if (string.IsNullOrEmpty(package.baseUrl) || string.IsNullOrEmpty(package.requestUrl))
			{
				Utility.LogError("The given package's URL is either null, empty, or incomplete. Cannot create request...");
				return null;
			}

			return StartCoroutine(AsyncPOSTRequestCR(package));
		}

		private IEnumerator AsyncPOSTRequestCR(PostRequestPackage package)
		{
			string returnResponse;
			var returnJSON = "";

			// First build the URI
			var fullUri = package.baseUrl.TrimEnd('/') + "/" + package.requestUrl.TrimStart('/');

			// Then add any query parameters if they're given
			if (package.queryParameters != null && package.queryParameters.Length > 0)
			{
				fullUri += $"?{package.queryParameters[0].Param}";

				if (package.queryParameters.Length > 1)
				{
					for (var i = 1; i < package.queryParameters.Length; ++i)
					{
						fullUri += $"&{package.queryParameters[i].Param}";
					}
				}
			}

			// Construct the unity web request
			var request = package.form != null ? UnityWebRequest.Post(fullUri, package.form) : new UnityWebRequest(fullUri, UnityWebRequest.kHttpVerbPOST) { timeout = 5000 };

			// Add a bearer token if it exists
			if (!string.IsNullOrEmpty(package.bearerToken))
			{
				request.SetRequestHeader("Authorization", $"Bearer {package.bearerToken}");
			}

			// Set up the request header and construct a download handler
			if (package.form == null)
			{
				request.SetRequestHeader("Content-Type", !string.IsNullOrEmpty(package.requestContentType) ? package.requestContentType : "application/json-patch+json");
			}

			request.downloadHandler = new DownloadHandlerBuffer();

			// Convert the JSON string into raw bytes
			if (!string.IsNullOrEmpty(package.jsonData) && package.form == null)
			{
				var bytes = Encoding.ASCII.GetBytes(package.jsonData);
				request.uploadHandler = new UploadHandlerRaw(bytes) { contentType = !string.IsNullOrEmpty(package.formContentType) ? package.formContentType : "application/json" };
			}

			// Send out the request and wait for the response
			yield return request.SendWebRequest();

			// Parse out the return status code
			var returnStatusCode = Utility.ConvertToHTTPStatusCode(request.responseCode);

			// Determine if there was an error, and if not gather the return data as a raw string
			if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				Utility.LogError(request.error, GetType());
				returnResponse = request.error;
			}
			else if (request.result == UnityWebRequest.Result.ConnectionError)
			{
				var errorText = request.error;

				if (!string.IsNullOrEmpty(request.downloadHandler.text))
				{
					errorText = request.downloadHandler.text;
				}

				Utility.LogError(errorText, GetType());
				returnResponse = errorText;
			}
			else
			{
				var json = request.downloadHandler.text;
				returnResponse = "success";
				returnJSON = json;
			}

			// Initiate the callback
			package.callback?.Invoke(returnResponse, returnJSON, returnStatusCode);
		}
	}
}


