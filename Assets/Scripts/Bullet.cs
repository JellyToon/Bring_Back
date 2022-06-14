using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] float shotPower = 500f;
	private Transform shotTransform;
	private Transform playerPosition;
	Vector3 direction;
	// Start is called before the first frame update
	void Start()
	{
		shotTransform = GameObject.Find("ShotPosition").GetComponent<Transform>();
		playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

		direction = (playerPosition.position - shotTransform.position);
		direction.y = playerPosition.position.y;
		direction.Normalize();
		transform.position = shotTransform.position;

		Destroy(gameObject, 5f);
	}

	// Update is called once per frame
	void Update()
	{
		Debug.Log("Shot");

		Rigidbody rigidbody = GetComponent<Rigidbody>();
		rigidbody.AddForce(direction * shotPower);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			Player player = other.GetComponent<Player>();
			player.anim.SetTrigger("Damaging");
			Player.HP -= 1;
			Destroy(gameObject);
		}
		if (other.CompareTag("Wall") || other.CompareTag("Window"))
			Destroy(gameObject);
	}
}
