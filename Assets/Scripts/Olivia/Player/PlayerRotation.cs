using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{

    GameObject player;


    void Start()
    {

        player = GameObject.FindWithTag("Player");

    }


    void Update()
    {

        if (player != null) 
        {
            player.transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
        }
    }
}
