using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN
{
    [CreateAssetMenu(fileName = "Series", menuName = "Scriptable Objects/Series")]
    public class Series : ScriptableObject
    {
        [Header("Atrributes")]
        public string SeriesName = "Default";
        public Sprite SeriesIcon = null;
        [SerializeField] protected List<Figure> FiguresInSeries = new List<Figure>();

        // If figure in series, establish reference in figure.series.
        private void OnValidate()
        {
            for (int i = 0; i < FiguresInSeries.Count; i++)
            {
                if (FiguresInSeries[i] != null)
                {
                    FiguresInSeries[i].SetSeries(this);
                    // numberInSeries is set according to place in series
                    FiguresInSeries[i].SetNumberInSeries(i + 1);
                }
            }
        }

        public int Size()
        {
            if (FiguresInSeries.Count > 0) return FiguresInSeries.Count;
            else return 0;
        }
    }
}