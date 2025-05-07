using UnityEngine;
using System.Collections.Generic;

namespace GASHAPWN
{
    [DefaultExecutionOrder(-4)]
    [CreateAssetMenu(fileName = "FigureDatabase", menuName = "Scriptable Objects/Figure/FigureDatabase")]
    public class FigureDatabase : ScriptableObject
    {
        // list of all series
        public List<Series> allSeries;

        // dictionary of figures
        public Dictionary<string, Figure> figureDictionary = new();

        // internal figureList for database
        private readonly List<Figure> figureList = new();

        // Update figureDictionary given allSeries
        public void UpdateFigureDictionary()
        {
            figureDictionary.Clear();
            foreach (var series in allSeries)
            {
                foreach (var figure in series.GetFigures())
                {
                    if (!figureDictionary.ContainsKey(figure.Name))
                    {
                        figureDictionary.Add(figure.Name, figure); // Add to dictionary
                        figure.SetSeries(series); // Set series for figure
                    }
                    else
                    {
                        Debug.LogWarning($" FigureDatabase: Duplicate figure found: {figure.GetID()} , {figure.Name}.");
                    }
                }
            }
        }

        private void OnEnable()
        {
            // This only needs to be called whenever figures / series are added.
            // This can eventually move out of OnEnable(), since it is inefficient
            UpdateFigureDictionary();
        }

    }
}