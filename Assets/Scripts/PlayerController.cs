using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//todo rework boxcast to be more precise

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
	[SerializeField] private Vector2 sizeMaxCheckGround = new Vector2(0.5f, 0.1f);
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private int maxNumberGravityUse = 1;

	public enum CardinalDirection
	{
		South,
		East,
		North,
		West
	}

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
	private int _numberGravityUseRemaining;
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
	}

	private void Update()
	{
		_horizontalInput = Input.GetAxis("Horizontal");

		CheckGrounded();

		if (Input.GetButtonDown("Jump") && _isGrounded)
		{
			_isPressingJump = true;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			_isPressingJump = false;
		}

		if (_canTurn)
		{
			if (Input.GetButtonDown("TurnLeft") || Input.GetButtonDown("TurnRight"))
			{
				if (Input.GetButtonDown("TurnLeft"))
				{
					TurnTo(_actualGravityDirection - (_actualGravityDirection == CardinalDirection.South ? -3 : 1));
				}
				else
				{
					TurnTo(_actualGravityDirection + (_actualGravityDirection == CardinalDirection.West ? -3 : 1));
				}
			}
		}
	}


	private void FixedUpdate()
	{
		if (_canMove)
		{
			_myRigidBody.AddForce(transform.up.normalized * gravityMultiplier * Physics2D.gravity.y);
			_myRigidBody.velocity = transform.right.normalized * playerHorizontalSpeed * _horizontalInput +
									Vector3.Project(_myRigidBody.velocity, transform.up);
			if (_isPressingJump)
			{
				_myRigidBody.velocity = transform.up.normalized * jumpSpeed +
										Vector3.Project(_myRigidBody.velocity, transform.right);
				_isPressingJump = false;
			}

			if (Vector3.Dot(Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
					transform.up.normalized) < 0)
			{
				_myRigidBody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y *
										 (fallMultiplier - 1) * Time.deltaTime;
			}
			else if (Vector3.Dot(Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
						 transform.up.normalized) > 0 && !Input.GetButton("Jump"))
			{
				_myRigidBody.velocity +=
					(Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_isGrounded)
		{
			_numberGravityUseRemaining = maxNumberGravityUse;
			_canTurn = _numberGravityUseRemaining > 0;
		}
	}

	private void CheckGrounded()
	{
		_isGrounded = Physics2D.BoxCast(transform.position, sizeMaxCheckGround, 0,
			-transform.up, distMaxGroundCheck, layerGround);
	}

	private void TurnTo(CardinalDirection direction)
	{
		if (_canMove)
		{
			_canMove = false;
			_numberGravityUseRemaining--;
			_myRigidBody.velocity = Vector2.zero;
			_myCollider.enabled = false;
		}

		_actualGravityDirection = direction;
		GameManager.Instance.CameraManager.ChangeVCamByDirection(_actualGravityDirection);
		if (_rotatingCoroutine != null)
		{
			StopCoroutine(_rotatingCoroutine);
		}

		_rotatingCoroutine = StartCoroutine(TurnCameraAndPlayer(rotationTime));
	}

	private IEnumerator TurnCameraAndPlayer(float time)
	{
		float timer = 0.0f;
		float initRotPlayer = transform.eulerAngles.z;
		while (timer < time)
		{
			timer += Time.deltaTime;
			float angle = (float) _actualGravityDirection * 90.0f;
			_myRigidBody.rotation = (Mathf.LerpAngle(initRotPlayer, angle, timer / time));
			yield return null;
		}

		yield return new WaitForSeconds(timeBeforeGravityAgain);
		_canMove = true;
		_canTurn = _numberGravityUseRemaining > 0;
		_myCollider.enabled = true;
	}

	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
			GameManager.Instance.LoadLevel(SceneManager.GetActiveScene().name, true, true);
		}
	}
}