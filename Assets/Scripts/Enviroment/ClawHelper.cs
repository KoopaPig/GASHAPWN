using UnityEngine;

namespace GASHAPWN.Environment
{
    // Ensures claw object is lined up with player spawn position
    public class ClawHelper : MonoBehaviour
    {
        [Tooltip("Reference to Machine Controller component in parent")]
        [SerializeField] private MachineController machineController;

        [Tooltip("Claw targets this player")]
        [SerializeField] private TargetPlayer targetPlayer;

        private void Start()
        {
            Transform playerSpawnPos = machineController.Player1SpawnPos;
            switch (targetPlayer)
            {
                case TargetPlayer.PLAYER1:
                    playerSpawnPos = machineController.Player1SpawnPos;
                    break;
                case TargetPlayer.PLAYER2:
                    playerSpawnPos = machineController.Player2SpawnPos;
                    break;
                default:
                    break;
            }
           
            // Make sure claw is at same x and z coord of corresponding player spawn position
            var spawnPos = new Vector3(playerSpawnPos.position.x, transform.position.y, playerSpawnPos.position.z);
            this.transform.position = spawnPos;
        }

        public enum TargetPlayer { PLAYER1, PLAYER2 }
    }
}