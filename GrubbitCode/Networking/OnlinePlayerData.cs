using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Grubbit
{
	public struct OnlinePlayerSessionData : IEquatable<OnlinePlayerSessionData>, INetworkSerializable
	{
		public bool isConnected;
		public ulong clientId;
		public FixedString64Bytes playerName;
		public FixedString64Bytes uniquePlayerId;

		public bool Equals(OnlinePlayerSessionData other)
		{
			return
				clientId == other.clientId &&
				playerName == other.playerName &&
				uniquePlayerId == other.uniquePlayerId;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref clientId);
			serializer.SerializeValue(ref playerName);
			serializer.SerializeValue(ref uniquePlayerId);
		}

		public void Reinitialize()
		{
			playerName = "";
			clientId = 0;
			isConnected = false;
		}
	}
}