using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float fallMultiplier = 2.5f;
	[SerializeField] private float lowJumpMultiplier = 2.0f;
	[SerializeField] private float playerHorizontalSpeed = 5.0f;
	[SerializeField] private float gravityMultiplier = 1.0f;
	[SerializeField] private float jumpSpeed = 3.0f;
	[SerializeField] private float rotationSpeed = 5.0f;
	[SerializeField] private Transform groundCheck = null;
	[SerializeField] private float groundRadius = 0.2f;
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private GameObject mainCamera = null;

	enum CardinalDirection
	{
		South,
		East,
		North,
		West
	}

	private CardinalDirection _gravityDirection;
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
		_myRigidbody = GetComponent<Rigidbody2D>();

		//setup correctly the direction the player is positioned at setup
		float initRot = transform.eulerAngles.z;
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

		if (_canTurn)
		{
			if (Input.GetButtonDown("TurnLeft"))
			{
				TurnTo(_gravityDirection - (_gravityDirection == CardinalDirection.South ? -3 : 1));
			}

			else if (Input.GetButtonDown("TurnRight"))
			{
				TurnTo(_gravityDirection + (_gravityDirection == CardinalDirection.North ? -3 : 1));
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
				_myRigidbody.velocity += (Vector2) transform.up.normalized * Physics2D.gravity.y * (fallMultiplier - 1);
			}
			else if (Vector3.Dot(Vector3.Project(_myRigidbody.velocity, transform.up).normalized,
				         transform.up.normalized) > 0 && !Input.GetButton("Jump"))
			{
				_myRigidbody.velocity +=
					(Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1);
			}
		}
	}

	private void TurnTo(CardinalDirection direction)
	{
		_canTurn = false;
		_myRigidbody.velocity = Vector2.zero;
		_gravityDirection = direction;
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
			float angle = (float) _gravityDirection * 90;
			mainCamera.transform.eulerAngles = (Vector3.right * cameraRotation.x + Vector3.up * cameraRotation.y +
			                                    Vector3.forward * (Mathf.LerpAngle(initRotCam, angle, timer / time)));
			Vector3 playerRotation = transform.eulerAngles;
			transform.eulerAngles = (Vector3.right * playerRotation.x + Vector3.up * playerRotation.y +
			                         Vector3.forward * (Mathf.LerpAngle(initRotPlayer, angle, timer / time)));
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