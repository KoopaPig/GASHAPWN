using UnityEngine;
using GASHAPWN.UI;

public class StaminaTest : MonoBehaviour
{
    [SerializeField] private BattleGUIController battleControllerP1;

    private void Awake()
    {
        battleControllerP1.SetMaxStaminaGUI(300);
    }
}
