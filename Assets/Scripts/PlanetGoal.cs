using UnityEngine;

public class PlanetGoal : MonoBehaviour
{
    public int score = 0;

    void OnTriggerEnter(Collider other)
    {
        BallPhysics ball = other.GetComponent<BallPhysics>();
        if (ball != null)
        {
            score++;
            Debug.Log("¡GOOOL! Score = " + score);
        }
    }
}