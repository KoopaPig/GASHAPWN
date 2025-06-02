using GASHAPWN;
using UnityEngine;

/// <summary>
/// Helper to spawn capsules in gasha machine when game ends
/// </summary>
public class MachineCapsuleSpawner : MonoBehaviour
{
    [Tooltip("Spawn position for capsule")]
    public Transform machineCapsuleSpawnPos;

    [Header("Player Capsule Prefabs")]
        [Tooltip("Player 1 Machine Capsule Prefab")]
        public GameObject Player1MachineCapsule;
        [Tooltip("Player 2 Machine Capsule Prefab")]
        public GameObject Player2MachineCapsule;

    private Figure WinningFigure;
    private string WinningFigureTag;


    /// PRIVATE METHODS ///

    private void Start()
    {
        BattleManager.Instance.OnWinner.AddListener(SetWinningPlayer);
        BattleManager.Instance.ChangeToNewFigure.AddListener(SpawnMachineCapsule);
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnWinner.RemoveListener(SetWinningPlayer);
            BattleManager.Instance.ChangeToNewFigure.RemoveListener(SpawnMachineCapsule);
        }
    }


    /// PUBLIC METHODS ///

    /// <summary>
    /// Set winning player info
    /// </summary>
    // Called with BattleManager.OnWinner
    public void SetWinningPlayer(GameObject player, string tag, Figure figure)
    {
        WinningFigureTag = tag;
        WinningFigure = figure;
    }

    /// <summary>
    /// Spawns capsule with winning figure behind machine door
    /// </summary>
    /// <param name="state"></param>
    // Called with BattleManager.ChangeToNewFigure
    public void SpawnMachineCapsule(BattleState state)
    {
        GameObject PlayerCapsule = null;

        if (WinningFigureTag == "Player1") PlayerCapsule = Player1MachineCapsule;
        else if (WinningFigureTag == "Player2") PlayerCapsule = Player2MachineCapsule;
        else Debug.Log("Unknown Tag");

        // Spawn in capsule and figure
        GameObject SpawnedCapsule = Instantiate(PlayerCapsule, machineCapsuleSpawnPos);
        SpawnedCapsule.GetComponent<PlayerAttachedFigure>().SetFigureInCapsule(WinningFigure, 2.5f);
    }
}