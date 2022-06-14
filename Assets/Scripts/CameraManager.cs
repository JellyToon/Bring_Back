using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Transform cameraRig;
	[SerializeField] float dist = 5f;

	[SerializeField] float xSpeed = 90f;
	[SerializeField] float ySpeed = 70f;

	[SerializeField] float yMinLimit = -10f;
	[SerializeField] float yMaxLimit = 45f;

	private float x;
	private float y;

	//[SerializeField] Transform camera;

	float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
		{
			angle += 360;
		}
		if (angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp(angle, min, max);
	}

	// Start is called before the first frame update
	void Start()
	{
		//camera = GetComponent<Transform>();

		Vector3 angles = transform.eulerAngles;
		x = angles.x - 90f;
		y = angles.y + 40f;
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void LateUpdate()
	{
		if (UIManager.option) return;
		if (target)
		{

			if (dist >= 10f)
			{
				dist = 10f;
			}

			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 position = rotation * new Vector3(0, 0f, -dist) + target.position + new Vector3(0f, 0, 0f);
			position.y += 4f;

			transform.rotation = rotation;
			transform.position = position;
		}

		RaycastHit hit;
		Vector3 rayDir = (transform.position - cameraRig.position).normalized;
		LayerMask layerMask = 1 << LayerMask.NameToLayer("Player");
		if(Physics.Raycast(cameraRig.position, rayDir, out hit, 10f, ~layerMask))
		{
			dist = Vector3.Distance(cameraRig.position, hit.point);
		}
		else
			dist = 10f;
	}
}
