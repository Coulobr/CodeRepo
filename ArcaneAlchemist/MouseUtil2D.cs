using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class MouseUtil2D
{
	public static Vector2 GetWorldOnTransformPlane(Transform plane, Camera cam = null)
	{
		if (!cam) cam = Camera.main;

		// Screen mouse
#if ENABLE_INPUT_SYSTEM
		Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue()
											   : (Vector2)Input.mousePosition;
#else
        Vector2 screen = Input.mousePosition;
#endif
		// Depth to use for ScreenToWorldPoint:
		//  - Ortho: any depth works, we’ll snap z afterward
		//  - Perspective: use the distance from camera to the plane point
		float depth = cam.orthographic
			? Mathf.Abs(plane.position.z - cam.transform.position.z)
			: Vector3.Dot(plane.position - cam.transform.position, cam.transform.forward);

		Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
		wp.z = plane.position.z; // lock to the plane’s Z (critical!)
		return (Vector2)wp;
	}

	public static Vector2 AimDirFrom(Transform origin, Camera cam = null)
	{
		Vector2 mouseWorld = GetWorldOnTransformPlane(origin, cam);
		Vector2 dir = mouseWorld - (Vector2)origin.position;
		return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
	}
}
