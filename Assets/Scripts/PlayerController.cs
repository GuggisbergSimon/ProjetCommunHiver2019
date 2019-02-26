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
	[SerializeField] private bool noVomitMode = false;
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private int maxNumberGravityUse = 1;
	[SerializeField] private float maxFallingSpeed = 5.0f;

	public enum CardinalDirection
	{
		South,
		East,
		North,
		West
	}

	private List<GameObject> _interactives = new List<GameObject>();
	private CardinalDirection _previousGravityDirection;
	private CardinalDirection _actualGravityDirection;
	public CardinalDirection ActualGravityDirection => _actualGravityDirection;
	private Coroutine _rotatingCoroutine;
	private Rigidbody2D _myRigidBody;
	private bool _isGrounded;
	private bool _isAlive = true;
	private bool _canTurn = true;
	private bool _canMove = true;
	private Vector2 _inputs;
	private float _horizontalInput;
	private float _verticalInput;
	private bool _isPressingJump;
	private bool _isPressingRight;
	private bool _isPressingLeft;
	private int _numberGravityUseRemaining;
	private Vector3 previousVelocity;
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

	private void Start()
	{
		noVomitMode = GameManager.Instance.NoVomitModeEnabled;
	}

	private void Update()
	{
		_inputs = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxisRaw("Vertical");
		if (noVomitMode)
		{
			//cases when walking on ground/ceiling
			if ((int) _actualGravityDirection % 2 == 0)
			{
				_inputs *= Vector2.right * ((int) _actualGravityDirection > 1 ? -1 : 1) + Vector2.up;
			}
		}

		if (_canMove)
		{
			//handles horizontal input
			_horizontalInput = _inputs.x;
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
		if (_canTurn && _numberGravityUseRemaining > 0)
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

		//handles retry input
		if (Input.GetButtonDown("Retry"))
		{
			Die();
		}

		//code for interacting with _interactives
		if (_interactives.Count > 0 && Input.GetAxisRaw("Vertical") > 0 && _isGrounded)
		{
			GameObject closestToPlayer = _interactives[0];
			foreach (var item in _interactives)
			{
				if ((closestToPlayer.transform.position - transform.position).magnitude >
				    (item.transform.position - transform.position).magnitude)
				{
					closestToPlayer = item;
				}
			}

			ResetVelocityAndInput();
			closestToPlayer.GetComponent<Interactive>().Interact();
			return;
		}

		//handles zoom/dezoom of map
		if (_isGrounded && _inputs.y >= 0 && _inputs.y.CompareTo(_verticalInput) != 0)
		{
			_verticalInput = Input.GetAxisRaw("Vertical");
			bool isPressingUp = _verticalInput > 0;
			if (isPressingUp)
			{
				previousVelocity = _myRigidBody.velocity;
				_myRigidBody.velocity = Vector2.zero;
			}
			else
			{
				_myRigidBody.velocity = previousVelocity;
			}

			ToggleFreeze(!isPressingUp);
			GameManager.Instance.CameraManager.ToggleGlobalCamera(isPressingUp);
		}
	}

	private void ToggleFreeze(bool value)
	{
		_canMove = value;
		_canTurn = value;
		if (value)
		{
			ResetVelocityAndInput();
		}
	}

	private void ResetVelocityAndInput()
	{
		_myRigidBody.velocity = Vector2.zero;
		_horizontalInput = 0;
		_verticalInput = 0;
		_isPressingJump = false;
		_isPressingLeft = false;
		_isPressingRight = false;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Interactive") && !_interactives.Contains(other.gameObject))
		{
			_interactives.Add(other.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Interactive") && _interactives.Contains(other.gameObject))
		{
			_interactives.Remove(other.gameObject);
		}
	}

	private void FixedUpdate()
	{
		if (_canMove)
		{
			Vector3 projectionVelocityUp = Vector3.Project(_myRigidBody.velocity, transform.up);
			//add gravity force depending on direction
			_myRigidBody.AddForce(transform.up.normalized * gravityMultiplier * Physics2D.gravity.y);
			//moves the player depending on direction
			_myRigidBody.velocity = transform.right.normalized * playerHorizontalSpeed * _horizontalInput +
			                        projectionVelocityUp;
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
			else if (Vector3.Dot(projectionVelocityUp.normalized, transform.up.normalized) > 0 &&
			         !Input.GetButton("Jump"))
			{
				_myRigidBody.velocity +=
					(Vector2) transform.up.normalized * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
			}

			//applies maxSpeed
			if (projectionVelocityUp.magnitude > maxFallingSpeed &&
			    Vector3.Dot(projectionVelocityUp.normalized, transform.up.normalized) <
			    0)
			{
				_myRigidBody.velocity *=
					projectionVelocityUp.normalized.magnitude * maxFallingSpeed / projectionVelocityUp.magnitude;
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
			return true;
		}
		else
		{
			return false;
		}
	}

	public void ToggleTurningUse(bool value)
	{
		_canTurn = value;
	}

	public bool GetGrounded
	{
		get { return _isGrounded; }
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
				_myRigidBody.velocity = Vector2.zero;
				_myCollider.enabled = false;
			}

			_actualGravityDirection = direction;
			if (!noVomitMode)
			{
				GameManager.Instance.CameraManager.ChangeVCamByDirection(_actualGravityDirection);
			}

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
		_previousGravityDirection = _actualGravityDirection;
		_isPressingLeft = false;
		_isPressingRight = false;
		_canMove = true;
		_myCollider.enabled = true;
		_numberGravityUseRemaining--;
	}

	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
			_canMove = false;
			_canTurn = false;
			GameManager.Instance.LoadLevel(SceneManager.GetActiveScene().name, true, true);
		}
	}
}