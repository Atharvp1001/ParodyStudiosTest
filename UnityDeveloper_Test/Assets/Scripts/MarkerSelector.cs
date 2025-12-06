using System.Collections;
using UnityEngine;

public class MarkerSelector : MonoBehaviour
{
    public GameObject markerTop;
    public GameObject markerLeft;
    public GameObject markerRight;

    public Transform worldRoot;
    public float rotationDuration = 0.7f;

    // Rotation amounts YOU choose for each marker
    public Vector3 rotateTop = new Vector3(180, 0, 0);   // example
    public Vector3 rotateLeft = new Vector3(0, 0, -90);   // example
    public Vector3 rotateRight = new Vector3(0, 0, 90);    // example

    public enum Selection { None, Top, Left, Right }
    Selection selection = Selection.None;

    bool rotating = false;

    void Start()
    {
        HideAll();
    }

    void Update()
    {
        if (rotating) return;

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

        if (s == Selection.Top) markerTop.SetActive(true);
        if (s == Selection.Left) markerLeft.SetActive(true);
        if (s == Selection.Right) markerRight.SetActive(true);
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

        GameObject marker = null;
        Vector3 euler = Vector3.zero;

        if (selection == Selection.Top)
        {
            marker = markerTop;
            euler = rotateTop;
        }
        else if (selection == Selection.Left)
        {
            marker = markerLeft;
            euler = rotateLeft;
        }
        else if (selection == Selection.Right)
        {
            marker = markerRight;
            euler = rotateRight;
        }

        if (marker != null)
            StartCoroutine(Rotate(worldRoot, marker.transform.position, euler));

        selection = Selection.None;
        HideAll();
    }

    IEnumerator Rotate(Transform target, Vector3 pivot, Vector3 eulerDelta)
    {
        rotating = true;

        Quaternion startRot = target.rotation;
        Vector3 startPos = target.position;

        Quaternion delta = Quaternion.Euler(eulerDelta);
        Quaternion finalRot = delta * startRot;

        float t = 0;
        while (t < rotationDuration)
        {
            float s = Mathf.SmoothStep(0, 1, t / rotationDuration);
            Quaternion step = Quaternion.Slerp(Quaternion.identity, delta, s);

            target.rotation = step * startRot;
            target.position = pivot + step * (startPos - pivot);

            t += Time.deltaTime;
            yield return null;
        }

        target.rotation = finalRot;
        target.position = pivot + delta * (startPos - pivot);

        // Snap to clean 0/90/180/270 angles
        Vector3 snap = target.eulerAngles;
        snap.x = Mathf.Round(snap.x / 90f) * 90f;
        snap.y = Mathf.Round(snap.y / 90f) * 90f;
        snap.z = Mathf.Round(snap.z / 90f) * 90f;
        target.rotation = Quaternion.Euler(snap);

        rotating = false;
    }
}
