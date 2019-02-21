using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReactToGravity : MonoBehaviour
{
	[SerializeField] private float rotationSpeed = 5.0f;
	[SerializeField] private float radiusGroundCheck = 1.0f;
	[SerializeField] private float distMaxGroundCheck = 1.0f;
	[SerializeField] private LayerMask layerGroundMask = 0;
	private Rigidbody2D _myRigidBody;
	private Collider2D _myCollider;
	private ReactToGravityState _myState;

	private enum ReactToGravityState
	{
		Idle,
		Falling,
		Rotating,
	}

	private void Start()
	{
		_myRigidBody = GetComponent<Rigidbody2D>();
		_myCollider = GetComponent<Collider2D>();
	}

	private void FixedUpdate()
	{
		switch (_myState)
		{
			case ReactToGravityState.Idle:
			{
				AddGravityForce();
				if (!IsSameGravityThanPlayer())
				{
					_myRigidBody.velocity = Vector2.zero;
					_myRigidBody.bodyType = RigidbodyType2D.Kinematic;
					_myState = ReactToGravityState.Rotating;
				}

				break;
			}
			case ReactToGravityState.Rotating:
			{
				transform.rotation = Quaternion.RotateTowards(transform.rotation,
					GameManager.Instance.Player.transform.rotation, rotationSpeed * Time.deltaTime);
				if (IsSameGravityThanPlayer())
				{
					_myRigidBody.velocity = Vector2.zero;
					_myState = ReactToGravityState.Falling;
				}

				break;
			}
			case ReactToGravityState.Falling:
			{
				AddGravityForce();
				if (!IsSameGravityThanPlayer())
				{
					_myState = ReactToGravityState.Rotating;
				}
				else if (IsGrounded())
				{
					_myRigidBody.bodyType = RigidbodyType2D.Dynamic;
					_myState = ReactToGravityState.Idle;
				}

				break;
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		//todo check for if player is hit in head then kill him
		/*if (other.transform.CompareTag("Player"))
		{
			other.transform.GetComponent<PlayerController>().Die();
		}*/
	}

	private void AddGravityForce()
	{
		_myRigidBody.AddForce(transform.up.normalized * Physics2D.gravity.y);
	}

	private bool IsGrounded()
	{
		return Physics2D.CircleCast(transform.position, radiusGroundCheck, -transform.up, distMaxGroundCheck,
			layerGroundMask);
	}

	private bool IsSameGravityThanPlayer()
	{
		return transform.eulerAngles.z.CompareTo(GameManager.Instance.Player.transform.eulerAngles.z) == 0;
	}
}