using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace GASHAPWN
{
    /// <summary>
    /// Editor utility to help set up collection nodes in a scene
    /// </summary>
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    public class CollectionSetupUtility : MonoBehaviour
    {
        [Header("Node Creation")]
        [Tooltip("Parent transform where nodes will be created")]
        public Transform nodesParent;
        
        [Tooltip("Prefab to use for collection nodes")]
        public GameObject nodePrefab;
        
        [Tooltip("Figure database containing all collectable figures")]
        public FigureDatabase figureDatabase;
        
        [Tooltip("Spacing between nodes (used for auto-positioning)")]
        public float nodeSpacing = 2.0f;

        [Header("Camera Setup")]
        [Tooltip("Camera to use for collection viewing")]
        public Camera collectionCamera;
        
        [Tooltip("Distance from the camera to the collection nodes")]
        public float cameraDistance = 3.0f;
        
        [Tooltip("Height of the camera relative to the nodes")]
        public float cameraHeight = 1.0f;

        [Header("Layout Options")]
        [Tooltip("Whether to automatically position nodes in a line")]
        public bool autoPositionNodes = true;
        
        [Tooltip("Direction to align nodes along")]
        public Vector3 alignmentDirection = Vector3.right;
        
        [Tooltip("Whether to create a circular layout instead of linear")]
        public bool circularLayout = false;
        
        [Tooltip("Radius for circular layout")]
        public float circleRadius = 10f;

        // Store references to the created nodes
        private List<CollectionNode> nodes = new List<CollectionNode>();


        /// PRIVATE METHODS ///

        /// <summary>
        /// Creates a collection node for a specific figure
        /// </summary>
        private void CreateNodeForFigure(Figure figure, int index)
        {
            // Create the node from the prefab
            GameObject nodeObj = Instantiate(nodePrefab, nodesParent);
            nodeObj.name = $"CollectionNode_{figure.Name}";

            // Position the node based on layout type
            if (autoPositionNodes)
            {
                if (circularLayout)
                {
                    // Calculate position on a circle
                    float angle = (360f / figureDatabase.figureDictionary.Count) * index;
                    float radians = angle * Mathf.Deg2Rad;
                    float x = Mathf.Sin(radians) * circleRadius;
                    float z = Mathf.Cos(radians) * circleRadius;
                    nodeObj.transform.position = nodesParent.position + new Vector3(x, 0, z);

                    // Make node face center
                    nodeObj.transform.LookAt(new Vector3(nodesParent.position.x, nodeObj.transform.position.y, nodesParent.position.z));
                }
                else
                {
                    // Linear layout
                    nodeObj.transform.position = nodesParent.position + alignmentDirection.normalized * index * nodeSpacing;
                }
            }

            // Set up the CollectionNode component
            CollectionNode node = nodeObj.GetComponent<CollectionNode>();
            if (node == null)
                node = nodeObj.AddComponent<CollectionNode>();

            // Assign the figure
            node.associatedFigure = figure;

            // Set display point if it doesn't exist
            if (node.figureDisplayPoint == null)
            {
                GameObject displayPoint = new GameObject("DisplayPoint");
                displayPoint.transform.SetParent(nodeObj.transform);
                displayPoint.transform.localPosition = Vector3.zero;
                node.figureDisplayPoint = displayPoint.transform;
            }

            // Create camera position
            GameObject cameraPos = new GameObject("CameraPosition");
            cameraPos.transform.SetParent(nodeObj.transform);
            cameraPos.transform.localPosition = new Vector3(0, cameraHeight, -cameraDistance);
            cameraPos.transform.LookAt(node.figureDisplayPoint);

            // Add to our list
            nodes.Add(node);
        }

        /// <summary>
        /// Links the nodes together in a doubly-linked list
        /// </summary>
        private void LinkNodes()
        {
            if (nodes.Count == 0)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                // Set previous node (null for first node)
                nodes[i].previousNode = (i > 0) ? nodes[i - 1] : null;

                // Set next node (null for last node)
                nodes[i].nextNode = (i < nodes.Count - 1) ? nodes[i + 1] : null;
            }

            // Create circular references if desired
            if (circularLayout)
            {
                // Make last node point to first and first node point to last
                nodes[0].previousNode = nodes[nodes.Count - 1];
                nodes[nodes.Count - 1].nextNode = nodes[0];
            }
        }


        /// PUBLIC METHODS ///

        /// <summary>
        /// Method to set up collection nodes for all figures in the database
        /// </summary>
        [ContextMenu("Set Up Collection Nodes")]
        public void SetUpCollectionNodes()
        {
            if (figureDatabase == null)
            {
                Debug.LogError("CollectionSetupUtility: Figure database is required!");
                return;
            }

            if (nodePrefab == null)
            {
                Debug.LogError("CollectionSetupUtility: Node prefab is required!");
                return;
            }

            // Create a parent if one doesn't exist
            if (nodesParent == null)
            {
                GameObject parent = new GameObject("CollectionNodes");
                parent.transform.position = transform.position;
                nodesParent = parent.transform;
            }

            // Clear existing nodes
            nodes.Clear();
            
            #if UNITY_EDITOR
            // Calculate the total number of figures
            int totalFigures = figureDatabase.figureDictionary.Count;
            int currentFigure = 0;
            
            // Start creating nodes for each figure
            foreach (var kvp in figureDatabase.figureDictionary)
            {
                Figure figure = kvp.Value;
                EditorUtility.DisplayProgressBar("Creating Collection Nodes", 
                    $"Creating node for {figure.Name}", 
                    (float)currentFigure / totalFigures);
                
                CreateNodeForFigure(figure, currentFigure);
                currentFigure++;
            }
            
            EditorUtility.ClearProgressBar();
            #else
            // Runtime version without progress bar
            int currentFigure = 0;
            foreach (var kvp in figureDatabase.figureDictionary)
            {
                Figure figure = kvp.Value;
                CreateNodeForFigure(figure, currentFigure);
                currentFigure++;
            }
            #endif

            // Link the nodes in a chain
            LinkNodes();
            
            Debug.Log($"CollectionSetupUtility: Created and linked {nodes.Count} collection nodes.");
        }
        
        /// <summary>
        /// Test the camera view for each node
        /// </summary>
        [ContextMenu("Test Camera Views")]
        public void TestCameraViews()
        {
            if (collectionCamera == null)
            {
                Debug.LogError("CollectionSetupUtility: Collection camera is required!");
                return;
            }
            
            // Find all nodes if none are in the list
            if (nodes.Count == 0)
            {
                nodes.AddRange(nodesParent.GetComponentsInChildren<CollectionNode>());
                
                if (nodes.Count == 0)
                {
                    Debug.LogError("CollectionSetupUtility: No collection nodes found!");
                    return;
                }
            }
            
            #if UNITY_EDITOR
            // Display progress bar
            int totalNodes = nodes.Count;
            for (int i = 0; i < totalNodes; i++)
            {
                CollectionNode node = nodes[i];
                
                EditorUtility.DisplayProgressBar("Testing Camera Views", 
                    $"Testing view for {node.name}", 
                    (float)i / totalNodes);
                
                // Get camera position object
                Transform cameraPos = node.transform.Find("CameraPosition");
                if (cameraPos == null)
                {
                    Debug.LogWarning($"No CameraPosition found for {node.name}");
                    continue;
                }
                
                // Move camera to position
                Vector3 originalPos = collectionCamera.transform.position;
                Quaternion originalRot = collectionCamera.transform.rotation;
                
                collectionCamera.transform.position = cameraPos.position;
                collectionCamera.transform.rotation = cameraPos.rotation;
                
                // Wait a moment to update the scene view
                EditorApplication.QueuePlayerLoopUpdate();
                
                // Restore camera
                collectionCamera.transform.position = originalPos;
                collectionCamera.transform.rotation = originalRot;
            }
            
            EditorUtility.ClearProgressBar();
            #endif
            
            Debug.Log("CollectionSetupUtility: Camera view test complete");
        }
        
        /// <summary>
        /// Clear all created nodes
        /// </summary>
        [ContextMenu("Clear All Nodes")]
        public void ClearAllNodes()
        {
            #if UNITY_EDITOR
            if (nodesParent == null)
                return;
                
            // Get all child nodes
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in nodesParent)
            {
                toDestroy.Add(child.gameObject);
            }
            
            // Destroy them
            foreach (GameObject obj in toDestroy)
            {
                DestroyImmediate(obj);
            }
            
            nodes.Clear();
            Debug.Log("CollectionSetupUtility: Cleared all nodes");
            #endif
        }
    }
    #endif
}