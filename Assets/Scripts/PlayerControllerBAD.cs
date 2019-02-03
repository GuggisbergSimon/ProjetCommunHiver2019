using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;

public class PlayerControllerBAD : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2f;
	[SerializeField] private float playerHorizontalSpeed;
	[SerializeField] private float gravity = 1;
	[SerializeField] private float jumpSpeed;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask layerGround;
	[SerializeField] private GameObject mainCamera;

	enum CardinalDirection
	{
		North,
		South,
		East,
		West
	}

	private const float NORMAL_GRAVITY = 9.81f;
	private float groundRadius = 0.2f;
	private float _inputHorizontal;
	private Rigidbody2D _myRigidBody;
	private bool _isAlive = true;
	private CardinalDirection _gravityDirection;
	private bool _pressingTurnLeft;
	private bool _pressingTurnRight;
	private bool _pressingJump;
	private bool _canEnterInput;
	private bool _canTurn = true;
	private bool _isGrounded;

	private void Start()
	{
		_myRigidBody = GetComponent<Rigidbody2D>();
		gravity *= NORMAL_GRAVITY;

		//setup correctly the direction the player is positioned at setup
		float initRot = transform.rotation.eulerAngles.z % 180;
		if (Mathf.Abs(initRot) < 45)
		{
			_gravityDirection = CardinalDirection.South;
		}
		else
		{
			if (Mathf.Abs(initRot) > 135)
			{
				_gravityDirection = CardinalDirection.North;
			}
			else
			{
				if (initRot > 0)
				{
					_gravityDirection = CardinalDirection.East;
				}
				else
				{
					_gravityDirection = CardinalDirection.West;
				}
			}
		}
	}

	private void Update()
	{
		//check if player is grounded
		_isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, layerGround);
		
		//check for all inputs
		if (_canEnterInput)
		{
			_inputHorizontal = Input.GetAxis("Horizontal");
			if (Input.GetButtonDown("Jump") && _isGrounded)
			{
				_pressingJump = true;
			}

			if (_canTurn)
			{
				if (Input.GetButtonDown("TurnLeft"))
				{
					_pressingTurnLeft = true;
				}

				if (Input.GetButtonDown("TurnRight"))
				{
					_pressingTurnRight = true;
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (_pressingTurnLeft)
		{
			_gravityDirection = TurnLeft(_gravityDirection);
			StartCoroutine(TurnLeftCoroutine());
			_pressingTurnLeft = false;
		}

		if (_pressingTurnRight)
		{
			_gravityDirection = TurnRight(_gravityDirection);
			StartCoroutine(TurnRightCoroutine());
			_pressingTurnRight = false;
		}

		switch (_gravityDirection)
		{
			case CardinalDirection.South:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
				_myRigidBody.AddForce(new Vector2(0, -gravity));
				_myRigidBody.velocity = new Vector2(_inputHorizontal * playerHorizontalSpeed, _myRigidBody.velocity.y);
				if (_pressingJump && _isGrounded)
				{
					_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, jumpSpeed);
					_pressingJump = false;
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
			case CardinalDirection.North:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
				_myRigidBody.AddForce(new Vector2(0, gravity));
				_myRigidBody.velocity = new Vector2(-_inputHorizontal * playerHorizontalSpeed, _myRigidBody.velocity.y);
				if (_pressingJump && _isGrounded)
				{
					_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, -jumpSpeed);
					_pressingJump = false;
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
			case CardinalDirection.West:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 270);
				_myRigidBody.AddForce(new Vector2(-gravity, 0));
				_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, -_inputHorizontal * playerHorizontalSpeed);
				if (_pressingJump && _isGrounded)
				{
					_myRigidBody.velocity = new Vector2(jumpSpeed, _myRigidBody.velocity.y);
					_pressingJump = false;
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
			case CardinalDirection.East:
				gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
				_myRigidBody.AddForce(new Vector2(gravity, 0));
				_myRigidBody.velocity = new Vector2(_myRigidBody.velocity.x, _inputHorizontal * playerHorizontalSpeed);
				if (_pressingJump && _isGrounded)
				{
					_myRigidBody.velocity = new Vector2(-jumpSpeed, _myRigidBody.velocity.y);
					_pressingJump = false;
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

	CardinalDirection TurnLeft(CardinalDirection cardinalDirection)
	{
		switch (cardinalDirection)
		{
			case CardinalDirection.South:
				cardinalDirection = CardinalDirection.East;
				break;
			case CardinalDirection.North:
				cardinalDirection = CardinalDirection.West;
				break;
			case CardinalDirection.West:
				cardinalDirection = CardinalDirection.South;
				break;
			case CardinalDirection.East:
				cardinalDirection = CardinalDirection.North;
				break;
		}

		return cardinalDirection;
	}

	CardinalDirection TurnRight(CardinalDirection cardinalDirection)
	{
		switch (cardinalDirection)
		{
			case CardinalDirection.South:
				cardinalDirection = CardinalDirection.West;
				break;
			case CardinalDirection.North:
				cardinalDirection = CardinalDirection.East;
				break;
			case CardinalDirection.West:
				cardinalDirection = CardinalDirection.North;
				break;
			case CardinalDirection.East:
				cardinalDirection = CardinalDirection.South;
				break;
		}

		return cardinalDirection;
	}

	private void Turn(CardinalDirection direction)
	{
		//if is turning -->stopturningcoroutine
		//then start coroutine turning to direction with precise speed in given time
	}

	IEnumerator TurnLeftCoroutine()
	{
		_canTurn = false;
		for (int i = 0; i < 90; i += 2)
		{
			mainCamera.transform.eulerAngles = new Vector3(0, 0, mainCamera.transform.eulerAngles.z + 2);
			yield return new WaitForSeconds(0.01f);
		}

		_canTurn = true;
	}

	IEnumerator TurnRightCoroutine()
	{
		_canTurn = false;
		for (int i = 0; i < 90; i += 2)
		{
			mainCamera.transform.eulerAngles = new Vector3(0, 0, mainCamera.transform.eulerAngles.z - 2);
			yield return new WaitForSeconds(0.01f);
		}

		_canTurn = true;
	}


	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
		}
	}
}