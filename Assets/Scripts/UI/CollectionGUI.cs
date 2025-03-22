using UnityEngine;

namespace GASHAPWN.UI
{
    public class CollectionGUI : MonoBehaviour
    {

        [SerializeField] private GameObject infoCard;
        [SerializeField] private GameObject navigationArrows;
        [SerializeField] private GameObject footer;

        private InfoCardGUI infoCardGUI;

        private void Awake()
        {
            infoCardGUI = infoCard.GetComponent<InfoCardGUI>();
            //infoCard.SetActive(false);
            //navigationArrows.SetActive(false);
            //footer.SetActive(false);
        }


    }
}

