using UnityEngine;

[CreateAssetMenu(fileName = "Figure", menuName = "Scriptable Objects/Figure/Figure")]
public class Figure : ScriptableObject
{
    [Header("Attributes")]

    public string ID = "Undefined";
    public string Name = "Undefined";
    public string Description = "Undefined";
    public Sprite Icon = null;
    public float Rarity = 0;

    [Header("Prefabs")]

    public GameObject capsuleModelPrefab = null;
    public GameObject collectionModelPrefab = null;
}
