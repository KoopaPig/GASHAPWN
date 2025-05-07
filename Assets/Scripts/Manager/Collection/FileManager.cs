using System.IO;
using Newtonsoft.Json;
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

    // Save content to a file
    public static void Save<T>(string fileName, T content)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
        // convert to JSON and keep formatting
        string dataAsJson = JsonConvert.SerializeObject(content, Formatting.Indented);

        File.WriteAllText(filePath, dataAsJson);
        Debug.Log("To " + filePath + ": " + content);
    }

    public static T Load<T>(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName + ".json");
        // If the file does not exist, return a default value
        if (!File.Exists(filePath))
        {
            Debug.Log("File not found; Returning Default...");
            return default;
        }
        string dataAsJson = File.ReadAllText(filePath);
        

        T content = JsonUtility.FromJson<T>(dataAsJson);
        Debug.Log("From " + filePath + ": " + content);
        return content;
    }
}
