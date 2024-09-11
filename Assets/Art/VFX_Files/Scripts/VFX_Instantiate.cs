using UnityEngine;

public class VFX_Instantiate : MonoBehaviour
{
    [SerializeField] private GameObject VFX ;
    [SerializeField] private int rows = 10 ;

    [SerializeField] private int columns = 10 ;
    [SerializeField] private float offsetStep = 1.0f ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int y = 0; y < columns ; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                Instantiate(VFX, new Vector3(x * offsetStep, y *offsetStep, 0), Quaternion.identity);
            }
        }
    }

}
