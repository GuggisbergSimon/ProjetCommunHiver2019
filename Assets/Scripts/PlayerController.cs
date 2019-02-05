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

	[SerializeField] private float rotationSpeed = 5.0f;

	//[SerializeField] private float fallingSpeedLimit = 30.0f;
	[SerializeField] private Transform groundCheck = null;
	[SerializeField] private float groundRadius = 0.1f;
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private GameObject mainCamera = null;
	[SerializeField] private float inputBufferTime = 0.1f;

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

	private void Awake()
	{
		_myRigidbody = GetComponent<Rigidbody2D>();

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
		_isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, layerGround);
		if (Input.GetButtonDown("Jump") && _isGrounded)
		{
			_isPressingJump = true;
		}
		else if (Input.GetButtonUp("Jump"))
		{
			_isPressingJump = false;
		}

		if ((Input.GetButtonDown("TurnLeft") && Input.GetButtonDown("TurnRight")) ||
		    (_isPressingLeft && Input.GetButtonDown("TurnRight")) ||
		    (_isPressingRight && Input.GetButtonDown("TurnLeft")))
		{
			_isPressingLeft = true;
			_isPressingRight = true;
			TurnTo(_previousGravityDirection + ((int) _previousGravityDirection < 2 ? 2 : -2));
			StartCoroutine(ResetPressingTurn(0));
		}

		if (_canTurn)
		{
			if (Input.GetButtonDown("TurnLeft"))
			{
				_isPressingLeft = true;
				TurnTo(_actualGravityDirection - (_actualGravityDirection == CardinalDirection.South ? -3 : 1));
				StartCoroutine(ResetPressingTurn(inputBufferTime));
			}

			else if (Input.GetButtonDown("TurnRight"))
			{
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
		GameManager.Instance.CameraManager.ChangeVCamByDirection(_actualGravityDirection);
		if (_rotatingCoroutine != null)
		{
			StopCoroutine(_rotatingCoroutine);
		}

		_rotatingCoroutine = StartCoroutine(TurnCameraAndPlayer(rotationSpeed));
	}

	private IEnumerator TurnCameraAndPlayer(float speedTurn)
	{
		float time = (90.0f / speedTurn);
		float timer = 0.0f;
		float initRotCam = mainCamera.transform.eulerAngles.z;
		float initRotPlayer = transform.eulerAngles.z;

		while (timer < time)
		{
			Vector3 cameraRotation = mainCamera.transform.eulerAngles;
			float angle = (float) _actualGravityDirection * 90;
			//mainCamera.transform.eulerAngles = (Vector3.right * cameraRotation.x + Vector3.up * cameraRotation.y +
			//                                    Vector3.forward * (Mathf.LerpAngle(initRotCam, angle, timer / time)));
			Vector3 playerRotation = transform.eulerAngles;
			transform.eulerAngles = (Vector3.right * playerRotation.x + Vector3.up * playerRotation.y +
			                         Vector3.forward * (Mathf.LerpAngle(initRotPlayer, angle, timer / time)));
			timer += Time.deltaTime;
			yield return null;
		}

		_previousGravityDirection = _actualGravityDirection;
		_canTurn = true;
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