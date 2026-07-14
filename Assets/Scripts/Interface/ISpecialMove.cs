using System;
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
        bool CanExecute(SpecialMoveHandler spMoveHandler);

        /// <summary>
        /// Returns true if move is cancellable after checks in body of function
        /// </summary>
        bool CanCancel(SpecialMoveHandler spMoveHandler);

        /// <summary>
        /// Actions to take when executing move (context.performed or context.started)
        /// </summary>
        IEnumerator Execute(SpecialMoveHandler spMoveHandler);

        /// <summary>
        /// For context.canceled or interruptions
        /// </summary>
        ISpecialMove Cancel(SpecialMoveHandler spMoveHandler);

        /// <summary>
        /// Depending on the special move, it might be necessary to split into states for functional clarity
        /// </summary>
        Enum GetSubState();

        /// <summary>
        /// Many special moves require context from the player controller
        /// </summary>
        public struct SpecialMoveContext
        {
            public Vector2 moveInput;

            public SpecialMoveContext(Vector2 moveInput_)
            {
                this.moveInput = moveInput_;
            }
        }
    }
}