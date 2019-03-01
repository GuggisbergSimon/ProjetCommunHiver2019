using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
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
	[SerializeField] private float maxFallingSpeed = 5.0f;
	[SerializeField] private float deadZoneVertical = 0.5f;
	[SerializeField] private AudioClip gravityBackSound = null;
	[SerializeField] private AudioClip gravityNoUseSound = null;
	[SerializeField] private AudioClip gravityUseSound = null;
	[SerializeField] private AudioClip[] jumpSounds = null;
	[SerializeField] private AudioClip stepSound = null;
	[SerializeField] private AudioClip deathSound = null;
	[SerializeField] private float gravityTimeScale = 0.1f;
	[SerializeField] private float amplitudeShakeGravityUse = 1.0f;
	[SerializeField] private float frequencyShakeGravityUse = 1.0f;
	[SerializeField] private float timeShakeGravityUse = 1.0f;
	[SerializeField] private TrailRenderer[] myTrails = null;
	[SerializeField] private Gradient gradientTrailPowerOn = null;
	[SerializeField] private Gradient gradientTrailPowerOff = null;
	[SerializeField] private Gradient gradientTrailNoUsePower = null;
	[SerializeField] private Color powerUseColor = Color.magenta;
	[SerializeField] private Color powerNoUseColor = Color.grey;
	[SerializeField] private float timeFlashColor = 0.1f;
	[SerializeField] private SpriteRenderer interactivePrompt = null;
	[SerializeField] private AudioClip interactivePromptSound = null;
	[SerializeField] private Sprite gravityOnSprite = null;
	[SerializeField] private Sprite gravityOffSprite = null;
	[SerializeField] private float desintegrationTime = 0.3f;

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
	private Coroutine _flashColorCoroutine;
	private Rigidbody2D _myRigidBody;
	private bool _isGrounded;
	private bool _previousIsGrounded;
	private bool _isAlive = true;
	private bool _canTurn = true;
	private bool _canMove = true;
	private Vector2 _inputs;
	private float _horizontalInput;
	private bool _isPressingDown;
	private bool _isPressingJump;
	private bool _isPressingRight;
	private bool _isPressingLeft;
	private int _numberGravityUseRemaining;
	private AudioSource _myAudioSource;
	private SpriteRenderer _mySpriteRenderer;
	private Animator _myAnimator;
	private bool _isFalling;
	private bool _isLookingRight = true;

	private void Awake()
	{
		_myRigidBody = GetComponent<Rigidbody2D>();
		_myAudioSource = GetComponent<AudioSource>();
		_mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_myAnimator = GetComponent<Animator>();
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
		_inputs = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxisRaw("Vertical");
		if (GameManager.Instance.NoVomitModeEnabled)
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
			if ((_isLookingRight && _horizontalInput < 0) || (!_isLookingRight && _horizontalInput > 0))
			{
				_isLookingRight = !_isLookingRight;
				_mySpriteRenderer.transform.localEulerAngles += Vector3.up * 180;
			}

			CheckGrounded();
			//restores gravity power
			if (_isGrounded && _previousIsGrounded != _isGrounded)
			{
				_myAnimator.SetTrigger("Land");
				_isFalling = false;
				_myAudioSource.PlayOneShot(stepSound);
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
		//handles when player try to turn but can't
		else if (Input.GetButtonDown("TurnLeft") || Input.GetButtonDown("TurnRight"))
		{
			FlashColor(powerNoUseColor, gradientTrailNoUsePower);
			_myAudioSource.PlayOneShot(gravityNoUseSound);
		}

		//handles retry input
		if (Input.GetButtonDown("Retry"))
		{
			Die();
		}

		//code for interacting with _interactives
		if (_interactives.Count > 0 && Input.GetAxisRaw("Vertical") > deadZoneVertical && _isGrounded)
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
		if (_isGrounded && _inputs.y <= 0)
		{
			if (_isPressingDown != Input.GetAxisRaw("Vertical") < 0)
			{
				_isPressingDown = Input.GetAxisRaw("Vertical") < 0;
				ToggleFreeze(_isPressingDown);
				GameManager.Instance.CameraManager.ToggleGlobalCamera(_isPressingDown);
			}

			if (_isPressingDown)
			{
				CheckGrounded();
				if (!_isGrounded)
				{
					ToggleFreeze(false);
					GameManager.Instance.CameraManager.ToggleGlobalCamera(false);
				}
			}
		}
	}

	private void ToggleFreeze(bool value)
	{
		_canMove = !value;
		_canTurn = !value;
		if (value)
		{
			ResetVelocityAndInput();
		}
	}

	private void ResetVelocityAndInput()
	{
		_myRigidBody.velocity = Vector2.zero;
		_horizontalInput = 0;
		_isPressingJump = false;
		_isPressingLeft = false;
		_isPressingRight = false;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Interactive") && !_interactives.Contains(other.gameObject))
		{
			interactivePrompt.enabled = true;
			_myAudioSource.PlayOneShot(interactivePromptSound);
			_interactives.Add(other.gameObject);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Interactive") && _interactives.Contains(other.gameObject))
		{
			_interactives.Remove(other.gameObject);
			if (_interactives.Count < 1)
			{
				interactivePrompt.enabled = false;
			}
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
			_myAnimator.SetFloat("Speed", Mathf.Abs(_horizontalInput));
			//executes a jump depending on direction
			if (_isPressingJump)
			{
				_myAnimator.SetTrigger("Jump");
				_myAudioSource.PlayOneShot(jumpSounds[Random.Range(0, jumpSounds.Length)]);
				_myRigidBody.velocity = transform.up.normalized * jumpSpeed +
				                        Vector3.Project(_myRigidBody.velocity, transform.right);
				_isPressingJump = false;
			}

			//applies fallMultiplier
			if (Vector3.Dot(Vector3.Project(_myRigidBody.velocity, transform.up).normalized,
				    transform.up.normalized) < 0)
			{
				if (!_isFalling && !_isGrounded)
				{
					_isFalling = true;
					_myAnimator.SetTrigger("Fall");
				}

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
			_myAudioSource.PlayOneShot(gravityBackSound);
			_numberGravityUseRemaining = maxNumberGravityUse;
			foreach (var trail in myTrails)
			{
				trail.colorGradient = gradientTrailPowerOn;
			}

			FlashColor(powerUseColor, gradientTrailPowerOn);
			_mySpriteRenderer.sprite = gravityOnSprite;

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
		_previousIsGrounded = _isGrounded;
		_isGrounded = Physics2D.CircleCast(transform.position, radiusGroundCheck, -transform.up, distMaxGroundCheck,
			layerGround);
	}

	private void FlashColor(Color color, Gradient gradient)
	{
		if (_flashColorCoroutine != null)
		{
			StopCoroutine(_flashColorCoroutine);
		}

		StartCoroutine(FlashingColor(color, gradient, timeFlashColor));
	}

	private IEnumerator FlashingColor(Color color, Gradient gradient, float time)
	{
		_mySpriteRenderer.color = color;
		Gradient previousGradient = myTrails[0].colorGradient;
		foreach (var trail in myTrails)
		{
			trail.colorGradient = gradient;
		}

		yield return new WaitForSeconds(time);
		_mySpriteRenderer.color = Color.white;
		foreach (var trail in myTrails)
		{
			trail.colorGradient = previousGradient;
		}
	}

	private void TurnTo(CardinalDirection direction)
	{
		//checks that the player can not go further than 180°
		if (_actualGravityDirection + ((int) _actualGravityDirection > 1 ? -2 : 2) != _previousGravityDirection)
		{
			//setup the first 90° turn
			if (_canMove)
			{
				_myAnimator.SetTrigger("UseGravity");
				GameManager.Instance.ChangeTimeScale(gravityTimeScale);
				_isGrounded = false;
				_myAudioSource.PlayOneShot(gravityUseSound);
				_canMove = false;
				_myRigidBody.velocity = Vector2.zero;
			}

			_actualGravityDirection = direction;
			if (!GameManager.Instance.NoVomitModeEnabled)
			{
				GameManager.Instance.CameraManager.ChangeVCamByDirection(_actualGravityDirection);
			}

			GameManager.Instance.CameraManager.Shake(amplitudeShakeGravityUse, frequencyShakeGravityUse,
				timeShakeGravityUse);

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
			transform.eulerAngles = Vector3.forward *
			                        (Mathf.LerpAngle(initRotPlayer, (float) _actualGravityDirection * 90.0f,
				                        timer / time));
			yield return null;
		}

		yield return new WaitForSeconds(timeBeforeGravityAgain);
		//restores player move after turning
		_previousGravityDirection = _actualGravityDirection;
		_isPressingLeft = false;
		_isPressingRight = false;
		_canMove = true;
		_numberGravityUseRemaining--;
		GameManager.Instance.ChangeTimeScale(1.0f);
		_myAnimator.SetTrigger("Fall");
		foreach (var trail in myTrails)
		{
			trail.colorGradient = gradientTrailPowerOff;
		}

		_mySpriteRenderer.sprite = gravityOffSprite;
	}

	public void Die()
	{
		if (_isAlive)
		{
			_isAlive = false;
			_canMove = false;
			_canTurn = false;
			_myAudioSource.PlayOneShot(deathSound);
			_myAnimator.SetTrigger("Death");
			StartCoroutine(Dying());
			GameManager.Instance.DeathsCounter++;
			GameManager.Instance.LoadLevel(SceneManager.GetActiveScene().name, true, true);
		}
	}

	private IEnumerator Dying()
	{
		float timer = 0.0f;
		while (timer < desintegrationTime)
		{
			timer += Time.deltaTime;
			_mySpriteRenderer.color = Color.Lerp(Color.white, new Color(0, 0, 0, 0.5f), timer / desintegrationTime);
			yield return null;
		}
	}
}