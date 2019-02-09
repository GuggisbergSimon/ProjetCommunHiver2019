﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
	[SerializeField] private float warmUpTime = 0.5f;
	[SerializeField] private float activeTime = 2.0f;
	[SerializeField] private float inactiveTime = 2.0f;
	[SerializeField] private float rotationSpeed = 10.0f;
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private float distance = 100.0f;
	[SerializeField] private LaserMode myMode = 0;
	[SerializeField] private Color warmingUpColor = Color.gray;
	[SerializeField] private Color inactiveColor = Color.clear;
	[SerializeField] private Color activeColor = Color.white;
	private bool _isActive = true;
	private SpriteRenderer _mySpriteRenderer;
	private LineRenderer _myLineRenderer;
	private BoxCollider2D _myCollider;

	private enum LaserMode
	{
		FollowsPlayerGravity,
		FollowsPlayer,
		FollowsRoutine
	}

	private void Start()
	{
		_myLineRenderer = GetComponent<LineRenderer>();
		_myCollider = GetComponentInChildren<BoxCollider2D>();
		_mySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
		StartCoroutine(SimpleRoutine());
	}

	private void Update()
	{
		Quaternion focusAngle = new Quaternion();
		Vector2 currentPos = transform.position;

		if (myMode == LaserMode.FollowsPlayerGravity)
		{
			focusAngle = GameManager.Instance.Player.transform.rotation;
		}
		else if (myMode == LaserMode.FollowsPlayer)
		{
			//calculate the angle between the player and the laser
			Vector2 diff = currentPos - (Vector2) GameManager.Instance.Player.transform.position;
			focusAngle = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90);
		}

		//limit the rotation through a rotationSpeed
		//note : it is possible that this speed is 0 or a value so high, turning seems to be instantaneous
		transform.rotation = Quaternion.RotateTowards(transform.rotation, focusAngle, rotationSpeed * Time.deltaTime);
		RaycastHit2D hit = Physics2D.Raycast(currentPos, -transform.up, distance,
			layerGround);

		//adjusts the linerenderer and the collider based on the raycast
		_myLineRenderer.SetPosition(0, currentPos);
		_myLineRenderer.SetPosition(1, hit.point);
		_myCollider.offset = Vector2.down * (hit.point - currentPos).magnitude / 2;
		_myCollider.size = Vector2.right * _myCollider.size + Vector2.up * (hit.point - currentPos).magnitude;
	}

	private IEnumerator SimpleRoutine()
	{
		while (true)
		{
			//simple routine who alternate between three state, warming up (sign of attack), active, and inactive
			ChangeColor(warmingUpColor);
			if (warmUpTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(warmUpTime);
			}

			ChangeColor(activeColor);
			_isActive = true;
			if (activeTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(activeTime);
			}

			ChangeColor(inactiveColor);
			_isActive = false;
			if (inactiveTime.CompareTo(0) != 0)
			{
				yield return new WaitForSeconds(inactiveTime);
			}
		}
	}

	private void ChangeColor(Color color)
	{
		_mySpriteRenderer.color = color;
		_myLineRenderer.startColor = color;
		_myLineRenderer.endColor = color;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_isActive && other.CompareTag("Player"))
		{
			other.GetComponent<PlayerController>().Die();
		}
	}
}