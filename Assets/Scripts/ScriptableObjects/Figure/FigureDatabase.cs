using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FigureDatabase", menuName = "Scriptable Objects/Figure/FigureDatabase")]
public class FigureDatabase : ScriptableObject
{
    public Dictionary<string, Figure> figureDictionary = new();

    [SerializeField] private List<Figure> figureList = new();

    private void OnEnable()
    {
        figureDictionary.Clear();
        foreach (Figure figure in figureList)
        {
            if (!figureDictionary.ContainsKey(figure.Name))
            {
                figureDictionary.Add(figure.Name, figure);
            }
            else
            {
                Debug.LogWarning($"Duplicate figure found: {figure.ID} , {figure.Name}.");
            }
        }
    }
}
