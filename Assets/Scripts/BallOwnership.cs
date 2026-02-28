using UnityEngine;

public class BallOwnership : MonoBehaviour
{
    public Transform holder;   // quien “posee” la pelota (puede ser null)
    public float holdDistance = 1.2f;

    public bool IsHeldBy(Transform t) => holder == t;

    public void SetHolder(Transform t)
    {
        holder = t;
    }

    public void ClearHolder(Transform t)
    {
        if (holder == t) holder = null;
    }
}