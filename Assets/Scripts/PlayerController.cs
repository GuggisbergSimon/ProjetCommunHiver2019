using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//todo player 180° by double tap in same frame then again is causing problem (maybe?)
//todo rework boxcast to be more precise

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2.0f;
	[SerializeField] private float playerHorizontalSpeed = 5.0f;
	[SerializeField] private float gravityMultiplier = 1.0f;
	[SerializeField] private float jumpSpeed = 3.0f;
	[SerializeField] private float rotationTime = 1.75f;
	[SerializeField] private float distMaxGroundCheck = 0.1f;
	[SerializeField] private Vector2 sizeMaxCheckGround = new Vector2(0.5f, 0.1f);
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private float inputBufferTime = 0.1f;
	[SerializeField] private int maxNumberGravityUse = 1;

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
	private Rigidbody2D _myRigidbody;
	private bool _isGrounded;
	private bool _isAlive = true;
	private bool _canTurn = true;
	private float _horizontalInput;
	private bool _isPressingJump;
	private bool _isPressingLeft;
	private bool _isPressingRight;
	private int _numberGravityUseRemaining;
	private Collider2D _myCollider;

	private void Awake()
	{
		_myRigidbody = GetComponent<Rigidbody2D>();
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
		_horizontalInput = Input.GetAxis("Horizontal");

		CheckGrounded();
		/*bool isGroundedPastValue = _isGrounded;

		_isGrounded = Physics2D.BoxCast(transform.position, sizeMaxCheckGround, 0, -transform.up, distMaxGroundCheck,
			layerGround);
		if (_isGrounded && !isGroundedPastValue)
		{
			_numberGravityUseRemaining = maxNumberGravityUse;
		}*/

		if (Input.GetButtonDown("Jump") && _isGrounded)
		{
			_isPressingJump = true;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			_isPressingJump = false;
		}

		if ((Input.GetButtonDown("TurnLeft") && Input.GetButtonDown("TurnRight") && _numberGravityUseRemaining > 0) ||
			(_isPressingLeft && Input.GetButtonDown("TurnRight")) ||
			(_isPressingRight && Input.GetButtonDown("TurnLeft")))
		{
			if (!_isPressingLeft && !_isPressingRight)
			{
				_numberGravityUseRemaining--;
			}

			_isPressingLeft = true;
			_isPressingRight = true;
			TurnTo(_previousGravityDirection + ((int) _previousGravityDirection < 2 ? 2 : -2));
			StartCoroutine(ResetPressingTurn(0));
		}

		if (_canTurn && _numberGravityUseRemaining > 0)
		{
			if (Input.GetButtonDown("TurnLeft"))
			{
				_numberGravityUseRemaining--;
				_isPressingLeft = true;
				TurnTo(_actualGravityDirection - (_actualGravityDirection == CardinalDirection.South ? -3 : 1));
				StartCoroutine(ResetPressingTurn(inputBufferTime));
			}

			else if (Input.GetButtonDown("TurnRight"))
			{
				_numberGravityUseRemaining--;
				_isPressingRight = true;
				TurnTo(_actualGravityDirection + (_actualGravityDirection == CardinalDirection.West ? -3 : 1));
				StartCoroutine(ResetPressingTurn(inputBufferTime));
			}
		}
	}


	private void FixedUpdate()
	{
		if (_canTurn)
		{
			_myRigidbody.AddForce(transform.up.normalized * gravityMultiplier * Physics2D.gravity.y);
			_myRigidbody.velocity = transform.right.normalized * playerHorizontalSpeed * _horizontalInput +
									Vector3.Project(_myRigidbody.velocity, transform.up);
			if (_isPressingJump)
			{
				_myRigidbody.velocity = transform.up.normalized * jumpSpeed +
										Vector3.Project(_myRigidbody.velocity, transform.right);
				_isPressingJump = false;
			}

			if (Vector3.Dot(Vector3.Project(_myRigidbody.velocity, transform.up).normalized,
					transform.up.normalized) < 0)
			{
				_myRigidbody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y *
										 (fallMultiplier - 1) * Time.deltaTime;
			}
			else if (Vector3.Dot(Vector3.Project(_myRigidbody.velocity, transform.up).normalized,
						 transform.up.normalized) > 0 && !Input.GetButton("Jump"))
			{
				_myRigidbody.velocity +=
					(Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (_isGrounded)
		{
			_numberGravityUseRemaining = maxNumberGravityUse;
		}
	}

	private void CheckGrounded()
	{
		Debug.Log(transform.up.ToString());
		_isGrounded = Physics2D.BoxCast(transform.position, sizeMaxCheckGround, 0,
			-transform.up, distMaxGroundCheck, layerGround);
	}

	private IEnumerator ResetPressingTurn(float timeToWait)
	{
		yield return new WaitForSeconds(timeToWait);
		_isPressingRight = false;
		_isPressingLeft = false;
	}

	private void TurnTo(CardinalDirection direction)
	{
		_canTurn = false;
		_myRigidbody.velocity = Vector2.zero;
		_actualGravityDirection = direction;
		_myCollider.enabled = false;
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
			//Vector3 playerRotation = transform.eulerAngles;
			_myRigidbody.rotation = (Mathf.LerpAngle(initRotPlayer, angle, timer / time));
			//transform.eulerAngles = (Vector3.right * playerRotation.x + Vector3.up * playerRotation.y +
			//						 Vector3.forward * (Mathf.LerpAngle(initRotPlayer, angle, timer / time)));
			yield return null;
		}

		_previousGravityDirection = _actualGravityDirection;
		_canTurn = true;
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