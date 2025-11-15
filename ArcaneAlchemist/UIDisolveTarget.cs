using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Assign a full-screen UI Image that uses a material with a float property (e.g. "_Dissolve").
/// The value should dissolve/clip the image as it goes from 0 -> 1.
/// </summary>
[RequireComponent(typeof(Image))]
public class UIDissolveTarget : MonoBehaviour
{
	public string dissolveProperty = "_Dissolve";
	Image img;
	Material runtimeMat;
	float current;

	void Awake()
	{
		img = GetComponent<Image>();
		// Use a unique material instance so we don't affect sharedMaterials
		runtimeMat = Instantiate(img.material);
		img.material = runtimeMat;
		SetDissolve(0f);
	}

	public void SetDissolve(float v)
	{
		current = Mathf.Clamp01(v);
		if (runtimeMat && runtimeMat.HasProperty(dissolveProperty))
			runtimeMat.SetFloat(dissolveProperty, current);
	}

	public float Get() => current;
}