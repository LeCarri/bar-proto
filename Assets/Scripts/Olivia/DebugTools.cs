using GLTFast.Schema;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugTools : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
            EndNight();
    }

    void EndNight() 
    {
        
        int currScene = SceneManager.GetActiveScene().buildIndex;

        Debug.Log("Ending Scene " + currScene);
        switch (currScene)
        {
            case 0: SaveManager.Instance.LoadSave(); break;
            case 1: Act1Manager.Instance.EndNight(); break;
        }
    }
}
