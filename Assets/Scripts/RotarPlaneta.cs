using UnityEngine;

public class RotarPlaneta : MonoBehaviour
{
    [Header("Grados por segundo")]
    public float velocidad = 15f;

    [Header("Eje local del planeta (1,0,0)=X, (0,1,0)=Y, (0,0,1)=Z")]
    public Vector3 ejeLocal = Vector3.up;

    void Update()
    {
        // Rota suavemente sobre su propio eje (local)
        transform.Rotate(ejeLocal.normalized, velocidad * Time.deltaTime, Space.Self);
    }
}