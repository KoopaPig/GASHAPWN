using System.Collections.Generic;
using UnityEngine;

namespace GASHAPWN {
    /// <summary>
    /// CollectedFigure is the saved approximation of Figure
    /// </summary>
    [System.Serializable]
    public class CollectedFigure
    {
        public string ID;
        public int amount;

        public CollectedFigure() { ID = null; amount = 0; }
        public CollectedFigure(Figure _figure) { ID = _figure.GetID(); amount = 1; }
    };

    /// <summary>
    /// CollectionData is the saved list of CollectedFigures
    /// </summary>
    [System.Serializable]
    public class CollectionData
    {
        public List<CollectedFigure> collection;
        public CollectionData() { new List<CollectedFigure>(); }
        public CollectionData(List<CollectedFigure> d) { collection = d; }

        public bool IsEmpty() { return collection.Count == 0; }
        public void Clear() { collection.Clear(); }
        public void Add(CollectedFigure figure) { collection.Add(figure); }
        public int Count() { return collection.Count; }

        // IN FUTURE: Tie collection data to player profiles
    }
}