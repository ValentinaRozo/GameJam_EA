using UnityEngine;

public class PlayerPass : MonoBehaviour
{
    public BallPhysics ball;
    public Transform teammate;

    public float passRange = 2f;
    public float passForce = 6f;

    void Update()
    {
        // Usa Space si quieres
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPassToTeammate();
        }
    }

    void TryPassToTeammate()
    {
        if (ball == null || teammate == null) return;

        // Solo si la pelota está cerca del jugador
        if (Vector3.Distance(transform.position, ball.transform.position) > passRange)
            return;

        Vector3 dir = (teammate.position - ball.transform.position);
        if (dir.sqrMagnitude < 0.01f) return;

        ball.Impulse(dir, passForce);
    }
}
