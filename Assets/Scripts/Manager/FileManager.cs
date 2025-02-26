using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public static FileManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(this);
    }
    public static void Save<T>(string fileName, T content)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        string dataAsJson = JsonUtility.ToJson(content);

        File.WriteAllText(filePath, dataAsJson);
    }

    public static T Load<T>(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        string dataAsJson = File.ReadAllText(filePath);

        T content = JsonUtility.FromJson<T>(dataAsJson);
        return content;
    }
}
