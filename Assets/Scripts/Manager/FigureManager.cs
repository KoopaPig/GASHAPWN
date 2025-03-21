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
        public Figure GetRandomFigure()
        {
            var figureList = figureDatabase.figureDictionary.Values.ToList<Figure>();
            int randomIndex = UnityEngine.Random.Range(0, figureList.Count);
            Figure newFigure = figureList[randomIndex];
            return newFigure;
            
        }

    }

}

