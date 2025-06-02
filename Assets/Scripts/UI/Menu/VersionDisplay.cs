using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
/// <summary>
/// Gets and displays version info from Application
/// </summary>
public class VersionDisplay : MonoBehaviour
{
    private void Awake()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        text.text = Application.version;
    }
}