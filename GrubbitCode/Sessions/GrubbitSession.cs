using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Grubbit
{
	public abstract class GrubbitSession<T> : GrubbitSession where T : GrubbitSession<T>
	{
		public static T Instance { get; private set; }

		protected override void Awake()
		{
			Instance = (T)this;
			base.Awake();
		}
	}

	public class GrubbitSession : MonoBehaviour
	{
		private Transform parentTransform;
		public bool SessionActive { get; private set; } = false;
		public System.DateTime SessionStartTime { get; private set; }
		public System.DateTime SessionEndTime { get; private set; }
		public string UniquePlayerId { get; set; }

		protected virtual void Awake() { }
		public static T CreateSession<T>(GameObject parentGo = null, bool initialize = true) where T : GrubbitSession
		{
			var desiredMode = typeof(T);
			T session;

			if (parentGo != null)
			{
				session = (T)parentGo.AddComponent(desiredMode);
				session.parentTransform = parentGo.transform;
			}
			else
			{
				var newGo = new GameObject { name = $"{desiredMode.Name}" };
				session = (T)newGo.AddComponent(desiredMode);
			}

			if (initialize)
			{
				session.LoadSession();
			}

			return session;
		}

		public virtual void LoadSession()
		{
			SessionActive = true;
			SessionStartTime = System.DateTime.UtcNow;
		}

		public virtual void UnloadSession()
		{
			SessionActive = false;
			SessionEndTime = System.DateTime.UtcNow;
			if (parentTransform)
			{
				Destroy(this);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public virtual void SetupConnectingPlayerSessionData(string playerName, ulong clientId, string playerId, OnlinePlayerSessionData sessionPlayerData)
		{

		}
	}
}