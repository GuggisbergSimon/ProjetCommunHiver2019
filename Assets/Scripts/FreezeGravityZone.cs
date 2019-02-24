using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.U2D;

public class FreezeGravityZone : MonoBehaviour
{
	[SerializeField] private float timeBetweenChangeOfTexture = 0.05f;
	[SerializeField] private Texture2D[] noiseTextures = null;
	private Collider2D _myCollider;
	private SpriteShapeController _mySpriteShape;

	private void Start()
	{
		_mySpriteShape = GetComponent<SpriteShapeController>();
		_myCollider = GetComponent<Collider2D>();
		StartCoroutine(ChangeTexture());
	}

	private IEnumerator ChangeTexture()
	{
		int i = 0;
		while (true)
		{
			yield return new WaitForSeconds(timeBetweenChangeOfTexture);
			i++;
			i %= noiseTextures.Length;
			_mySpriteShape.spriteShape.fillTexture = noiseTextures[i];
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			other.GetComponent<PlayerController>().ToggleTurningUse(false);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player") && !_myCollider.bounds.Contains(other.transform.position))
		{
			other.GetComponent<PlayerController>().ToggleTurningUse(true);
		}
	}
}