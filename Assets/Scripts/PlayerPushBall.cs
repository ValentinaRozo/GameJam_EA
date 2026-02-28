using UnityEngine;

public class PlayerPushBall : MonoBehaviour
{
    public float pushForce = 2f;
    public LayerMask ballLayer; // aquí eliges "Ball" en el inspector

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Solo si es la pelota (layer)
        if (((1 << hit.gameObject.layer) & ballLayer) == 0)
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;

        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}