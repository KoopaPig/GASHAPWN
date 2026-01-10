using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Generic Interface for a Special Move
    /// </summary>
    public interface ISpecialMove
    {
        string Name { get; }
        float StaminaCost { get; }
        float DamageMultiplier { get; }

        /// <summary>
        /// Returns true if move can execute after checks in body of function
        /// </summary>
        bool CanExecute(PlayerData playerData);

        /// <summary>
        /// Returns true if move is cancellable after checks in body of function
        /// </summary>
        bool CanCancel(PlayerData playerData);

        /// <summary>
        /// Actions to take when executing move (context.performed or context.started)
        /// </summary>
        /// <param name="playerData"></param>
        /// <param name="host">Host MonoBehavior when Coroutines are necessary</param>
        IEnumerator Execute(PlayerData playerData, MonoBehaviour host);

        /// <summary>
        /// For context.canceled or interruptions
        /// </summary>
        void Cancel(PlayerData playerData, MonoBehaviour host);
    }
}