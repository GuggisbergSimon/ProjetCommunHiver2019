using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour
{
	[SerializeField] private float warmUpTime = 0.5f;
	[SerializeField] private float activeTime = 2.0f;
	[SerializeField] private float inactiveTime = 2.0f;
	[SerializeField] private LayerMask layerGround = 0;
	[SerializeField] private float distance = 100.0f;
	private bool _isActive = true;
	private SpriteRenderer _mySprite;
	private LineRenderer _myLineRenderer;
	private BoxCollider2D _myCollider;

	private void Start()
	{
		_myLineRenderer = GetComponent<LineRenderer>();
		_myCollider = GetComponentInChildren<BoxCollider2D>();
		_mySprite = GetComponentInChildren<SpriteRenderer>();
		StartCoroutine(SimpleRoutine());
	}

	private void Update()
	{
		//follows player rotation
		Vector2 currentPos = transform.position;
		RaycastHit2D hit = Physics2D.Raycast(currentPos, -transform.up, distance,
			layerGround);
		transform.rotation = GameManager.Instance.Player.transform.rotation;
		_myLineRenderer.SetPosition(0, currentPos);
		_myLineRenderer.SetPosition(1, hit.point);
		_myCollider.offset =Vector2.down * (hit.point - currentPos).magnitude / 2;
		_myCollider.size = Vector2.right * _myCollider.size + Vector2.up * (hit.point - currentPos).magnitude;
	}

	private IEnumerator SimpleRoutine()
	{
		while (true)
		{
			ChangeLineColor(Color.gray);
			yield return new WaitForSeconds(warmUpTime);
			ChangeLineColor(Color.white);
			_isActive = true;
			yield return new WaitForSeconds(activeTime);
			ChangeLineColor(Color.clear);
			_isActive = false;
			yield return new WaitForSeconds(inactiveTime);
		}
	}

	private void ChangeLineColor(Color color)
	{
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