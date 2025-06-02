using UnityEngine;
using System.Collections;

namespace GASHAPWN.Environment
{
	public class UpDownMover : MonoBehaviour
	{
		[Tooltip("How far up object moves")]
		public float moveDistance = 7f;

		[Tooltip("Speed of object")]
		public float moveSpeed = 4f;

		private Vector3 startPosition;

		private void Start()
		{
			startPosition = transform.position;
			StartCoroutine(MoveRoutine());
		}

		private IEnumerator MoveRoutine()
		{
			while (true)
			{
				float waitTime = Random.Range(0f, 15f);
				yield return new WaitForSeconds(waitTime);

				yield return StartCoroutine(MoveObject(startPosition, startPosition + Vector3.up * moveDistance));
				yield return StartCoroutine(MoveObject(startPosition + Vector3.up * moveDistance, startPosition));
			}
		}

		private IEnumerator MoveObject(Vector3 from, Vector3 to)
		{
			float elapsed = 0f;
			float duration = Vector3.Distance(from, to) / moveSpeed;

			while (elapsed < duration)
			{
				transform.position = Vector3.Lerp(from, to, elapsed / duration);
				elapsed += Time.deltaTime;
				yield return null;
			}

			transform.position = to;
		}
	}

}