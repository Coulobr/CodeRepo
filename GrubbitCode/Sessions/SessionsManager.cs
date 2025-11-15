using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Grubbit
{
	public class SessionsManager : GrubbitSingleton<SessionsManager>
	{
		public GrubbitSession ServerSession { get; private set; }
		public GrubbitSession ClientSession { get; private set; }

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		public T LoadSession<T>(bool isServer, string playerId = "", bool initialize = true) where T : GrubbitSession
		{
			if (isServer)
			{
				if (ServerSession != null)
				{
					ServerSession.UnloadSession();
				}
				return (T)(ServerSession = GrubbitSession.CreateSession<T>(gameObject));
			}
			else
			{
				if (ClientSession != null)
				{
					ClientSession.UnloadSession();
				}

				ClientSession = GrubbitSession.CreateSession<T>(gameObject);
				ClientSession.UniquePlayerId = playerId;
				return (T)ClientSession;
			}
		}
	}
}


