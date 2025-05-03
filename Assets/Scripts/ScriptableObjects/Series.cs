using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GASHAPWN
{
    [CreateAssetMenu(fileName = "Series", menuName = "Scriptable Objects/Series")]
    public class Series : ScriptableObject
    {
        [Header("Atrributes")]
        public string SeriesName = "Default";
        public Sprite SeriesIcon = null;
        [SerializeField] private List<Figure> figuresInSeries = new List<Figure>();

        // Returns true if series contains given figure
        public bool ContainsFigure(Figure figure)
        {
            return figuresInSeries.Contains(figure);
        }

        private void OnValidate()
        {
            SetSeriesForFigures();
        }

        private void Awake()
        {
            SetSeriesForFigures();
        }

        public int Size()
        {
            if (figuresInSeries.Count > 0) return figuresInSeries.Count;
            else return 0;
        }

        public List<Figure> GetFigures()
        {
            return figuresInSeries;
        }

        // If figure in series, establish reference in figure.series.
        private void SetSeriesForFigures()
        {
            for (int i = 0; i < figuresInSeries.Count; i++)
            {
                // numberInSeries is set according to place in series
                figuresInSeries[i].SetNumberInSeries(i + 1);
                if (figuresInSeries[i].GetSeries() == null)
                {
                    figuresInSeries[i].SetSeries(this);
                    // construct new ID in here
                    figuresInSeries[i].SetID(SeriesName + "_" + (i + 1).ToString());
                    #if UNITY_EDITOR
                    EditorUtility.SetDirty(figuresInSeries[i]);
                    #endif
                }
            }
        }


    }
}