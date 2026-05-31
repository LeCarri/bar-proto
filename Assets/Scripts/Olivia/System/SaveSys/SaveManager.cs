using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private SaveData save;
    private string filePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        filePath = Path.Combine(Application.persistentDataPath, "Save.data");
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        save = new SaveData();
    }

    public void SaveData() 
    {
        save.currentAct = SceneManager.GetActiveScene().buildIndex;
        if (save.currentAct == 0)
            save.currentAct = 1;

        string jsonSave = JsonUtility.ToJson(save, true);
        File.WriteAllText(filePath, jsonSave);

    }

    public void LoadData() 
    {
        if (!File.Exists(filePath)) return;

        string jsonSave = File.ReadAllText(filePath);
        save = JsonUtility.FromJson<SaveData>(jsonSave);

        //Debug.Log(jsonSave);
    }

    public void LoadSave() 
    {
        LoadData();
 
        SceneManager.LoadScene(save.currentAct);
    }

    public int GetSavedAct() 
    {
        LoadData();

        return save.currentAct;
    }

    public void WipeData() 
    {
        save.currentAct = 1;
        SaveData();

        if (UIConitnueHandler.Instance != null)
            UIConitnueHandler.Instance.UpdateText();
    }
}
