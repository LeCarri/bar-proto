using UnityEngine;

public class IntroMovement : MonoBehaviour
{
    

    public float velocity = 5.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (velocity != 0)
        {
            transform.Translate(Vector3.forward * velocity * Time.deltaTime);

        }





    }
}
