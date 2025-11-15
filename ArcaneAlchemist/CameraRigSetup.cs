using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Optional: drop this on a bootstrap object, assign a CinemachineVirtualCamera,
/// and it will auto-follow the Player tagged "Player".
/// </summary>
public class CameraRigSetup : MonoBehaviour
{
	public CinemachineVirtualCamera vcam;

	void Start()
	{
		if (!vcam) vcam = FindObjectOfType<CinemachineVirtualCamera>();
		var player = GameObject.FindWithTag("Player");
		if (vcam && player)
		{
			vcam.Follow = player.transform;
			vcam.LookAt = player.transform;
		}
	}
}