using System.Collections;
using UnityEngine;

public class MarkerSelector : MonoBehaviour
{
    // Public flag the player script reads
    public static bool IsRotating = false;

    public GameObject markerTop;
    public GameObject markerLeft;
    public GameObject markerRight;

    public Transform worldRoot;             // parent of level geometry (player MUST NOT be a child)
    public float rotationDuration = 0.7f;

    public Camera cam;                      // used to determine left/right based on camera facing

    public enum Selection { None, Top, Left, Right }
    Selection selection = Selection.None;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        HideAll();
    }

    void Update()
    {
        if (IsRotating) return; // don't accept new inputs while rotating

        if (Input.GetKeyDown(KeyCode.UpArrow)) Select(Selection.Top);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Select(Selection.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Select(Selection.Right);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ApplyRotation();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selection = Selection.None;
            HideAll();
        }
    }

    void Select(Selection s)
    {
        selection = s;
        HideAll();

        if (s == Selection.Top && markerTop) markerTop.SetActive(true);
        if (s == Selection.Left && markerLeft) markerLeft.SetActive(true);
        if (s == Selection.Right && markerRight) markerRight.SetActive(true);
    }

    void HideAll()
    {
        if (markerTop) markerTop.SetActive(false);
        if (markerLeft) markerLeft.SetActive(false);
        if (markerRight) markerRight.SetActive(false);
    }

    void ApplyRotation()
    {
        if (selection == Selection.None) return;
        if (worldRoot == null) return;

        Vector3 eulerDelta = Vector3.zero;

        if (selection == Selection.Top)
        {
            // ALWAYS 180° on X for Top
            eulerDelta = new Vector3(180f, 0f, 0f);
        }
        else if (selection == Selection.Left)
        {
            // compute camera-relative left rotation (camera.right gives right; invert for left)
            eulerDelta = ComputeCameraBasedRotation(-cam.transform.right);
        }
        else if (selection == Selection.Right)
        {
            eulerDelta = ComputeCameraBasedRotation(cam.transform.right);
        }

        // Important: rotate around a sensible pivot. Using worldRoot.position keeps behavior consistent.
        // If you prefer rotating around a marker point (markerTop/Left/Right), change pivot accordingly.
        StartCoroutine(RotateWorld(worldRoot, worldRoot.position, eulerDelta));

        selection = Selection.None;
        HideAll();
    }

    // camera-based discretization for left/right: maps camera direction to nearest cardinal and returns Euler delta
    Vector3 ComputeCameraBasedRotation(Vector3 dir)
    {
        dir.y = 0f;
        dir.Normalize();

        float dotF = Vector3.Dot(dir, Vector3.forward);
        float dotB = Vector3.Dot(dir, Vector3.back);
        float dotR = Vector3.Dot(dir, Vector3.right);
        float dotL = Vector3.Dot(dir, Vector3.left);

        float max = Mathf.Max(dotF, dotB, dotR, dotL);

        if (max == dotF) return new Vector3(90f, 0f, 0f);   // camera-facing forward -> rotate X 90
        if (max == dotB) return new Vector3(-90f, 0f, 0f);  // backward
        if (max == dotR) return new Vector3(0f, 0f, -90f);  // right -> make right down
        return new Vector3(0f, 0f, 90f);                    // left -> make left down
    }

    IEnumerator RotateWorld(Transform target, Vector3 pivot, Vector3 eulerDelta)
    {
        // Safety checks
        if (target == null) yield break;

        // warn about non-uniform scale - this breaks pivot math
        Vector3 s = target.lossyScale;
        if (Mathf.Abs(s.x - s.y) > 1e-4f || Mathf.Abs(s.x - s.z) > 1e-4f)
            Debug.LogWarning("MarkerSelector: worldRoot (or parent) has non-uniform scale. Set to (1,1,1) for reliable pivot rotation.");

        // ensure player isn't child of worldRoot (very important)
        // (We can't automatically reparent because that may cause other issues; just warn)
        // Start rotation
        IsRotating = true;

        Quaternion startRot = target.rotation;
        Quaternion delta = Quaternion.Euler(eulerDelta);
        Quaternion finalRot = delta * startRot;

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
            Quaternion step = Quaternion.Slerp(Quaternion.identity, delta, t);

            target.rotation = step * startRot;
            // Keep pivot fixed in world space:
            target.position = pivot + step * (target.position - pivot);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // final exact apply
        target.rotation = finalRot;
        target.position = pivot + finalRot * (target.position - pivot);

        // snap to clean 90-degree multiples for inspector/readability
        Vector3 snap = target.eulerAngles;
        snap.x = Mathf.Round(snap.x / 90f) * 90f;
        snap.y = Mathf.Round(snap.y / 90f) * 90f;
        snap.z = Mathf.Round(snap.z / 90f) * 90f;
        target.rotation = Quaternion.Euler(snap);
        target.position = pivot + target.rotation * (target.position - pivot);

        // small wait of one frame to ensure physics settle (optional but helps)
        yield return null;

        IsRotating = false;
    }
}
