using UnityEngine;

public class BallArrowPointer3D : MonoBehaviour
{
    public Transform player;
    public Transform ball;

    public Vector3 offset = new Vector3(0f, 2f, 0f);

    [Header("Ajuste del modelo")]
    public Vector3 modelRotationOffsetEuler = Vector3.zero;
    // Ejemplos: (0,90,0) o (90,0,0) o (0,180,0)

    public float rotationSpeed = 12f;

    void LateUpdate()
    {
        if (player == null || ball == null) return;

        // Pegada al jugador
        transform.position = player.position + offset;

        // Dirección al balón
        Vector3 dir = ball.position - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        // Rotación mirando al balón
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);

        // Offset por orientación del modelo
        Quaternion modelOffset = Quaternion.Euler(modelRotationOffsetEuler);

        Quaternion target = look * modelOffset;

        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }
}