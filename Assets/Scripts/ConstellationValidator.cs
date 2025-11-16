using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConstellationValidator : MonoBehaviour
{
    public enum State { Incomplete, HasForbidden, CorrectComplete }

    [Header("Constellation Rules (use star IDs)")]
    public List<Vector2Int> requiredEdges = new List<Vector2Int>(); // e.g., (1,2), (2,3)
    public List<Vector2Int> forbiddenEdges = new List<Vector2Int>(); // e.g., (1,3)

    public struct Result
    {
        public State state;
        public string message;
    }

    public Result Validate(HashSet<(int,int)> currentEdges)
    {
        // Normalize vectors to (min,max)
        HashSet<(int,int)> req = new HashSet<(int,int)>();
        foreach (var e in requiredEdges)
            req.Add((Mathf.Min(e.x, e.y), Mathf.Max(e.x, e.y)));

        HashSet<(int,int)> forb = new HashSet<(int,int)>();
        foreach (var e in forbiddenEdges)
            forb.Add((Mathf.Min(e.x, e.y), Mathf.Max(e.x, e.y)));

        // Forbidden present?
        foreach (var e in currentEdges)
        {
            if (forb.Contains(e))
                return new Result { state = State.HasForbidden, message = $"Forbidden link {e.Item1}-{e.Item2}" };
        }

        // All required present?
        foreach (var e in req)
        {
            if (!currentEdges.Contains(e))
                return new Result { state = State.Incomplete, message = $"Missing required link {e.Item1}-{e.Item2}" };
        }

        // If we want to ALSO block any other extra-but-not-forbidden edges, you can enforce exact match:
        // if (currentEdges.Except(req).Any()) return new Result { state = State.HasForbidden, message = "Remove extra links." };

        return new Result { state = State.CorrectComplete, message = "All good." };
    }
}
