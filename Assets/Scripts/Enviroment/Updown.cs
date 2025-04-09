using UnityEngine;

public class UpDownMover : MonoBehaviour
{
    public float moveDistance = 7f;       // How far up it goes
    public float moveSpeed = 4f;          // How fast it moves
    private Vector3 startPosition;
    private bool isMoving = false;

    void Start()
    {
        startPosition = transform.position;
        StartCoroutine(MoveRoutine());
    }

    System.Collections.IEnumerator MoveRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(0f, 15f);
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(MoveObject(startPosition, startPosition + Vector3.up * moveDistance));
            yield return StartCoroutine(MoveObject(startPosition + Vector3.up * moveDistance, startPosition));
        }
    }

    System.Collections.IEnumerator MoveObject(Vector3 from, Vector3 to)
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

