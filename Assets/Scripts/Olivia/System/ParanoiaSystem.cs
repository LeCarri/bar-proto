using UnityEngine;
using UnityEngine.UI;

public class ParanoiaSystem : MonoBehaviour
{
    public static ParanoiaSystem Instance { get; private set; }

    [SerializeField]
    private Image paranoiaBarImage;

    private float paranoiaBarFillAmmount;
    private float paranoia = 0f;

    float lerpTime = 5f; // Tiempo que tarda la "animación"
    float currentLerpTime;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        paranoiaBarFillAmmount = paranoiaBarImage.fillAmount;

        if (currentLerpTime < lerpTime)
            currentLerpTime+= Time.deltaTime;
        if (currentLerpTime > lerpTime)
            currentLerpTime = lerpTime;

        InterpolateBar();
    }

    void InterpolateBar() 
    {
        float t = currentLerpTime / lerpTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ecuacuión que da un efecto de Ease Out
        paranoiaBarImage.fillAmount = Mathf.Lerp(paranoiaBarFillAmmount, paranoia/100, t);
    }

    public void AddParanoia(float value) 
    {
        currentLerpTime = 0;
        paranoia += value;
        if (paranoia > 100f)
            paranoia = 100f;
    }

}
