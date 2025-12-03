using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryPreview : MonoBehaviour
{
    public int points = 40;
    public float timeStep = 0.05f;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        lr.positionCount = points;
        lr.widthMultiplier = 0.03f;
    }

    public void Show(Vector2 start, Vector2 initialVelocity, Vector2 gravity, LayerMask starMask)
    {
        lr.enabled = true;
        Vector2 pos = start;
        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;
            Vector2 p = start + initialVelocity * t + 0.5f * gravity * (t * t);
            lr.SetPosition(i, p);
        }
    }

    public void Hide()
    {
        lr.enabled = false;
    }
}
