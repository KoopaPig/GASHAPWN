using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GASHAPWN
{
    public class FigureManager : MonoBehaviour
    {

        public FigureDatabase figureDatabase;

        public static FigureManager instance { get; private set; }

        private void Awake()
        {
            if(instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;

            DontDestroyOnLoad(this);

            if(figureDatabase == null)
            {
                Debug.Log("Figure database not assigned to FigureManager");
                return;
            }

        }

        // Generates and returns a random figure from the dictionary
        // Does not take into consideration series
        public Figure GetRandomFigure()
        {
            // Detect if database is null or empty, don't reutrn
            if (figureDatabase == null || figureDatabase.figureDictionary.Count == 0)
            {
                Debug.LogWarning("FigureManager: No figures available in the database.");
                return null;
            }
            var figureList = figureDatabase.figureDictionary.Values.ToList<Figure>();
            int randomIndex = Random.Range(0, figureList.Count);
            Figure newFigure = figureList[randomIndex];
            return newFigure;
            
        }

        // Generates and returns a random figure from the dictionary, considers rarity
        // Does not take into consideration series
        public Figure GetRandomFigureWeighted() {
            // Detect if database is null or empty, don't reutrn
            if (figureDatabase == null || figureDatabase.figureDictionary.Count == 0)
            {
                Debug.LogWarning("FigureManager: No figures available in the database.");
                return null;
            }

            List<Figure> figures = new List<Figure>(figureDatabase.figureDictionary.Values);
            List<float> weights = new List<float>();

            float totalWeight = 0f;

            // How this works: add up all rarities
            foreach (var figure in figures)
            {
                float rarity = figure.Rarity;
                weights.Add(rarity);
                totalWeight += rarity;
            }

            // then select random value from range
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            // then iterate through the list and return the first figure where cumulativeWeight >= randomValue
            for (int i = 0; i < figures.Count; i++)
            {
                cumulativeWeight += weights[i];
                if (randomValue <= cumulativeWeight) return figures[i];
            }

            return figures[^1]; // fallback
        }

        public Figure GetFigureByID(string ID)
        {
            Figure result = null;

            List<Figure> figures = new List<Figure>(figureDatabase.figureDictionary.Values);
            foreach(Figure figure in figures)
            {
                if (figure.GetID() == ID)
                {
                    result = figure;
                    break;
                }
            }

            if (result == null) Debug.LogError("Figure of ID " + ID + " was not found");

            return result;
        }
    }

}

