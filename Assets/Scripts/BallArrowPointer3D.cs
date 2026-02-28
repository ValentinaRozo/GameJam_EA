using UnityEngine;

public class BallArrowPointer3D : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Transform ball;

    [Header("Posición relativa al jugador")]
    public Vector3 offset = new Vector3(0, 2f, 0);

    [Header("Suavizado")]
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        if (ball == null || player == null) return;

        // Mantener la flecha encima del jugador
        transform.position = player.position + offset;

        // Dirección hacia el balón
        Vector3 direction = ball.position - transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        // Rotación objetivo
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Rotación suave
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}