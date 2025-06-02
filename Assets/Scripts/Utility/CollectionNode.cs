using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Represents a node in the collection - a position for a figure to be displayed
    /// </summary>
    public class CollectionNode : MonoBehaviour
    {
        [Header("Node Connections")]
        [Tooltip("The previous node in the collection (null if this is the first node)")]
        public CollectionNode previousNode;
        
        [Tooltip("The next node in the collection (null if this is the last node)")]
        public CollectionNode nextNode;

        [Header("Figure Information")]
        [Tooltip("The transform where the figure model will be placed")]
        public Transform figureDisplayPoint;
        
        [Tooltip("The figure associated with this node (null if not collected yet)")]
        public Figure associatedFigure;
        
        [Tooltip("If true, this figure has been collected by the player")]
        public bool isCollected = false;

        [Tooltip("The amount of the figure associated with this note")]
        public int amount;

        [Header("Visual Elements")]
        [Tooltip("GameObject to show when this figure is selected")]
        public GameObject highlightEffect;
        
        [Tooltip("GameObject to show when this figure has not been collected")]
        public GameObject lockedVisual;
        
        // The model instance that's currently displayed
        private GameObject currentModel;

        public Transform cameraPosition;


        /// PRIVATE METHODS ///

        private void Awake()
        {
            // Initialize to correct visual state
            UpdateVisualState(false);
            
            // Create display point if needed
            if (figureDisplayPoint == null)
            {
                GameObject displayPointObj = new GameObject("DisplayPoint");
                displayPointObj.transform.SetParent(transform);
                displayPointObj.transform.localPosition = Vector3.zero;
                figureDisplayPoint = displayPointObj.transform;
            }
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Updates the visual state of the node based on selection and collection status
        /// </summary>
        /// <param name="isSelected">Whether this node is currently selected</param>
        public void UpdateVisualState(bool isSelected)
        {
            if (highlightEffect != null)
                highlightEffect.SetActive(isSelected);
            
            if (lockedVisual != null)
                lockedVisual.SetActive(!isCollected);
        }

        /// <summary>
        /// Displays the associated figure at this node's display point
        /// </summary>
        public void DisplayFigure()
        {
            // Clear any existing model
            ClearDisplayedFigure();
            
            if (!isCollected || associatedFigure == null)
                return;
            
            // Instantiate the figure's collection model at the display point
            currentModel = Instantiate(associatedFigure.collectionModelPrefab, 
                                      figureDisplayPoint.position, 
                                      figureDisplayPoint.rotation, 
                                      figureDisplayPoint);
        }

        /// <summary>
        /// Removes the currently displayed figure model
        /// </summary>
        public void ClearDisplayedFigure()
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
                currentModel = null;
            }
        }

        /// <summary>
        /// Rotates the displayed figure around the Y axis
        /// </summary>
        /// <param name="rotationAmount">Amount to rotate in degrees</param>
        public void RotateFigure(float rotationAmount)
        {
            if (currentModel != null)
            {
                currentModel.transform.Rotate(Vector3.up, rotationAmount);
            }
        }
    }
}