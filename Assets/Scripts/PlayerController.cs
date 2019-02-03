using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2f;
	[SerializeField] private float playerHorizontalSpeed;
	[SerializeField] private float gravity = 1;
	[SerializeField] private float jumpSpeed = 3.0f;
	[SerializeField] private float rotationSpeed = 5.0f;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float groundRadius;
	[SerializeField] private LayerMask layerGround;
	[SerializeField] private GameObject mainCamera;

	enum CardinalDirection
	{
		South,
		East,
		North,
		West
	}

	private CardinalDirection _gravityDirection;

	private const float NORMAL_GRAVITY = 9.81f;
	private Rigidbody2D _myRigidbody;
	private bool _isGrounded;
	private bool _isAlive = true;
	private bool _canTurn = true;
	private float _horizontalInput;
	private bool _isPressingJump;
	private bool _isPressingLeft;
	private bool _isPressingRight;

	private void Start()
	{
		gravity *= NORMAL_GRAVITY;
		_myRigidbody = GetComponent<Rigidbody2D>();

		//setup correctly the direction the player is positioned at setup
		float initRot = transform.eulerAngles.z;
		Debug.Log(initRot);
		if (Mathf.Abs(initRot) < 45)
		{
			_gravityDirection = CardinalDirection.South;
		}
		else
		{
			if (Mathf.Abs(initRot % 180) > 135)
			{
				_gravityDirection = CardinalDirection.North;
			}
			else
			{
				if (initRot > 180)
				{
					_gravityDirection = CardinalDirection.West;
				}
				else
				{
					_gravityDirection = CardinalDirection.East;
				}
			}
		}
	}

	private void Update()
	{
		_isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, layerGround);
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
			if (Input.GetButtonDown("TurnLeft"))
			{
				TurnTo(_gravityDirection + (_gravityDirection == CardinalDirection.West ? -3 : 1));
			}

			else if (Input.GetButtonDown("TurnRight"))
			{
				TurnTo(_gravityDirection - (_gravityDirection == CardinalDirection.South ? -3 : 1));
			}
		}
	}

	private void FixedUpdate()
	{
		if (_canTurn)
		{
			_myRigidbody.AddForce(-transform.up.normalized * gravity);
			_myRigidbody.velocity = transform.right.normalized * playerHorizontalSpeed * _horizontalInput +
			                        Vector3.Project(_myRigidbody.velocity, transform.up);
			if (_isPressingJump)
			{
				_myRigidbody.velocity = transform.up.normalized * jumpSpeed +
				                        Vector3.Project(_myRigidbody.velocity, transform.right);
				_isPressingJump = false;
			}

			if (Vector3.Project(_myRigidbody.velocity, transform.up).magnitude < 0)
			{
				_myRigidbody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y * (fallMultiplier - 1);
			}
			else if (_myRigidbody.velocity.y > 0 && !Input.GetButton("Jump"))
			{
				_myRigidbody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1);
			}
		}
	}

	private void TurnTo(CardinalDirection direction)
	{
		_canTurn = false;
		_myRigidbody.velocity = Vector2.zero;
		StartCoroutine(TurnCameraAndPlayer(rotationSpeed));
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
			mainCamera.transform.eulerAngles = (Vector3.right * cameraRotation.x + Vector3.up * cameraRotation.y +
			                                    Vector3.forward * (Mathf.Lerp(initRotCam,
				                                    (float) _gravityDirection * 90, timer / time)));
			Vector3 playerRotation = transform.eulerAngles;
			transform.eulerAngles = (Vector3.right * playerRotation.x + Vector3.up * playerRotation.y +
			                         Vector3.forward * (Mathf.Lerp(initRotPlayer,
				                         (float) _gravityDirection * 90, timer / time)));
			timer += Time.deltaTime;
			yield return null;
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