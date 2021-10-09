using System;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform following;
    public Vector3 orginOffset;
    public Vector3 orginOffsetWithRotation;
    public Vector2 clipPlaneOffset;
    public float distance;
    public float sensitivity;
    public Vector2 rotXMinMax;

    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // LateUpdate is called once per frame at the end of everything
    void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Cursor.visible) {
            if (Input.GetMouseButton(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else {
            float rotMod = transform.rotation.eulerAngles.x <= 90 ? transform.rotation.eulerAngles.x + 360 : transform.rotation.eulerAngles.x;
            transform.rotation = Quaternion.Euler(Mathf.Clamp(rotMod - Input.GetAxis("Mouse Y") * sensitivity, rotXMinMax.x, rotXMinMax.y),
                transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);
        }

        //Info provided is Camera FoV/Aspect Ratio/Near Clip Plane, get topleft of Near Clip Plane's local position in rectagular cordinates
        float calc1 = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * cam.nearClipPlane + clipPlaneOffset.x;    //see if this can be called only if smth changes to save on calcs
        float calc2 = calc1 * cam.aspect + clipPlaneOffset.x;
        float calc3 = cam.nearClipPlane - clipPlaneOffset.y;
        float calcDist = distance;
        //Vector3 calVec = new Vector3(calc2, calc1, calc3);
        //Vector3 cenOfst = new Vector3(0.1f, 0.1f, 0.1f);
        int layer = ~(1 << LayerMask.NameToLayer("Player"));
        RaycastHit ray;

        Vector3 orginOfAll = following.position + orginOffset + transform.rotation * orginOffsetWithRotation;

        //follow.pos + orgOfst + Vec(0.1 * Pos/Neg(orgOfst.x), 0.1 * Pos/Neg(orgOfst.y), 0)     Pos/Neg() uses bitwise to find signed (but probs not cuz float)
		//follow.pos + orgOfst + Vec(0.1 * vec.x, 0.1 * vec.y, 0)	vec is (1, 1, 1)
        //getting Hypotnuse dist
        foreach (Vector3 vec in new Vector3[4] { new Vector3(calc2, calc1, calc3), new Vector3(-calc2, calc1, calc3), new Vector3(calc2, -calc1, calc3), new Vector3(-calc2, -calc1, calc3) })
            if (Physics.Linecast(orginOfAll, orginOfAll + transform.rotation * (Vector3.back * distance + vec), out ray, layer) && ray.distance < calcDist)
                calcDist = ray.distance;

        following.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.position = orginOfAll - transform.forward * (Mathf.Cos(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * calcDist);
    }
}
