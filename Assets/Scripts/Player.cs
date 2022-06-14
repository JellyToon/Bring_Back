using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	public static float HP;
	public static float coolDown;
	private float saveCool = 15f;
	public static int missionPoint;

	float hAxis;
	float vAxis;

	bool jDown;
	bool isjump;

	private Rigidbody rbody;
	public Animator anim;
	Vector3 moveVec;

	public static bool dead;

	public float speed;
	public float jumppower;

	[SerializeField] LayerMask EnemyMask;
	[SerializeField] float skillDistance = 15f;
	[SerializeField] Transform cameraTransform;

	// Start is called before the first frame update
	void Start()
    {
		HP = 5.0f;
		coolDown = saveCool;
		missionPoint = 0;
		dead = false;
		rbody = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
	}

    // Update is called once per frame
    void Update()
	{
		if (UIManager.option) return;

		GetInput();
		Jump();

		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (coolDown == 0f)
			{
				Skill();
				coolDown = saveCool;

				UIManager.degreeOfRisk += 1;
				Enemy.changeRisk = true;
				Drone.changeRisk = true;
			}
		}

		if (HP == 0)
			GameFail();
	}

	private void FixedUpdate()
	{
		if (UIManager.option) return;
		Move();
		FreezeRotation();
		CollTimeController();
	}

	void GetInput()
	{
		hAxis = Input.GetAxisRaw("Horizontal");
		vAxis = Input.GetAxisRaw("Vertical");
		jDown = Input.GetButtonDown("Jump");
	}

	void Move()
	{
		Vector3 moveInput = new Vector3(hAxis, 0f, vAxis);

		bool isMove = moveInput.magnitude != 0;
		anim.SetBool("isRun", isMove);

		if(isMove)
		{
			Vector3 lookForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
			Vector3 lookRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;
			Vector3 moveDir = lookForward * moveInput.z + lookRight * moveInput.x;
			moveDir.Normalize();

			transform.forward = moveDir;
			transform.position += moveDir * Time.deltaTime * speed;
		}
	}

	void Jump()
	{
		if (jDown && !isjump)
		{
			rbody.AddForce(Vector3.up * jumppower, ForceMode.Impulse);
			anim.SetTrigger("Jumping");
			isjump = true;
		}
	}

	public static void CollTimeController()
	{
		if (coolDown <= 0f)
		{
			coolDown = 0f;
			return;
		}
		if (coolDown != 0f)
			coolDown -= Time.deltaTime;
	}

	private void Skill()
	{
		Collider[] EnemyColliders = Physics.OverlapSphere(transform.position, skillDistance, EnemyMask);

		for (int i = 0; i < EnemyColliders.Length; ++i)
		{
			if (EnemyColliders[i].name == "Drone")
			{
				EnemyColliders[i].GetComponent<Drone>().Stoping();
			}
			if (EnemyColliders[i].name == "Enemy")
			{
				EnemyColliders[i].GetComponent<Enemy>().Stoping();
			}
		}
	}

	private void GameClear()
	{
		SceneManager.LoadScene("Clear");
	}

	private void GameFail()
	{
		SceneManager.LoadScene("Fail");
	}

	void FreezeRotation()
	{
		rbody.angularVelocity = Vector3.zero;
	}

	void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.tag == "Floor")
		{
			isjump = false;
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.gameObject.tag == "Interaction" && Input.GetKeyDown(KeyCode.E))
		{
			if (other.name == "Mushin")
			{
				Destroy(other.gameObject);
				missionPoint++;
			}

			if(other.name == "ClearArea1")
			{
				GameClear();
			}
		}
	}
}
