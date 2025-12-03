using System.Linq;
using UnityEngine;

public class ConnectToStar : MonoBehaviour
{
    public GameObject player;

    public float lifeTime = 5f;
    private float startTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > lifeTime)
        {
            Destroy(this.gameObject);
		}
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Star"))
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
            {
				pm.connectedStar = collision.gameObject;
				pm.snakeLine.SetPosition(0, player.transform.position);
                pm.snakeLine.SetPosition(1, collision.transform.position);
                pm.snakeLine.startColor = Color.green;
                pm.snakeLine.startWidth = 0.1f;
                pm.snakeLine.endColor = Color.green;
                pm.snakeLine.endWidth = 0.1f;
				pm.snakeLine.enabled = true;
			}

			Destroy(this.gameObject);
		}
	}
}
