using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Drone : MonoBehaviour
{
	private enum State { patrol, trace }
	private State state = State.patrol;

	private bool isStop = false;
	private float stopTime = 3f;

	[SerializeField] float viewAngle = 60f;
	[SerializeField] float viewDistance = 5f;
	[SerializeField] float callDistance = 20f;

	[SerializeField] float walkSpeed = 2.0f;
	[SerializeField] float runSpeed = 5.0f;

	[SerializeField] Transform PlayerTransform;
	private Vector3 SavePosition;

	[SerializeField] Transform eyetransform;
	[SerializeField] LayerMask targetMask;
	[SerializeField] LayerMask robotMask;

	private NavMeshAgent agent;
	[SerializeField] Transform[] EnemyWayPoint;
	private int wayCount = 0;

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

		stopTime = 3f;

		agent = GetComponent<NavMeshAgent>();
		InvokeRepeating("PatrolMove", 0f, 2f);
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
			viewDistance += (float)UIManager.degreeOfRisk * 1f;
			viewAngle += (float)UIManager.degreeOfRisk * 4f;
		}

		Sight();
	}

	private void FixedUpdate()
	{
		if (UIManager.option)
		{
			agent.isStopped = true;
			changeOption = true;
			return;
		}

		if (changeOption)
		{
			agent.isStopped = false;
		}

		if (isStop) return;
		switch (state)
		{
			case State.patrol:
				if (patroling) break;
				agent.speed = walkSpeed;
				if (wasTracing)
				{
					if (agent.velocity == Vector3.zero)
					{
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
				InvokeRepeating("PatrolMove", 0f, 2f);
				patroling = true;
				break;
			case State.trace:
				agent.SetDestination(PlayerTransform.position);
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

			if (Physics.Raycast(eyetransform.position, targetDirection, out hit, viewDistance))
			{
				if (hit.transform.name == "Player")
				{
					if (state == State.patrol) CancelInvoke();

					state = State.trace;
					patroling = false;
					agent.speed = runSpeed;
					wasTracing = true; ;

					CallRobot();
					return;
				}
			}
			if (Physics.Raycast(eyetransform.position, targetfoot, out hit, viewDistance))
			{
				if (hit.transform.name == "Player")
				{
					if (state == State.patrol) CancelInvoke();

					state = State.trace;
					patroling = false;
					agent.speed = runSpeed;
					wasTracing = true;

					CallRobot();
					return;
				}
			}
		}

		if (state == State.trace)
		{
			SavePosition = PlayerTransform.position;
			agent.SetDestination(SavePosition);
			state = State.patrol;
		}
	}

	private void PatrolMove()
	{
		if (agent.velocity == Vector3.zero)
		{
			if (EnemyWayPoint.Length == 1)
			{
				agent.SetDestination(EnemyWayPoint[0].position);
				return;
			}

			agent.SetDestination(EnemyWayPoint[wayCount++].position);

			if (wayCount >= EnemyWayPoint.Length)
				wayCount = 0;
		}
	}

	private void CallRobot()
	{
		Collider[] RobotColliders = Physics.OverlapSphere(eyetransform.position, callDistance, robotMask);

		for (int i = 0; i < RobotColliders.Length; ++i)
		{
			GameObject Enemy = RobotColliders[i].gameObject;

			if (Enemy.name == "Enemy")
			{
				Enemy.GetComponent<Enemy>().StateChangeToTrace();
			}
		}
	}
	public void Stoping()
	{
		CancelInvoke();
		isStop = true;
		agent.isStopped = true;
		patroling = false;

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

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		var leftRayRotation = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
		var leftRayDirection = leftRayRotation * eyetransform.forward;

		Handles.color = new Color(1f, 1f, 1f, 0.2f);
		Handles.DrawSolidArc(eyetransform.position, Vector3.up, leftRayDirection, viewAngle, viewDistance);
	}
#endif
}
