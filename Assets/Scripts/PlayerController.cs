using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2f;
	[SerializeField] private float playerHorizontalSpeed;

	[SerializeField] private float gravity = 1;
	[SerializeField] private float jumpSpeed;

	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask whatIsGround;

	[SerializeField] private GameObject mainCamera;
	private const float NORMAL_GRAVITY = 9.81f;
	private float groundRadius = 0.2f;
	private float _inputHorizontal;
	private Rigidbody2D _myRigidBody;
	private bool _isAlive = true;

	enum GravityDirection
	{
		NORTH,
		SOUTH,
		EAST,
		WEST
	}

	GravityDirection gravityDirection;

	bool turnLeft;
	bool turnRight;
	bool jump;
	bool canTurn;
	bool grounded;


	private void Start()
	{
		_myRigidBody = GetComponent<Rigidbody2D>();
		gravityDirection = GravityDirection.SOUTH;
		canTurn = true;
		gravity *= NORMAL_GRAVITY;
	}

	private void Update()
	{
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
		_inputHorizontal = Input.GetAxis("Horizontal");
		if (canTurn)
		{
			if (Input.GetButtonDown("TurnLeft"))
				turnLeft = true;
			if (Input.GetButtonDown("TurnRight"))
				turnRight = true;
			if (Input.GetButtonDown("Jump") && grounded)
				jump = true;
		}

		Debug.Log(jump);
	}

	private void FixedUpdate()
	{
		if (turnLeft)
		{
			gravityDirection = TurnLeft(gravityDirection);
			StartCoroutine("TurnLeftCoroutine");
			turnLeft = false;
		}

		if (turnRight)
		{
			gravityDirection = TurnRight(gravityDirection);
			StartCoroutine("TurnRightCoroutine");
			turnRight = false;
		}

		switch (gravityDirection)
		{
			case GravityDirection.SOUTH:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
				_myRigidBody.AddForce(new Vector2(0, -gravity));
				_myRigidBody.velocity = new Vector2(_inputHorizontal * playerHorizontalSpeed, _myRigidBody.velocity.y);
				if (jump && grounded)
				{
					_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, jumpSpeed);
					jump = false;
				}


				if (_myRigidBody.velocity.y < 0)
				{
					_myRigidBody.velocity += Vector2.up * -gravity * (fallMultiplier + 1) * Time.deltaTime;
				}
				else if (_myRigidBody.velocity.y > 0 && !Input.GetButton("Jump"))
				{
					_myRigidBody.velocity += Vector2.up * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
				}

				break;
			case GravityDirection.NORTH:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
				_myRigidBody.AddForce(new Vector2(0, gravity));
				_myRigidBody.velocity = new Vector2(-_inputHorizontal * playerHorizontalSpeed, _myRigidBody.velocity.y);
				if (jump && grounded)
				{
					_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, -jumpSpeed);
					jump = false;
				}


				if (_myRigidBody.velocity.y > 0)
				{
					_myRigidBody.velocity += Vector2.down * -gravity * (fallMultiplier + 1) * Time.deltaTime;
				}
				else if (_myRigidBody.velocity.y < 0 && !Input.GetButton("Jump"))
				{
					_myRigidBody.velocity += Vector2.down * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
				}

				break;
			case GravityDirection.WEST:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 270);
				_myRigidBody.AddForce(new Vector2(-gravity, 0));
				_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, -_inputHorizontal * playerHorizontalSpeed);
				if (jump && grounded)
				{
					_myRigidBody.velocity = new Vector2(jumpSpeed, _myRigidBody.velocity.y);
					jump = false;
				}

				if (_myRigidBody.velocity.x < 0)
				{
					_myRigidBody.velocity += Vector2.right * -gravity * (fallMultiplier + 1) * Time.deltaTime;
				}
				else if (_myRigidBody.velocity.x > 0 && !Input.GetButton("Jump"))
				{
					_myRigidBody.velocity += Vector2.right * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
				}

				break;
			case GravityDirection.EAST:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
				_myRigidBody.AddForce(new Vector2(gravity, 0));
				_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, _inputHorizontal * playerHorizontalSpeed);
				if (jump && grounded)
				{
					_myRigidBody.velocity = new Vector2(-jumpSpeed, _myRigidBody.velocity.y);
					jump = false;
				}

				if (_myRigidBody.velocity.x > 0)
				{
					_myRigidBody.velocity += Vector2.left * -gravity * (fallMultiplier + 1) * Time.deltaTime;
				}
				else if (_myRigidBody.velocity.x < 0 && !Input.GetButton("Jump"))
				{
					_myRigidBody.velocity += Vector2.left * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
				}

				break;
		}
	}

	GravityDirection TurnLeft(GravityDirection gravityDirection)
	{
		switch (gravityDirection)
		{
			case GravityDirection.SOUTH:
				gravityDirection = GravityDirection.EAST;
				break;
			case GravityDirection.NORTH:
				gravityDirection = GravityDirection.WEST;
				break;
			case GravityDirection.WEST:
				gravityDirection = GravityDirection.SOUTH;
				break;
			case GravityDirection.EAST:
				gravityDirection = GravityDirection.NORTH;
				break;
		}

		return gravityDirection;
	}

	GravityDirection TurnRight(GravityDirection gravityDirection)
	{
		switch (gravityDirection)
		{
			case GravityDirection.SOUTH:
				gravityDirection = GravityDirection.WEST;
				break;
			case GravityDirection.NORTH:
				gravityDirection = GravityDirection.EAST;
				break;
			case GravityDirection.WEST:
				gravityDirection = GravityDirection.NORTH;
				break;
			case GravityDirection.EAST:
				gravityDirection = GravityDirection.SOUTH;
				break;
		}

		return gravityDirection;
	}

	IEnumerator TurnLeftCoroutine()
	{
		canTurn = false;
		for (int i = 0; i < 90; i += 2)
		{
			mainCamera.transform.eulerAngles = new Vector3(0, 0, mainCamera.transform.eulerAngles.z + 2);
			yield return new WaitForSeconds(0.01f);
		}

		canTurn = true;
	}

	IEnumerator TurnRightCoroutine()
	{
		canTurn = false;
		for (int i = 0; i < 90; i += 2)
		{
			mainCamera.transform.eulerAngles = new Vector3(0, 0, mainCamera.transform.eulerAngles.z - 2);
			yield return new WaitForSeconds(0.01f);
		}

		canTurn = true;
	}


	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
		}
	}
}