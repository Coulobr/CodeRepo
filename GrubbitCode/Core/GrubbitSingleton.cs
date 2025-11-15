namespace Grubbit
{
	using UnityEngine;

	public abstract class GrubbitSingleton<T> : MonoBehaviour where T : GrubbitSingleton<T>
	{
		public static T Instance { get; set; }
		protected virtual void Awake()
		{
			if (Instance != null) { DestroyImmediate(this); }
			Instance = (T)this;
			DontDestroyOnLoad(gameObject);
		}

		protected virtual void OnDestroy()
		{
			Instance = null;
		}
	}
}
