using UnityEngine;

public class ClawHelper : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPos;

    private void Start()
    {
        // Make sure claw is at same x and z coord of corresponding player spawn position
        var spawnPos = new Vector3(playerSpawnPos.position.x, transform.position.y, playerSpawnPos.position.z);
        this.transform.position = spawnPos;
    }
}
