using UnityEngine;

public class player : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private int speed = 3;
    [SerializeField] private int sens = 3;
    private float limitrotation = 0f;
    

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        //player movement

        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(Horizontal * speed * Time.deltaTime, 0, Vertical * speed * Time.deltaTime));


        //camera movement

        float rotationY = Input.GetAxis("Mouse X");
        transform.Rotate(new Vector3(0, rotationY * Time.deltaTime * sens, 0));


        float rotationX = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(-rotationX * Time.deltaTime * sens, 0 , 0));

        limitrotation = rotationY;
        limitrotation = Mathf.Clamp(rotationX, -90f, 90f);

        
    }
}
