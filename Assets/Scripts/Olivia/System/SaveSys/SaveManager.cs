using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private SaveData save;
    private string filePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.persistentDataPath, "Save.data");
        save = new SaveData();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SaveOnLoadScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SaveOnLoadScene;
    }

    public void SaveData()
    {
        if (save == null)
            save = new SaveData();

        int escenaActual = SceneManager.GetActiveScene().buildIndex;

        if (escenaActual == 0)
            escenaActual = 1;

        save.currentAct = escenaActual;
        WriteSave();
    }

    private void WriteSave()
    {
        string jsonSave = JsonUtility.ToJson(save, true);
        File.WriteAllText(filePath, jsonSave);
    }

    public void SaveOnLoadScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Home")
            return;

        SaveData();
        Debug.Log("Escena guardada: " + save.currentAct);
    }

    public void LoadData()
    {
        if (!File.Exists(filePath))
        {
            save = new SaveData();
            return;
        }

        string jsonSave = File.ReadAllText(filePath);
        save = JsonUtility.FromJson<SaveData>(jsonSave);

        if (save == null)
            save = new SaveData();
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
        save = new SaveData();
        save.currentAct = 1;

        WriteSave();

        Debug.Log("SAVE RESETEADO A ESCENA 1");

        if (UIConitnueHandler.Instance != null)
            UIConitnueHandler.Instance.UpdateText();
    }
}