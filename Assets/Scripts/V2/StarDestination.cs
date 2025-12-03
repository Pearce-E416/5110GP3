using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class StarDestination : MonoBehaviour
{
    public List<GameObject> correctConnections;
	public List<GameObject> currentConnections;

    private bool isOccupied = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	private void OnTriggerEnter2D(Collider2D collision)
	{
        StarV2 s = collision.gameObject.GetComponent<StarV2>();
		if (s != null && !s.isLocked && !isOccupied)
        {
            Vector3 dest = transform.position;
            dest.z = 0;
            collision.gameObject.transform.position = dest;
            s.isLocked = true;
            s.destination = this.gameObject;
            isOccupied = true;
			Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
			}
		}
	}

    public bool AddConnection(GameObject target)
    {
        if (!currentConnections.Contains(target))
        {
			currentConnections.Add(target);
            StarDestination sdTarget = target.GetComponent<StarDestination>();
            if (sdTarget != null)
            {
                sdTarget.currentConnections.Add(this.gameObject);
            }
			var lineObject = new GameObject();
            LineRenderer lr = lineObject.AddComponent<LineRenderer>();
            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, target.transform.position);
            if (correctConnections.Contains(target))
            {
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
            }
            else
            {
                lr.startColor = Color.green;
				lr.endColor = Color.green;
			}
            lr.startWidth = 0.1f;
			lr.endWidth = 0.1f;
			lr.material = new Material(Shader.Find("Sprites/Default"));
			return true;
        }
        return false;
    }
}
