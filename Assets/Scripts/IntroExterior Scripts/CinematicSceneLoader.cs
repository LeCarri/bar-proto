/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicSceneLoader : MonoBehaviour
{
    public void LoadBarScene()
    {
        SceneManager.LoadScene("Night_1 Scene");
    }
}
*/


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicSceneLoader : MonoBehaviour
{
    [Header("Pantalla negra de transición")]
    [SerializeField] private Image transitionBlack;

    private void Awake()
    {
        // La pantalla empieza oculta.
        if (transitionBlack != null)
        {
            Color color = transitionBlack.color;
            color.a = 1f;
            transitionBlack.color = color;
            transitionBlack.gameObject.SetActive(false);
        }
    }

    public void LoadBarScene()
    {
        if (transitionBlack == null)
        {
            Debug.LogError(
                "[CinematicSceneLoader] Falta asignar TransitionBlack."
            );
            return;
        }

        // Mostrar el negro al finalizar la cinemática.
        transitionBlack.gameObject.SetActive(true);

        Color color = transitionBlack.color;
        color.a = 1f;
        transitionBlack.color = color;

        transitionBlack.raycastTarget = false;

        SceneManager.LoadScene("Night_1 Scene");
    }
}
