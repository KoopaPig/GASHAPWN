using GASHAPWN;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Positions")]
    public Transform StartingPosition;
    public Transform EndingPosition;

    [Header("Cameras")]
    public Camera Maincam;
    public Camera Player1Cam;
    public Camera Player2Cam;

    public bool PathingEnabled;

    private void Awake()
    {
        Maincam.transform.position = StartingPosition.position;
        Maincam.transform.rotation = StartingPosition.rotation;
    }

    public void StartPath()
    {
        PathingEnabled = true;
        BattleManager.Instance.OnWinningFigure.AddListener(SwitchCamera);
        BattleManager.Instance.ChangeToNewFigure.AddListener(SwitchToMain);
    }

    public void SwitchCamera(string Tag, Figure figure)
    {
        if (Tag == "Player1")
        {
            Maincam.enabled = false;
            Player1Cam.enabled = true;
        }
        else if (Tag == "Player2")
        {
            Maincam.enabled = false;
            Player2Cam.enabled = true;
        }
        else Debug.LogError("Unknown Tag");
    }

    public void SwitchToMain(BattleState state)
    {
        if (Player1Cam.enabled) Player1Cam.enabled = false;
        else Player2Cam.enabled = false;
        Maincam.enabled = true;
    }

    private void Update()
    {
        if (PathingEnabled && BattleManager.Instance.countDownTime > 0)
        {
            Maincam.transform.position = Vector3.Lerp(StartingPosition.position, EndingPosition.position, 1/BattleManager.Instance.countDownTime);
            Maincam.transform.rotation = Quaternion.Lerp(StartingPosition.rotation, EndingPosition.rotation, 1/BattleManager.Instance.countDownTime);
        }
        else PathingEnabled = false;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Maincam.enabled = !Maincam.enabled;
            Player1Cam.enabled = !Player1Cam.enabled;
        }
    }
}
