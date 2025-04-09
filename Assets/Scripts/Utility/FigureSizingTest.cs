using UnityEngine;
using GASHAPWN;

public class FigureSizingTest : MonoBehaviour
{
    [SerializeField] public Figure selectedFigure;
    [SerializeField] private Transform shelfPos;

    private bool figureSet = false;
    public void SetFigure()
    {
        if (!figureSet)
        {
            FindFirstObjectByType<PlayerAttachedFigure>().SetFigureInCapsule(selectedFigure);
            Instantiate(selectedFigure.collectionModelPrefab, shelfPos);
            figureSet = true;
        }
        else Debug.LogWarning("Figure already set!");
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetFigure();
        }
    }
}
