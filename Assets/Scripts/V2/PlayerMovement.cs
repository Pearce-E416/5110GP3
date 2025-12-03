using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
	public GameObject[] GravitySourceList;
    public GameObject GravitySource;
	public float gravityStrength = 0f;

	private Vector3 gravityDirection = Vector3.zero;
	private float gravityRange = 2.5f;

	private float moveStrength = 8f;
	private float maxSpeed = 3f;
	private float jumpStrength = 200f;
	private float brakeLinearDamping = 0.9f;
	private float brakeAngularDamping = 0.3f;
	private float jumpTime = 0f;
	private float jumpBrakeCooldown = 0.2f;
	private float gotoStrength = 5f;
	private float pullStrength = 1000f;

	private bool isGrounded = false;

	public GameObject connectedStar;
	public GameObject snakeHead;
	public LineRenderer snakeLine;

	Rigidbody2D rb;

	PlayerInput playerInput;
	InputAction moveAction;
    InputAction jumpAction;
	InputAction brakeAction;
	InputAction pullAction;
	InputAction gotoAction;
	InputAction connectAction;

	private bool connectActionPressed = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        rb = GetComponent<Rigidbody2D>();

		playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");
        brakeAction = playerInput.actions.FindAction("Brake");
		pullAction = playerInput.actions.FindAction("Pull");
		gotoAction = playerInput.actions.FindAction("Goto");
		connectAction = playerInput.actions.FindAction("Connect");

		snakeLine = gameObject.AddComponent<LineRenderer>();
		snakeLine.enabled = false;
	}

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

		SetGravity();

		//MovePlayer();

		DrawLines();

		//HandleConnections();
	}

	private void FixedUpdate()
	{
		MovePlayer();

		HandleConnections();
	}

	void MovePlayer()
    {
		if (isGrounded) {
			float moveValue = moveAction.ReadValue<float>();
			Vector3 moveVector = transform.right * moveValue * moveStrength;
			rb.AddForce(moveVector);

			if (jumpAction.ReadValue<float>() != 0 && Time.time - jumpTime > jumpBrakeCooldown)
			{
				Vector3 jumpVector = -gravityDirection * jumpStrength;
				rb.AddForce(jumpVector);
				jumpTime = Time.time;
			}
			

			float horizontalSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, Vector3.Cross(gravityDirection, transform.forward)));
			if (horizontalSpeed > maxSpeed)
			{
				rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
			}
		}
		else
		{
			if (jumpAction.ReadValue<float>() != 0 && Time.time - jumpTime > jumpBrakeCooldown)
			{
				rb.linearDamping = brakeLinearDamping;
				rb.angularDamping = brakeAngularDamping;
			}
			else
			{
				rb.linearDamping = 0f;
				rb.linearDamping = 0.05f;
			}
		}
	}

    bool CheckGrounded()
    {
		isGrounded = GetComponent<Collider2D>().IsTouchingLayers(LayerMask.GetMask("Star"));
		return isGrounded;
	}

    void SetGravity()
    {
		GravitySource = null;

		float minDistance = Mathf.Infinity;
		foreach (GameObject source in GravitySourceList)
		{
			float distanceToSource = Vector3.Distance(transform.position, source.transform.position);
			if (distanceToSource < gravityRange && distanceToSource < minDistance)
			{
				GravitySource = source;
				minDistance = distanceToSource;
			}
		}

		if (GravitySource != null)
		{
			gravityDirection = (GravitySource.transform.position - transform.position).normalized;
			gravityStrength = GravitySource.GetComponent<StarV2>().gravityStrength;

			if (gravityDirection != Vector3.zero && gravityStrength != 0f)
			{
				Physics2D.gravity = gravityDirection * gravityStrength / Mathf.Pow(Vector3.Distance(transform.position, GravitySource.transform.position), 2);
				transform.rotation = Quaternion.LookRotation(Vector3.forward, -gravityDirection);
			}

		}
		else
		{
			gravityDirection = Vector3.zero;
			gravityStrength = 0f;
			Physics2D.gravity = Vector3.zero;
		}
		
	}

	void DrawLines()
	{
		if (snakeHead == null && connectedStar == null)
		{
			snakeLine.enabled = false;
			return;
		}

		if (snakeLine.enabled)
		{
			snakeLine.SetPosition(0, transform.position);
			if (connectedStar != null)
				snakeLine.SetPosition(1, connectedStar.transform.position);
			else if (snakeHead != null)
				snakeLine.SetPosition(1, snakeHead.transform.position);
		}
	}
	void HandleConnections()
	{
		if (connectedStar != null)
		{
			if (pullAction.ReadValue<float>() != 0)
			{
				StarV2 s = connectedStar.GetComponent<StarV2>();
				if (s != null && !s.isLocked)
				{
					Rigidbody2D rbStar = connectedStar.GetComponent<Rigidbody2D>();
					if (rbStar != null)
					{
						rbStar.AddForce((transform.position - connectedStar.transform.position).normalized * pullStrength);
					}
				}
			}

			if (gotoAction.ReadValue<float>() != 0)
			{
				rb.AddForce((connectedStar.transform.position - transform.position).normalized * gotoStrength);
			}

			if (connectAction.ReadValue<float>() != 0 && isGrounded && !connectActionPressed)
			{
				connectActionPressed = true;
				StarV2 s1 = connectedStar.GetComponent<StarV2>();
				StarV2 s2 = GravitySource.GetComponent<StarV2>();
				if (s1 != null && s2 != null && s1.isLocked && s2.isLocked)
				{
					GameObject d1 = s1.destination;
					GameObject d2 = s2.destination;
					StarDestination sd1 = d1.GetComponent<StarDestination>();
					if (sd1 != null && d2 != null && sd1.AddConnection(d2))
					{
						snakeLine.enabled = false;
						FindFirstObjectByType<WinCheck>().PerformCheck();
					}
				}
			}

			if (connectAction.ReadValue<float>() == 0)
			{
				connectActionPressed = false;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position, transform.position + gravityDirection * 2f);
	}
}
