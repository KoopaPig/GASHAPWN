using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GASHAPWN
{
    [CreateAssetMenu(fileName = "Figure", menuName = "Scriptable Objects/Figure/Figure")]
    public class Figure : ScriptableObject
    {
        [Header("Attributes")]

        private string ID = string.Empty;

        public string Name = "Undefined";
        public string Description = "Undefined";
        public Sprite Icon = null;
        public float Rarity = 0;

        [SerializeField] protected Series series = null;
        [SerializeField] protected int numberInSeries = 0;

        [Header("Prefabs")]

        public GameObject capsuleModelPrefab = null;
        public GameObject collectionModelPrefab = null;

        // Set the Series (single-assignment only)
        public void SetSeries(Series series)
        {
            if (this.series == null)
            {
                this.series = series;
            }
            else if (this.series != series)
            {
                Debug.LogWarning($"{Name} already belongs to {series.SeriesName}. Clear Figure.series and add the Figure to Series.FiguresInSeries.");
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

        private void OnValidate()
        {
            if (series != null) { ID = series.name + "_" + numberInSeries; }
            else Debug.LogWarning($"{Name} is not assigned to a series and an ID cannot be constructed.");
        }
    }
}

