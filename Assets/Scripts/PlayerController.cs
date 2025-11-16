using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerController : MonoBehaviour
{
    public enum Surface { Star, Rope }

    [Header("Refs")]
    public Star currentStar;
    public GameManager gameManager;
    public TrajectoryPreview trajectoryPreview;

    [Header("Orbit Movement")]
    public float orbitSpeed = 60f;
    public float orbitRadius = 1.2f;

    [Header("Throw")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 20f;
    public float chargeTime = 1.0f;
    public LayerMask starMask; // set in Inspector

    [Header("State (debug)")]
    public Surface surface = Surface.Star;
    public Rope currentRope;
    private float ropeT = 0f;
    private float chargeTimer = 0f;
    private bool charging = false;

    // Rope-boarding (keyboard)
    private Rope ropeCandidate; // rope we're currently overlapping (near the star)

    private Vector2 gravity => Physics2D.gravity;

    // Aiming/highlight
    private Rope aimedRope;                 // currently highlighted
    [SerializeField] private float aimConeDegrees = 30f; // same threshold used for deletion



    [Header("Start Position")]
    [Tooltip("0 = right, 90 = up, 180 = left, 270 = down")]
    public float startAngleDegrees = 0f;

    Vector2 AngleToDir(float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(r), Mathf.Sin(r));
    }

    void PlaceOnStar(Star star, Vector2 outwardDir)
    {
        if (!star) return;
        Vector2 d = outwardDir.sqrMagnitude > 1e-6f ? outwardDir.normalized : Vector2.right;
        transform.position = star.Position2D + d * orbitRadius;
    }

    void Start()
    {
        if (currentStar != null)
        {
            // Use Inspector angle instead of forcing Vector2.right
            Vector2 dir = AngleToDir(startAngleDegrees);
            PlaceOnStar(currentStar, dir);
        }   
    }

    void Update()
    {
        if (surface == Surface.Star) UpdateOnStar();
        else UpdateOnRope();

        HandleThrowInput();

        // Keyboard boarding: press R to enter the overlapping rope connected to this star
        if (surface == Surface.Star && ropeCandidate != null && InputShim.GetKeyDownR())
        {
            if (ropeCandidate.A == currentStar) EnterRope(ropeCandidate, 0.02f);
            else if (ropeCandidate.B == currentStar) EnterRope(ropeCandidate, 0.98f);
            // if neither end is this star, ignore (shouldn't happen with our filter)
        }

        UpdateAimHighlight();
        // --- Rope removal ---
        if (surface == Surface.Star && InputShim.GetKeyDownX())
        {
        // Prefer the explicitly highlighted rope if present
            if (aimedRope != null)
            {
                gameManager.RemoveRope(aimedRope.A, aimedRope.B);
                ClearAimHighlight();
            }
            else
            {
                // Fallback: attempt aimed removal using cone (if you kept TryRemoveAimedRope)
                TryRemoveAimedRope();
            }
        }

        /*
        if (surface == Surface.Star && InputShim.GetKeyDownX())
        {
            TryRemoveAimedRope();
        }
        */
    }

    void UpdateOnStar()
    {
        if (currentStar == null) return;

        float input = InputShim.GetHorizontal();
        float deltaAngle = input * orbitSpeed * Mathf.Deg2Rad * Time.deltaTime;

        Vector2 center = currentStar.Position2D;
        Vector2 fromCenter = (Vector2)transform.position - center;
        if (fromCenter.sqrMagnitude < 0.0001f)
            fromCenter = Vector2.right * orbitRadius;

        float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) + deltaAngle;
        Vector2 newPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
        transform.position = newPos;
    }

    void UpdateOnRope()
    {
        if (!currentRope) { surface = Surface.Star; return; }

        float input = InputShim.GetHorizontal();
        float speedAlong = 0.5f;
        ropeT = Mathf.Clamp01(ropeT + input * speedAlong * Time.deltaTime);

        Vector3 p0 = currentRope.A.transform.position;
        Vector3 p1 = currentRope.B.transform.position;
        transform.position = Vector3.Lerp(p0, p1, ropeT);
        if (ropeT <= 0.0f)
        {
        // Land on A in the direction of the rope (A -> B)
        Vector2 dirToB = (currentRope.B.Position2D - currentRope.A.Position2D).normalized;
        currentStar = currentRope.A;
        surface = Surface.Star;
        currentRope = null;
        PlaceOnStar(currentStar, dirToB);    // snap at rope contact direction
        }
        else if (ropeT >= 1.0f)
        {
        // Land on B in the direction of the rope (B -> A)
        Vector2 dirToA = (currentRope.A.Position2D - currentRope.B.Position2D).normalized;
        currentStar = currentRope.B;
        surface = Surface.Star;
        currentRope = null;
        PlaceOnStar(currentStar, dirToA);    // snap at rope contact direction
        }
    }

    void HandleThrowInput()
    {
        Vector3 mouseWorld = InputShim.GetMouseWorld(Camera.main, transform.position.z);
        Vector2 aimDir = (mouseWorld - transform.position);
        aimDir.Normalize();

        if (InputShim.MouseJustPressed())
        {
            charging = true;
            chargeTimer = 0f;
        }

        if (charging)
        {
            chargeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(chargeTimer / chargeTime);
            float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);
            trajectoryPreview?.Show((Vector2)transform.position, aimDir * speed, gravity, starMask);
        }

        if (charging && InputShim.MouseJustReleased())
        {
            charging = false;
            trajectoryPreview?.Hide();

            float t = Mathf.Clamp01(chargeTimer / chargeTime);
            float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);
            TryThrow(aimDir, speed);
        }
    }

    void TryThrow(Vector2 dir, float speed)
    {
    float totalTime = 3.5f;
    float step = 0.025f;
    float detectRadius = 0.15f;
    Vector2 start = transform.position;

    Vector2 prev = start;

    for (float time = step; time <= totalTime; time += step)
    {
        Vector2 pos = start + dir * speed * time + 0.5f * gravity * (time * time);
        Vector2 delta = pos - prev;
        float dist = delta.magnitude;
        Vector2 rayDir = dist > 0f ? delta / dist : Vector2.right;

        // 1) Try a CircleCast along this segment
        RaycastHit2D castHit = Physics2D.CircleCast(prev, detectRadius, rayDir, dist, starMask);
        Star hitStar = null;

        if (castHit.collider != null)
        {
            hitStar = castHit.collider.GetComponent<Star>();
        }
        else
        {
            // 2) Fallback: OverlapCircle at the sample point
            Collider2D ov = Physics2D.OverlapCircle(pos, detectRadius, starMask);
            if (ov != null) hitStar = ov.GetComponent<Star>();
        }

        if (hitStar != null && hitStar != currentStar)
        {
            Rope rope = gameManager.CreateRope(currentStar, hitStar);
            currentRope = rope;
            ropeT = 0.02f;
            surface = Surface.Rope;
            gameManager.OnConnectionCreated(currentStar, hitStar);
            return;
        }

        prev = pos;
    }

    // no hit → do nothing
    }


    public void EnterRope(Rope rope, float t)
    {
        currentRope = rope;
        ropeT = Mathf.Clamp01(t);
        surface = Surface.Rope;
    }

    void TryRemoveAimedRope()
    {
        if (currentStar == null || gameManager == null) return;

        // Mouse aim direction from the current star’s center
        Vector3 mouseWorld = InputShim.GetMouseWorld(Camera.main, transform.position.z);
        Vector2 aimDir = ((Vector2)mouseWorld - currentStar.Position2D).normalized;
        if (aimDir.sqrMagnitude < 1e-5f) return;

        Rope best = null;
        float bestAngle = float.MaxValue;

        // Angle threshold: only ropes within this angle cone are eligible
        const float angleThresholdDeg = 30f;

        // Search all ropes connected to this star and pick the one closest to our aim
        foreach (var rope in gameManager.ropes)
        {
            if (!rope) continue;

            bool touches =
                rope.A == currentStar ||
                rope.B == currentStar;

            if (!touches) continue;

            // Direction from this star to the other end of the rope
            Star other = (rope.A == currentStar) ? rope.B : rope.A;
            if (!other) continue;

            Vector2 toOther = (other.Position2D - currentStar.Position2D).normalized;
            float angle = Vector2.Angle(aimDir, toOther); // unsigned 0..180

            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = rope;
            }

            // Optional: visualize candidates
            Debug.DrawLine(currentStar.Position2D, other.Position2D, Color.magenta, 0.25f);
        }

    // Only remove if we are actually aiming “close enough” to a connected rope
        if (best != null && bestAngle <= angleThresholdDeg)
        {
            // Small visual confirmation
            Vector2 from = currentStar.Position2D;
            Vector2 to = (best.A == currentStar ? best.B.Position2D : best.A.Position2D);
            Debug.DrawLine(from, to, Color.red, 0.35f);

            gameManager.RemoveRope(best.A, best.B);
            // Optionally: show UI “Rope removed”
        }
        else
        {
            // Optionally: UI feedback “No rope in aim cone”
            // Debug.Log("No connected rope in aim direction.");
        }
    }

    void UpdateAimHighlight()
    {
        // Only highlight while on a star
        if (surface != Surface.Star || currentStar == null || gameManager == null)
        {
            ClearAimHighlight();
            return;
        }

        // Aim direction from star center toward mouse
        Vector3 mouseWorld = InputShim.GetMouseWorld(Camera.main, transform.position.z);
        Vector2 aimDir = ((Vector2)mouseWorld - currentStar.Position2D).normalized;
        if (aimDir.sqrMagnitude < 1e-5f)
        {
            ClearAimHighlight();
            return;
        }

        Rope best = null;
        float bestAngle = float.MaxValue;

        foreach (var rope in gameManager.ropes)
        {
            if (!rope) continue;
            if (rope.A != currentStar && rope.B != currentStar) continue; // must touch this star

            Star other = (rope.A == currentStar) ? rope.B : rope.A;
            if (!other) continue;

            Vector2 toOther = (other.Position2D - currentStar.Position2D).normalized;
            float angle = Vector2.Angle(aimDir, toOther);

            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = rope;
            }

            // Optional: draw all candidates briefly
            Debug.DrawLine(currentStar.Position2D, other.Position2D, Color.magenta, 0f);
        }

        // Apply highlight if within cone
        if (best != null && bestAngle <= aimConeDegrees)
        {
            if (aimedRope != best)
            {
                // swap highlight
                if (aimedRope) aimedRope.SetHighlighted(false);
                aimedRope = best;
                aimedRope.SetHighlighted(true);
            }
        }
        else
        {
            ClearAimHighlight();
        }
    }

    void ClearAimHighlight()
    {
        if (aimedRope)
        {
            aimedRope.SetHighlighted(false);
            aimedRope = null;
        }
    }


    /*
        void TryRemoveConnectedRope()
        {
            if (currentStar == null) return;

            // Look for all ropes connected to this star
            foreach (var rope in gameManager.ropes)
            {
                if (rope == null) continue;

                if (rope.A == currentStar || rope.B == currentStar)
                {
                    // Remove only the *forbidden* ones or the last created if you prefer
                    gameManager.RemoveRope(rope.A, rope.B);
                    Debug.Log($"Rope removed between {rope.A.starId} and {rope.B.starId}");
                    return; // remove just one per key press
                }
            }
        }
    */


    // ---------- Trigger detection for "press R to board" ----------
    // Player must have Rigidbody2D (Kinematic is fine) + CircleCollider2D (IsTrigger = true)
    // Rope EdgeCollider2D must be IsTrigger = true
    void OnTriggerStay2D(Collider2D other)
    {
        // Only consider Rope layer overlaps
        if (other.gameObject.layer != LayerMask.NameToLayer("Rope"))
            return;

        var rope = other.GetComponent<Rope>();
        if (!rope) return;

        // Only allow boarding ropes that are connected to the current star
        if (currentStar != null && (rope.A == currentStar || rope.B == currentStar))
        {
            ropeCandidate = rope;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var rope = other.GetComponent<Rope>();
        if (rope && rope == ropeCandidate)
            ropeCandidate = null;
    }
}

static class InputShim
{
    public static float GetHorizontal()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        float v = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) v += 1f;
        }
        return Mathf.Clamp(v, -1f, 1f);
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    public static bool MouseJustPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    public static bool MouseJustReleased()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }

    public static bool GetKeyDownR()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    public static bool GetKeyDownX()
    {
    #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame;
    #else
        return Input.GetKeyDown(KeyCode.X);
    #endif
    }


    public static Vector3 GetMouseWorld(Camera cam, float zPlane)
    {
        if (cam == null) return Vector3.zero;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Vector2 screen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z - zPlane)));
        world.z = 0f;
        return world;
#else
        Vector3 screen = Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z - zPlane)));
        world.z = 0f;
        return world;
#endif
    }
}
