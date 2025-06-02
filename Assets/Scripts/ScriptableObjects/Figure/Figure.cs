using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GASHAPWN
{
    /// <summary>
    /// Main collectible; defines class for associated model
    /// </summary>
    [CreateAssetMenu(fileName = "Figure", menuName = "Scriptable Objects/Figure/Figure")]
    public class Figure : ScriptableObject
    {
        [Header("Attributes")]
        public string Name = "Undefined";
        public string Description = "Undefined";
        public Sprite Icon = null;
        [Range(0, 1)] public float Rarity = 0;

        [NonSerialized] protected Series series = null;
        [NonSerialized] protected int numberInSeries = 0;
        // ID is of form {series.seriesName}_{numberInSeries}
        private string ID = string.Empty;

        [Header("Prefabs")]
        // Prefab that appears in-capsule
        public GameObject capsuleModelPrefab = null;
        // Prefab that appears in-collection
        public GameObject collectionModelPrefab = null;


        /// PUBLIC METHODS ///

        // Set the Series (single-assignment only)
        public void SetSeries(Series series)
        {
            if (this.series == null)
            {
                this.series = series;
            }
            else if (this.series != series)
            {
                // Warn if figure is already part of another series
                Debug.LogWarning($"{Name} already belongs to {series.SeriesName}. Remove {Name} from {series.SeriesName} and try SetFigure again.");
            }
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public Series GetSeries()
        {
            return series;
        }

        public void SetNumberInSeries(int n) { 
            numberInSeries = n;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        public int GetNumberInSeries() { return numberInSeries; }

        public string GetID() { return ID; }

        public void SetID(string str) { ID = str; }
    }
}

