using GASHAPWN;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores Dictionary of Special Moves and is a middleman for execution
/// </summary>
public class SpecialMoveHandler : MonoBehaviour
{
    // Dictionary of Special Moves
    private Dictionary<string, ISpecialMove> moveSet;

    // Reference to Player Data
    private PlayerData playerData;

    // Reference to Special Move Host
    private MonoBehaviour host;

    // Single corotoutine ensures only one special move can execute at a time
    private Coroutine _specialMoveCoroutine = null;

    // Currently active special move
    private ISpecialMove _activeSpecialMove = null;

    private void Awake()
    {
        // Register Special Moves here
        // If attributes need to be adjusted, construct with new attributes
        moveSet = new Dictionary<string, ISpecialMove>
        {
            { "Jump", new Jump_SpecialMove() },
            { "Slam", new Slam_SpecialMove() },
            { "QuickBreak", new QuickBreak_SpecialMove() },
            { "ChargeRoll", new ChargeRoll_SpecialMove() }
        };
        playerData = GetComponent<PlayerData>();
        // maybe host should not be player data?
        host = GetComponent<PlayerData>();
    }

    /// <summary>
    /// Given string corresponding to special move, try to execute it
    /// </summary>
    public void TryUseSpecialMove(string moveName)
    {
        if (moveSet.TryGetValue(moveName, out ISpecialMove move))
        {
            if (move.CanExecute(playerData))
            {
                if (_activeSpecialMove != move && _activeSpecialMove != null) _activeSpecialMove.Cancel(playerData, host);
                if (_specialMoveCoroutine != null) StopCoroutine(_specialMoveCoroutine); 

                _specialMoveCoroutine = StartCoroutine(move.Execute(playerData, host));

                Debug.Log($"Executed: {move.Name}");
            }
            else
            {
                if (playerData.currentStamina < move.StaminaCost)
                {
                    playerData.OnLowStamina.Invoke(playerData.currentStamina);
                    Debug.Log($"Not enough stamina to execute move: {move.Name}");
                }
                else
                {
                    Debug.Log($"Cannot execute move: {move.Name} (other conditions failed)");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Move not found: {moveName}");
        }
    }

    /// <summary>
    /// Given string corresponding to special move, try to call its Cancel method
    /// </summary>
    /// <param name="moveName"></param>
    public void TryCancelSpecialMove(string moveName)
    {
        if (moveSet.TryGetValue(moveName, out ISpecialMove move))
        {
            if (move.CanCancel(playerData))
                move.Cancel(playerData, host);
        }
    }
}