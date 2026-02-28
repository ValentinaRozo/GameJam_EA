using UnityEngine;

public class BallImpactSound : MonoBehaviour
{
    public AudioSource impactAudio;
    public float minImpactForce = 2f; // evita sonidos por micro-roces

    private void Awake()
    {
        if (impactAudio == null)
            impactAudio = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Solo si golpea algo con suficiente fuerza
        if (collision.relativeVelocity.magnitude > minImpactForce)
        {
            impactAudio.Play();
        }
    }
}