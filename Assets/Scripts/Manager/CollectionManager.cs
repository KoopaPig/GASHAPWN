using GASHAPWN.UI;
using UnityEngine;

namespace GASHAPWN {
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance;

        [Header("Animators")]
        [SerializeField] private Animator mainCameraAnimator;
        [SerializeField] private Animator doorAnimator;

        [SerializeField] public bool isCamIntro = true;

        private CollectionGUI collectionGUI;



        

        /// PUBLIC METHODS ///





        /// PRIVATE METHODS ///

        private void Awake()
        {
            mainCameraAnimator.enabled = false;
            collectionGUI = FindFirstObjectByType<CollectionGUI>();
        }

        private void Start()
        {
            collectionGUI.CollectionGUISetActive(false); // start CollectionGUI as inactive
            // Handle Camera Intro
            if (isCamIntro) {
                mainCameraAnimator.enabled = true;
                mainCameraAnimator.SetBool("isCamIntro", true);
                doorAnimator.SetTrigger("openDoor");
            } else {
                collectionGUI.CollectionGUISetActive(true);
                
                // Then snap to node-based camera transform change
            }
        }
    }
}

