using UnityEngine;

[CreateAssetMenu(fileName = "Figure", menuName = "Scriptable Objects/Figure")]
public class Figure : ScriptableObject
{
    [Header("Attributes")]

    public string ID;
    public string Name;
    public string Description;
    // Image
    // Rarity

    [Header("Prefabs")]

    public GameObject capsuleModelPrefab;
    public GameObject collectionModelPrefab;
}
