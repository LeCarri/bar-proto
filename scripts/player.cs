using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
public class player : MonoBehaviour
{
    private Rigidbody rb;

    public float interactDistance = 3f;

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


        //raycast para puertas

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.collider.CompareTag("Door"))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Door door = hit.collider.GetComponent<Door>();

                    if (door != null)
                    {
                        SceneManager.LoadScene(door.sotano);
                    }
                }
            }
        }


    }
}
