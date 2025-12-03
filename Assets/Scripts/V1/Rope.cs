using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
public class Rope : MonoBehaviour
{
    public Star A;
    public Star B;

    [Header("Visuals")]
    public float normalWidth = 0.05f;
    public float highlightWidth = 0.09f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    LineRenderer lr;
    EdgeCollider2D edge;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        edge = GetComponent<EdgeCollider2D>();
        // Defaults
        if (lr != null)
        {
            lr.widthMultiplier = normalWidth;
            lr.startColor = lr.endColor = normalColor;
        }
        if (edge != null) edge.isTrigger = true;
    }


    public void Init(Star a, Star b)
    {
        A = a; B = b;
        lr = GetComponent<LineRenderer>();
        edge = GetComponent<EdgeCollider2D>();

        Vector3 p0 = a.transform.position;
        Vector3 p1 = b.transform.position;

        lr.positionCount = 2;
        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);
        /*
                // Edge collider needs 2D points
                Vector2[] pts = new Vector2[2] { p0, p1 };
                edge.points = pts;
                //edge.usedByComposite = false;
                edge.isTrigger = true; // we’ll handle “standing” logically

                // Optional: put on Rope layer
                gameObject.layer = LayerMask.NameToLayer("Rope");
        */
        edge.points = new Vector2[2] { p0, p1 };

    }
    public void SetHighlighted(bool on)
    {
        if (!lr) return;
        lr.widthMultiplier = on ? highlightWidth : normalWidth;
        lr.startColor = lr.endColor = on ? highlightColor : normalColor;
    }

    public bool Connects(Star x, Star y)
    {
        return (A == x && B == y) || (A == y && B == x);
    }
}
