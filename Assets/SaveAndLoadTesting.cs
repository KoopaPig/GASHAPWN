using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GASHAPWN
{
    public class SaveAndLoadTesting : MonoBehaviour
    {

        public Transform startingPosition;
        Transform currentPosition;

        [System.Serializable]
        public class Data
        {
            [SerializeReference]
            public List<string> figureIDs;

            public Data() { figureIDs = new (); }
        }

        public Data playerData;

        public List<GameObject> figures;
        private void Start()
        {
            currentPosition = startingPosition;
        }

        public void AddFigure(string ID)
        {
            Figure figureToAdd;
            if (ID == "empty") figureToAdd = FigureManager.instance.GetRandomFigure();
            else figureToAdd = FigureManager.instance.GetFigureByID(ID);

            // If the current position is equal to the starting position (the first figure),
            if (currentPosition == startingPosition)
            {
                // Spawn at starting position & translate
                figures.Add(Instantiate(figureToAdd.collectionModelPrefab, startingPosition.position, startingPosition.rotation));
                currentPosition.Translate(1.0f, 0, 0);
            }
            // Else,
            else
            {
                // Spawn at current position & translate
                figures.Add(Instantiate(figureToAdd.collectionModelPrefab, currentPosition.position, currentPosition.rotation));
                float translatePositionValue = 0;
                if (currentPosition.position.x > 0) translatePositionValue = currentPosition.position.x * -1;
                else translatePositionValue = currentPosition.position.x * -1 + 1.0f;
                currentPosition.Translate(translatePositionValue, 0, 0);
            }

            if(ID == "empty") playerData.figureIDs.Add(figureToAdd.GetID());

        }

        public void SaveFigures()
        {
            FileManager.Save<Data>("test", playerData);
        }

        public void RemoveFigures()
        {
            foreach(GameObject figure in figures)
            {
                Destroy(figure);
            }
            figures.Clear();
            playerData.figureIDs.Clear();
        }

        public void LoadFigures()
        {
            RemoveFigures();
            playerData = FileManager.Load<Data>("test");
            UpdateFigures(playerData);
        }

        private void UpdateFigures(Data data)
        {
            foreach (string ID in data.figureIDs)
            {
                AddFigure(ID);
            }
        }
    }
}

