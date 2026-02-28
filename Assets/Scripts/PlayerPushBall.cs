using UnityEngine;

public class PlayerPushBall : MonoBehaviour
{
    public float pushForce = 2f;
    public LayerMask ballLayer;

    private PlayerSpaceController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerSpaceController>();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & ballLayer) == 0) return;

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        BallPhysics ball = hit.collider.GetComponent<BallPhysics>();
        if (ball != null && playerController != null)
            ball.RegisterTouch(playerController.teamID);

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}