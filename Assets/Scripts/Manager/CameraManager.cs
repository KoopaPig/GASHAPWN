using GASHAPWN;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Positions")]
    public Transform StartingPosition;
    public Transform EndingPosition;
    public Transform FocusDoorPosition;
    public Transform SpawnPosition;

    [Header("Cameras")]
    public Camera Maincam;
    public Camera Player1Cam;
    public Camera Player2Cam;

    public bool PathingEnabled;

    public GameObject Player1Capsule;
    public GameObject Player2Capsule;

    Figure WinningFigure;
    string WinningFigureTag;

    float time;
    private void Awake()
    {
        
        Maincam.transform.position = StartingPosition.position;
        Maincam.transform.rotation = StartingPosition.rotation;
    }

    private void Start()
    {
        
    }

    public void StartPath()
    {
        PathingEnabled = true;
        BattleManager.Instance.OnWinningFigure.AddListener(SwitchCamera);
        BattleManager.Instance.ChangeToNewFigure.AddListener(SwitchToMain);
    }

    public void SwitchCamera(string Tag, Figure figure)
    {
        WinningFigureTag = Tag;
        WinningFigure = figure;
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
        // Turn off player cameras
        if (Player1Cam.enabled) Player1Cam.enabled = false;
        else Player2Cam.enabled = false;

        // Translate main camera to focus on the door
        Maincam.transform.SetPositionAndRotation(FocusDoorPosition.position, FocusDoorPosition.rotation);

        // Spawn a player behind the door

        GameObject PlayerCapsule = null;
        if (WinningFigureTag == "Player1") PlayerCapsule = Player1Capsule;
        else if (WinningFigureTag == "Player2") PlayerCapsule = Player2Capsule;
        else Debug.Log("Unknown Tag");

        GameObject SpawnedCapsule = Instantiate(PlayerCapsule, SpawnPosition);
        Instantiate(WinningFigure.capsuleModelPrefab, SpawnedCapsule.transform);
        
        // Turn on main camera
        Maincam.enabled = true;
    }

    private float Timefunc(float time)
    {
        if(time < 0.75f)
        {
            time += 0.3f * Time.deltaTime;
        }
        else time += 0.1f * Time.deltaTime;
        return time;
    }

    private void Update()
    {
        if (PathingEnabled && BattleManager.Instance.countDownTime > 0)
        {
            Maincam.transform.position = Vector3.Lerp(StartingPosition.position, EndingPosition.position, time);
            Maincam.transform.rotation = Quaternion.Lerp(StartingPosition.rotation, EndingPosition.rotation, time);
            time = Timefunc(time);
        }
        else PathingEnabled = false;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Maincam.enabled = !Maincam.enabled;
            Player1Cam.enabled = !Player1Cam.enabled;
        }
    }
}