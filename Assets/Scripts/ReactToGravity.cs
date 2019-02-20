using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReactToGravity : MonoBehaviour
{
	[SerializeField] private float rotationSpeed = 5.0f;
	[SerializeField] private float radiusGroundCheck = 1.0f;
	[SerializeField] private float distMaxGroundCheck = 1.0f;
	[SerializeField] private LayerMask layerGround = 0;
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
		//todo to test
		switch (_myState)
		{
			case ReactToGravityState.Idle:
			{
				AddGravityForce();
				if (!CompareGravity())
				{
					_myRigidBody.velocity = Vector2.zero;
					_myCollider.enabled = false;
					_myState = ReactToGravityState.Rotating;
				}

				break;
			}
			case ReactToGravityState.Rotating:
			{
				if (CompareGravity())
				{
					_myState = ReactToGravityState.Falling;
				}

				break;
			}
			case ReactToGravityState.Falling:
			{
				AddGravityForce();
				if (CompareGravity())
				{
					_myState = ReactToGravityState.Rotating;
				}
				else if (IsGrounded())
				{
					_myCollider.enabled = true;
					_myState = ReactToGravityState.Idle;
				}

				break;
			}
		}

		//todo remove magical number
		if (transform.rotation.Compare(GameManager.Instance.Player.transform.rotation, 100))
		{
			_myCollider.enabled = true;
			_myRigidBody.AddForce(transform.up.normalized * Physics2D.gravity.y);
		}
		else
		{
			_myRigidBody.velocity = Vector2.zero;
			_myCollider.enabled = false;
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				GameManager.Instance.Player.transform.rotation, rotationSpeed * Time.deltaTime);
		}
	}

	private void AddGravityForce()
	{
		_myRigidBody.AddForce(transform.up.normalized * Physics2D.gravity.y);
	}

	private bool IsGrounded()
	{
		return Physics2D.CircleCast(transform.position, radiusGroundCheck, -transform.up, distMaxGroundCheck,
			layerGround);
	}

	private bool CompareGravity()
	{
		return transform.rotation.Compare(GameManager.Instance.Player.transform.rotation, 1);
	}
}