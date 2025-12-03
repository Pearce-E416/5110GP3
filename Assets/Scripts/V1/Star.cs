using UnityEngine;

public class Star : MonoBehaviour
{
    [Header("Orbit")]
    public float orbitRadiusVisual = 1.2f; // purely visual/helpful

    [Header("ID for Constellation")]
    public int starId; // e.g., 1..N assigned in inspector

    public Vector2 Position2D => (Vector2)transform.position;
}
