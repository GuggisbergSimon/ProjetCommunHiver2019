using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2.0f;
	[SerializeField] private float playerHorizontalSpeed = 5.0f;
	[SerializeField] private float gravityMultiplier = 1.0f;
	[SerializeField] private float jumpSpeed = 3.0f;
	[SerializeField] private float rotationTime = 1.75f;
	[SerializeField] private float timeBeforeGravityAgain = 0.2f;
	[SerializeField] private float distMaxGroundCheck = 0.1f;
	[SerializeField] private float radiusGroundCheck = 0.5f;
	[SerializeField] private LayerMask layerGround = 0;

	[SerializeField] private int maxNumberGravityUse = 1;

	//[SerializeField] private float powerGravityTimeScale = 0.1f;
	[SerializeField] private float maxFallingSpeed = 5.0f;

	public enum CardinalDirection
	{
		South,
		East,
		North,
		West
	}

	private CardinalDirection _previousGravityDirection;
	private CardinalDirection _actualGravityDirection;
	public CardinalDirection ActualGravityDirection => _actualGravityDirection;
	private Coroutine _rotatingCoroutine;
	private Rigidbody2D _myRigidBody;
	private bool _isGrounded;
	private bool _isAlive = true;
	private bool _canTurn = true;
	private bool _canMove = true;
	private float _horizontalInput;
	private bool _isPressingJump;
	private bool _isPressingRight;
	private bool _isPressingLeft;
	private int _numberGravityUseRemaining;

	public int NumberGravityUseRemaining => _numberGravityUseRemaining;
	private Collider2D _myCollider;

	private void Awake()
	{
		_myRigidBody = GetComponent<Rigidbody2D>();
		_myCollider = GetComponent<Collider2D>();
		_numberGravityUseRemaining = maxNumberGravityUse;

		//setup correctly the direction the player is positioned at setup
		float initRot = transform.eulerAngles.z;
		if (Mathf.Abs(initRot) < 45)
		{
			_actualGravityDirection = CardinalDirection.South;
		}
		else
		{
			if (Mathf.Abs(initRot % 180) > 135)
			{
				_actualGravityDirection = CardinalDirection.North;
			}
			else
			{
				if (initRot > 180)
				{
					_actualGravityDirection = CardinalDirection.West;
				}
				else
				{
					_actualGravityDirection = CardinalDirection.East;
				}
			}
		}

		_previousGravityDirection = _actualGravityDirection;
	}

	private void Update()
	{
		if (_canMove)
		{
			//handles horizontal input
			_horizontalInput = Input.GetAxis("Horizontal");
			CheckGrounded();
			//restores gravity power
			if (_isGrounded)
			{
				RestoreGravityPower();
			}
		}

		//handles jump input
		if (Input.GetButtonDown("Jump") && _isGrounded)
		{
			_isPressingJump = true;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			_isPressingJump = false;
		}

		//handles turn input
		if (_canTurn)
		{
			//a ternary operator is used in order to go from the top of the enumlist to the bottom and vice-versa
			if (Input.GetButtonDown("TurnLeft") && !_isPressingRight)
			{
				_isPressingLeft = true;
				TurnTo(_actualGravityDirection -
					   (_actualGravityDirection == CardinalDirection.South ? -3 : 1));
			}
			else if (Input.GetButtonDown("TurnRight") && !_isPressingLeft)
			{
				_isPressingRight = true;
				TurnTo(_actualGravityDirection +
					   (_actualGravityDirection == CardinalDirection.West ? -3 : 1));
			}
		}
	}

	private void FixedUpdate()
	{
		if (_canMove)
		{
			//add gravity force depending on direction
			_myRigidBody.AddForce(transform.up.normalized * gravityMultiplier * Physics2D.gravity.y);
			//moves the player depending on direction
			_myRigidBody.velocity = transform.right.normalized * playerHorizontalSpeed * _horizontalInput +
									Vector3.Project(_myRigidBody.velocity, transform.up);
			//executes a jump depending on direction
			if (_isPressingJump)
			{
				_myRigidBody.velocity = transform.up.normalized * jumpSpeed +
										Vector3.Project(_myRigidBody.velocity, transform.right);
				_isPressingJump = false;
			}

			//applies fallMultiplier
			if (Vector3.Dot(Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
					transform.up.normalized) < 0)
			{
				_myRigidBody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y *
										 (fallMultiplier - 1) * Time.deltaTime;
			}
			//applies lowJumpMultiplier
			else if (Vector3.Dot(Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
						 transform.up.normalized) > 0 && !Input.GetButton("Jump"))
			{
				_myRigidBody.velocity +=
					(Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
			}

			if (Vector3.Project(_myRigidBody.velocity, transform.up).magnitude > maxFallingSpeed && Vector3.Dot(
					Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
					transform.up.normalized) < 0)
			{
				//todo to perfect
				Vector3 projectionVelocity = Vector3.Project(_myRigidBody.velocity,transform.up);
				_myRigidBody.velocity /= projectionVelocity.magnitude / projectionVelocity.normalized.magnitude * maxFallingSpeed;
			}
		}
	}

	public void StopMoving()
	{
		_myRigidBody.velocity = Vector2.zero;
		_canMove = false;
	}

	public bool RestoreGravityPower()
	{
		if (_numberGravityUseRemaining < maxNumberGravityUse)
		{
			_numberGravityUseRemaining = maxNumberGravityUse;
			_canTurn = _numberGravityUseRemaining > 0;
			return true;
		}
		else
		{
			return false;
		}
	}

	//raycast to check if the player is grounded
	private void CheckGrounded()
	{
		_isGrounded = Physics2D.CircleCast(transform.position, radiusGroundCheck, -transform.up, distMaxGroundCheck,
			layerGround);
	}

	private void TurnTo(CardinalDirection direction)
	{
		//checks that the player can not go further than 180°
		if (_actualGravityDirection + ((int) _actualGravityDirection > 1 ? -2 : 2) != _previousGravityDirection)
		{
			//setup the first 90° turn
			if (_canMove)
			{
				_canMove = false;
				_numberGravityUseRemaining--;
				_myRigidBody.velocity = Vector2.zero;
				_myCollider.enabled = false;
				//GameManager.Instance.ChangeTimeScale(powerGravityTimeScale);
			}

			_actualGravityDirection = direction;
			GameManager.Instance.CameraManager.ChangeVCamByDirection(_actualGravityDirection);
			//checks if a coroutine is already running
			if (_rotatingCoroutine != null)
			{
				StopCoroutine(_rotatingCoroutine);
			}

			_rotatingCoroutine = StartCoroutine(TurnPlayer(rotationTime));
		}
	}

	//Coroutine which turns progressively the player to _actualGravityDirection
	private IEnumerator TurnPlayer(float time)
	{
		float timer = 0.0f;
		float initRotPlayer = transform.eulerAngles.z;
		while (timer < time)
		{
			timer += Time.deltaTime;
			_myRigidBody.rotation =
				(Mathf.LerpAngle(initRotPlayer, (float) _actualGravityDirection * 90.0f, timer / time));
			yield return null;
		}

		yield return new WaitForSeconds(timeBeforeGravityAgain);
		//restores player move after turning
		//GameManager.Instance.ChangeTimeScale(1.0f);
		_previousGravityDirection = _actualGravityDirection;
		_isPressingLeft = false;
		_isPressingRight = false;
		_canMove = true;
		_canTurn = _numberGravityUseRemaining > 0;
		_myCollider.enabled = true;
	}

	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
			_canMove = false;
			GameManager.Instance.LoadLevel(SceneManager.GetActiveScene().name, true, true);
		}
	}
}