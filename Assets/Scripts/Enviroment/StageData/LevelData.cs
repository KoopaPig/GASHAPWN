using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Runtime management and initialization controller for level
    /// </summary>
    public class LevelData : MonoBehaviour
    {
        [SerializeField] protected Transform player1SpawnPos;
        [SerializeField] protected Transform player2SpawnPos;

        public virtual void Initialize(MachineController machine_)
        {
            machine_.Player1SpawnPos = player1SpawnPos;
            machine_.Player2SpawnPos = player2SpawnPos;
        }
    }
}