using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LightningBoltMaterialDriver : MonoBehaviour
{
	public SpriteRenderer sr;
	public Rigidbody2D sourceBody;

	public string tileProp = "_Tile";      // Vector4
	public string distortProp = "_Distort";// float

	public Vector4 baseTile = new(3f, 8f, 0f, 0f);
	public float baseDistort = 0.35f;

	public float speedToTileY = 0.20f;
	public float speedToDistort = 0.02f;
	public float lerpSpeed = 0.1f;

	MaterialPropertyBlock mpb;
	bool hasTile, hasDistort;

	void Awake()
	{
		if (!sr) sr = GetComponent<SpriteRenderer>();
		if (!sourceBody) sourceBody = GetComponentInParent<Rigidbody2D>();
		mpb = new MaterialPropertyBlock();
		var mat = sr.sharedMaterial;
		hasTile = mat && mat.HasProperty(tileProp);
		hasDistort = mat && mat.HasProperty(distortProp);

		sr.GetPropertyBlock(mpb);
		if (hasTile) mpb.SetVector(tileProp, baseTile);
		if (hasDistort) mpb.SetFloat(distortProp, baseDistort);
		sr.SetPropertyBlock(mpb);
	}

	void Update()
	{
		if (!sr) return;
		float spd = sourceBody ? sourceBody.linearVelocity.magnitude : 0f;

		sr.GetPropertyBlock(mpb);

		if (hasTile)
		{
			Vector4 current = baseTile;
			try { current = mpb.GetVector(tileProp); } catch { }
			current.y = Mathf.Lerp(current.y, baseTile.y + spd * (baseTile.y * speedToTileY), lerpSpeed);
			mpb.SetVector(tileProp, current);
		}

		if (hasDistort)
		{
			float current = baseDistort;
			try { current = mpb.GetFloat(distortProp); } catch { }
			current = Mathf.Lerp(current, baseDistort + spd * speedToDistort, lerpSpeed);
			mpb.SetFloat(distortProp, current);
		}

		sr.SetPropertyBlock(mpb);
	}
}