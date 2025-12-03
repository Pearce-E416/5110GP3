using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSnake : MonoBehaviour
{
	public GameObject SnakePrefab;

	PlayerInput playerInput;
	InputAction shootAction;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		playerInput = GetComponent<PlayerInput>();
		shootAction = playerInput.actions.FindAction("Point");
	}

    // Update is called once per frame
    void Update()
    {
        Vector2 shootPosition = shootAction.ReadValue<Vector2>();
		if (shootAction.WasPressedThisFrame())
		{
			PlayerMovement pm = GetComponent<PlayerMovement>();
			if (pm == null) return;
			if (pm.snakeHead != null)
			{
				Destroy(pm.snakeHead);
			}
			GameObject currentSnake = Instantiate(SnakePrefab, transform.position, Quaternion.identity);
			Vector3 screenPosition = Camera.main.ScreenToWorldPoint(new Vector3(shootPosition.x, shootPosition.y, transform.position.z));
			Vector3 up = screenPosition - transform.position;
			currentSnake.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);
			up.z = 0;
			currentSnake.GetComponent<Rigidbody2D>().linearVelocity = up.normalized * 10f;
			currentSnake.GetComponent<ConnectToStar>().player = gameObject;

			pm.connectedStar = null;

			pm.snakeHead = currentSnake;
			pm.snakeLine.SetPosition(0, transform.position);
			pm.snakeLine.SetPosition(1, currentSnake.transform.position);
			pm.snakeLine.startColor = Color.green;
			pm.snakeLine.startWidth = 0.1f;
			pm.snakeLine.endColor = Color.green;
			pm.snakeLine.endWidth = 0.1f;
			pm.snakeLine.material = new Material(Shader.Find("Sprites/Default"));
			pm.snakeLine.enabled = true;
		}
	}
}
