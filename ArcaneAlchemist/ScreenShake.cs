using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Drop this in the scene. Assign a CinemachineImpulseSource (or it will add one).
/// Call ScreenShake.Shake(amplitude, duration).
/// Requires a Cinemachine Impulse Listener on your vcam or main camera (usually added automatically in newer versions).
/// </summary>
public class ScreenShake : MonoBehaviour
{
	static ScreenShake _inst;
	CinemachineImpulseSource _source;

	void Awake()
	{
		if (_inst && _inst != this) { Destroy(gameObject); return; }
		_inst = this;

		_source = GetComponent<CinemachineImpulseSource>();
		if (!_source) _source = gameObject.AddComponent<CinemachineImpulseSource>();

		// Reasonable defaults
		var idef = new CinemachineImpulseDefinition();
		idef.TimeEnvelope.AttackTime = 0.01f;
		idef.TimeEnvelope.SustainTime = 0.02f;
		idef.TimeEnvelope.DecayTime = 0.12f;

		_source.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
		_source.ImpulseDefinition.TimeEnvelope = idef.TimeEnvelope;
		_source.DefaultVelocity = Vector3.down;
	}

	public static void Shake(float amplitude = 1f, float duration = 0.12f)
	{
		if (!_inst || !_inst._source) return;
		var def = _inst._source.ImpulseDefinition;
		def.TimeEnvelope.SustainTime = Mathf.Max(0f, duration * 0.25f);
		def.TimeEnvelope.DecayTime = Mathf.Max(0.06f, duration * 0.75f);
		_inst._source.GenerateImpulseWithForce(Mathf.Max(0.001f, amplitude));
	}
}