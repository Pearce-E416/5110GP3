using UnityEngine;

public class WinCheck : MonoBehaviour
{
    public GameObject[] starDestinations;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //PerformCheck();
    }

    public void PerformCheck()
    {
        Debug.Log("Performing Win Check...");
		foreach (GameObject starDestination in starDestinations)
        {
            StarDestination sd = starDestination.GetComponent<StarDestination>();
            if (sd != null)
            {
                if (sd.currentConnections.Count != sd.correctConnections.Count)
                {
                    return;
				}

				foreach (GameObject correctConnection in sd.correctConnections)
                {
                    if (!sd.currentConnections.Contains(correctConnection))
                    {
                        return;
                    }
                }
            }
        }
        Debug.Log("You Win!");
	}
}
