using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Enemy : MonoBehaviour
{
	private enum State { patrol, trace, Attack }
	private State state = State.patrol;

	private bool isStop = false;
	private float stopTime = 2f;

	[SerializeField] float viewAngle = 60f;
	[SerializeField] float viewDistance = 5f;

	[SerializeField] float walkSpeed = 2.0f;
	[SerializeField] float runSpeed = 5.0f;

	[SerializeField] Transform PlayerTransform;
	private Vector3 SavePosition;

	[SerializeField] Transform eyetransform;
	[SerializeField] LayerMask targetMask;
	[SerializeField] LayerMask windowMask;

	[SerializeField] Transform attackArea;
	[SerializeField] float attackRadius = 4.0f;
	[SerializeField] float attackDelay = 1.0f;
	private float attackDistanceMax;

	private NavMeshAgent agent;
	[SerializeField] Transform[] EnemyWayPoint;
	private int wayCount = 0;

	private Animator animator;

	public GameObject flashPrefab;
	[SerializeField] Transform flashTransform;
	[SerializeField] float destroyTimer = 1f;

	public GameObject bulletPrefab;

	private bool patroling;
	private bool wasTracing;
	private float stayTime_f = 0.0f;

	private bool changeOption = false;

	public static bool changeRisk;

	// Start is called before the first frame update
	void Start()
    {
		patroling = true;
		wasTracing = false;

		changeRisk = false;

		agent = GetComponent<NavMeshAgent>();
		InvokeRepeating("PatrolMove", 0f, 2f);

		animator = GetComponentInChildren<Animator>();

		attackDistanceMax = Vector3.Distance(transform.position, new Vector3(attackArea.position.x, transform.position.y, attackArea.position.z)) + attackRadius;
		attackDistanceMax += agent.radius;
	}

    // Update is called once per frame
    void Update()
    {
		if (UIManager.option) return;
		StopControl();
		if (isStop) return;
		if (changeRisk)
		{
			changeRisk = false;
			viewDistance += 1f;
			viewAngle += 4f;
			attackRadius += 0.5f;
			Vector3 Area = attackArea.position;
			Area += transform.forward * 1f;
			attackArea.position = Area;
		}
		Sight();
	}

	private void FixedUpdate()
	{
		if (UIManager.option)
		{
			agent.isStopped = true;
			changeOption = true;
			animator.SetFloat("Speed", 0f);
			return;
		}

		if (changeOption)
		{
			agent.isStopped = false;
			changeOption = false;
		}
		if (isStop) return;

		switch (state)
		{
			case State.patrol:
				animator.SetFloat("Speed", agent.velocity.magnitude);
				if (patroling) break;
				if (wasTracing)
				{
					if (agent.velocity == Vector3.zero)
					{
						animator.SetFloat("Speed", 0f);
						animator.SetBool("FindTarget", false);
						stayTime_f += Time.deltaTime;
						if (stayTime_f >= 1.0f && stayTime_f <= 1.5f)
							transform.Rotate(Vector3.up * 180.0f * Time.deltaTime);
						else if (2f <= stayTime_f && stayTime_f <= 2.5f)
							transform.Rotate(Vector3.up * -360.0f * Time.deltaTime);
						else if (3.5f <= stayTime_f)
						{
							wasTracing = false;
							--wayCount;
							if (wayCount <= 0) wayCount = 0;
							stayTime_f = 0f;
						}
					}
					break;
				}
				agent.speed = walkSpeed;
				agent.isStopped = false;
				InvokeRepeating("PatrolMove", 0f, 2f);
				patroling = true;
				break;
			case State.trace:
				agent.SetDestination(PlayerTransform.position);
				stayTime_f = 0f;
				break;
			case State.Attack:
				DoAttack();
				stayTime_f = 0f;
				break;
		}
	}

	private void Sight()
	{
		Collider[] targetColliders = Physics.OverlapSphere(eyetransform.position, viewDistance, targetMask);

		for (int i = 0; i < targetColliders.Length; ++i)
		{
			Transform targetTransform = targetColliders[i].transform;
			if (targetTransform.name != "Player") continue;

			Vector3 targetDirection = (targetTransform.position - eyetransform.position);
			targetDirection.y = targetTransform.position.y;
			targetDirection.Normalize();
			Vector3 targetfoot = (targetTransform.position - eyetransform.position);
			targetfoot.Normalize();

			float targetAngle = Vector3.Angle(targetDirection, transform.forward);

			if (viewAngle * 0.5f <= targetAngle) continue;

			RaycastHit hit;
			int layerMask = (1 << LayerMask.NameToLayer("Window"));
			if (Physics.Raycast(eyetransform.position, targetDirection, out hit, viewDistance, ~layerMask))
			{
				if (hit.transform.name == "Player")
				{
					if (state == State.patrol) CancelInvoke();
					if (Physics.Raycast(eyetransform.position, targetDirection, out hit, viewDistance))
					{
						if (hit.transform.CompareTag("Window"))
						{
							if (state == State.Attack)
								animator.SetBool("Attack", false);
							state = State.trace;
							agent.isStopped = false;
							patroling = false;
							agent.speed = runSpeed;
							wasTracing = true; ;
							animator.SetBool("FindTarget", true);
							return;
						}
					}

					if (Vector3.Distance(PlayerTransform.position, transform.position) <= attackDistanceMax)
					{
						if (state == State.Attack) return;
						animator.SetBool("Attack", true);
						state = State.Attack;
						agent.isStopped = true;
						patroling = false;
						return;
					}
					if (state == State.Attack)
						animator.SetBool("Attack", false);
					state = State.trace;
					agent.isStopped = false;
					patroling = false;
					agent.speed = runSpeed;
					wasTracing = true; ;
					animator.SetBool("FindTarget", true);
					return;
				}
			}
			if (Physics.Raycast(eyetransform.position, targetfoot, out hit, viewDistance))
			{
				if (hit.transform.name == "Player")
				{
					if (state == State.patrol) CancelInvoke();

					if (Physics.Raycast(eyetransform.position, targetDirection, out hit, viewDistance))
					{
						if (hit.transform.CompareTag("Window"))
						{
							if (state == State.Attack)
								animator.SetBool("Attack", false);
							state = State.trace;
							agent.isStopped = false;
							patroling = false;
							agent.speed = runSpeed;
							wasTracing = true; ;
							animator.SetBool("FindTarget", true);
							return;
						}
					}

					if (Vector3.Distance(PlayerTransform.position, transform.position) <= attackDistanceMax)
					{
						if (state == State.Attack) return;
						animator.SetBool("Attack", true);
						state = State.Attack;
						//runSpeed = 0f;
						agent.isStopped = true;
						patroling = false;
						return;
					}
					if (state == State.Attack)
						animator.SetBool("Attack", false);

					state = State.trace;
					agent.isStopped = false;
					patroling = false;
					agent.speed = runSpeed;
					wasTracing = true;
					animator.SetBool("FindTarget", true);
					return;
				}
			}
		}

		if (state == State.trace)
		{
			animator.SetBool("FindTarget", true);
			SavePosition = PlayerTransform.position;
			agent.SetDestination(SavePosition);
			state = State.patrol;
		}
		if (state == State.Attack)
		{
			animator.SetBool("Attack", false);
			animator.SetBool("FindTarget", true);
			wasTracing = true;
			agent.isStopped = false;

			SavePosition = PlayerTransform.position;
			agent.SetDestination(SavePosition);
			state = State.patrol;
		}
	}

	private void PatrolMove()
	{

		//agent.isStopped = false;
		if (agent.velocity == Vector3.zero)
		{
			if(EnemyWayPoint.Length == 1)
			{
				agent.SetDestination(EnemyWayPoint[0].position);
				return;
			}

			agent.SetDestination(EnemyWayPoint[wayCount++].position);

			if (wayCount >= EnemyWayPoint.Length)
				wayCount = 0;
		}
	}

	private void DoAttack()
	{
		agent.isStopped = true;
		var lookRotation = Quaternion.LookRotation(PlayerTransform.transform.position - transform.position);
		var targetAngleY = lookRotation.eulerAngles.y;

		float turnVelocity = 0.5f;
		float turnTime = 0.1f;
		transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnVelocity, turnTime);

		if (attackDelay <= 0f)
		{
			animator.SetBool("AttackBigin", true);
			Invoke("Shoot", 0.15f);
			attackDelay = 2f;
		}
		else
		{
			attackDelay -= Time.deltaTime;
			if (animator.GetBool("AttackBigin"))
				animator.SetBool("AttackBigin", false);
		}
	}

	public void StateChangeToTrace()
	{
		if (state == State.patrol)
		{
			state = State.trace;
			agent.isStopped = false;
			patroling = false;
			agent.speed = runSpeed;
			wasTracing = true; ;
			animator.SetBool("FindTarget", true);
		}
	}

	public void Stoping()
	{
		CancelInvoke();
		isStop = true;
		agent.isStopped = true;
		patroling = false;
		animator.SetFloat("Speed", 0);
		animator.SetBool("FindTarget", false);
		animator.SetBool("Attack", false);

		--wayCount;
		if (wayCount <= 0) wayCount = 0;
	}

	private void StopControl()
	{
		if (isStop == false) return;
		if (stopTime <= 0f)
		{
			InvokeRepeating("PatrolMove", 0f, 2f);
			agent.isStopped = false;
			isStop = false;
			stopTime = 3f;
			return;
		}
		else
		{
			stopTime -= Time.deltaTime;
		}
	}

	private void Shoot()
	{
		if (flashPrefab)
		{

			GameObject tempFlash;
			tempFlash = Instantiate(flashPrefab, flashTransform.position, flashTransform.rotation);

			attackDelay = 2f;
			Destroy(tempFlash, destroyTimer);
		}
		if (!bulletPrefab) return;

		GameObject bullet = Instantiate(bulletPrefab, flashTransform.position, flashTransform.rotation);
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		var leftRayRotation = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
		var leftRayDirection = leftRayRotation * transform.forward;

		Handles.color = new Color(1f, 1f, 1f, 0.2f);
		Handles.DrawSolidArc(transform.position, Vector3.up, leftRayDirection, viewAngle, viewDistance);

		if (attackArea != null)
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			Gizmos.DrawSphere(attackArea.position, attackRadius);
		}
	}
#endif
}
