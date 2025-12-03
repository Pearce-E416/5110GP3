using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

	public Transform target; // The target (player) to follow
	public float smoothSpeed = 0.125f; // Speed of smoothing
	public Vector3 offset; // Offset from the target

	public bool tendCenter;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {
        
    }
	void LateUpdate()
	{
		Vector3 desiredPosition = target.position + offset;
		if (tendCenter)
		{
			desiredPosition = Vector3.Lerp(desiredPosition, Vector3.zero, 0.2f);
		}
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;
	}
}
