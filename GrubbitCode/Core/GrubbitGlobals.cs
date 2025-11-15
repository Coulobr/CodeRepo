using System;
using Grubbit;
using UnityEngine;

public static class GrubbitGlobals
{
	public static bool IsServer => Application.platform == RuntimePlatform.LinuxServer;
	public static string UniquePlayerGuid => _uniquePlayerGuid;

	private static string _uniquePlayerGuid;
	public static int CurrentLanguage => _currentLanguage;

	private static int _currentLanguage;

	public static void LoadGlobals()
	{
		_uniquePlayerGuid = PlayerPrefs.GetString("UNIQUE_PLAYER_GUID", new Guid().ToString());
		_currentLanguage = PlayerPrefs.GetInt("CURRENT_LANGUAGE", 0);
	}


}

public class ErrorPayload
{
	public string message;
}

