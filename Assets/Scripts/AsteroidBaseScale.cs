using UnityEngine;

// Guarda la escala original del asteroide para que el escalado por gol
// se multiplique sobre ella y no se pierda la variación aleatoria
public class AsteroidBaseScale : MonoBehaviour
{
    [HideInInspector] public float originalScale = 1f;
}
