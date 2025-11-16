using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public Rope ropePrefab;

    [Header("Stars in this level (assign in inspector)")]
    public List<Star> stars = new List<Star>();

    [Header("Connections (runtime)")]
    public List<Rope> ropes = new List<Rope>();

    [Header("Validation")]
    public ConstellationValidator validator;

    public Rope CreateRope(Star a, Star b)
    {
        // If rope already exists, do nothing
        foreach (var r in ropes)
            if (r.Connects(a, b)) return r;

        Rope instance = Instantiate(ropePrefab, Vector3.zero, Quaternion.identity);
        instance.name = $"Rope_{a.starId}_{b.starId}";
        instance.Init(a, b);
        ropes.Add(instance);
        return instance;
    }

    public void RemoveRope(Star a, Star b)
    {
        Rope toRemove = null;
        foreach (var r in ropes)
            if (r.Connects(a, b)) { toRemove = r; break; }
        if (toRemove)
        {
            ropes.Remove(toRemove);
            Destroy(toRemove.gameObject);
        }
        OnConnectionsChanged();
    }

    public void OnConnectionCreated(Star a, Star b)
    {
        OnConnectionsChanged();
    }

    void OnConnectionsChanged()
    {
        // Build current edge set from ropes
        HashSet<(int,int)> edges = new HashSet<(int,int)>();
        foreach (var r in ropes)
        {
            int x = Mathf.Min(r.A.starId, r.B.starId);
            int y = Mathf.Max(r.A.starId, r.B.starId);
            edges.Add((x, y));
        }

        var result = validator.Validate(edges);
        if (result.state == ConstellationValidator.State.CorrectComplete)
        {
            Debug.Log(" Constellation complete!");
            // TODO: Show UI “You formed Aries!”
        }
        else if (result.state == ConstellationValidator.State.HasForbidden)
        {
            Debug.Log($" Extra rope detected: {result.message}");
            // TODO: Show a UI text box telling the player to remove the extra rope
        }
        else
        {
            // Incomplete or other issues → do nothing special
        }
    }
}
