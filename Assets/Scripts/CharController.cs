using System;
using UnityEngine;

struct Inputs {
    public Vector2 axis;
    public Vector2 tempAxis;
    public uint framesPassed;
    public byte jump;
}

public class CharController : MonoBehaviour {
    public float moveForce = 1400f;
    public float jumpForce = 30f;
    public float gravMult = 9.81f;
    public float rotateArmature = 0f;
    public float jumpCheckYOffset = 0.52f;
    public float jumpCheckRadOffset = 0.975f;
    public float velAnimCutoff = 0.1f;
    public float velAnimSpeedMulti = 0.6f;

    private Inputs curInputs;
    private Rigidbody RB3D;
    private Collider col;
    private Animator anim;
    private float sqrt2 = 0;
    private bool grounded = false;

	// Start is called before the first frame update
	void Start() {
		anim = GetComponent<Animator>();
		RB3D = gameObject.GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        sqrt2 = 1f / Mathf.Sqrt(2);         //sqrt is a fairly intensive operation, storing it in memory to avoid using opertaion every fixed update

        transform.GetChild(1).transform.Rotate(0f, 0f, rotateArmature);
    }

	// Update is called once per frame
	void Update() {
		if (Cursor.visible)
			return;

		//Store input from each update to be considered for fixed updates, dont do needless addition of 0 if unneeded
		curInputs.tempAxis.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		if (curInputs.tempAxis.x != 0f) curInputs.axis.x += curInputs.tempAxis.x;
		if (curInputs.tempAxis.y != 0f) curInputs.axis.y += curInputs.tempAxis.y;
        if (Input.GetButtonDown("Jump") && curInputs.jump == 0 && grounded) curInputs.jump = 5;
        ++curInputs.framesPassed;
    }

    private void FixedUpdate() {
        grounded = isGrounded();

        Vector2 horzVel = new Vector2(RB3D.velocity.x, RB3D.velocity.z);
        anim.SetInteger("AnimatorState", Convert.ToInt32(horzVel.magnitude > velAnimCutoff));
        anim.SetFloat("WalkSpeed", horzVel.magnitude * velAnimSpeedMulti);

        //if both inputs were pressed then semi-normalize inputs
        if (curInputs.axis.x != 0f && curInputs.axis.y != 0f)
            curInputs.axis.Set(curInputs.axis.x * sqrt2, curInputs.axis.y * sqrt2);

		if (curInputs.axis.x != 0f) {
            RB3D.AddForce(transform.right * moveForce * curInputs.axis.x / curInputs.framesPassed * Time.fixedDeltaTime);
            curInputs.axis.x = 0f;
		}

        if (curInputs.axis.y != 0f) {
            RB3D.AddForce(transform.forward * moveForce * curInputs.axis.y / curInputs.framesPassed * Time.fixedDeltaTime);
            curInputs.axis.y = 0f;
		}

        //jump on maxed cooldown
        if (curInputs.jump == 5) {
            RB3D.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            --curInputs.jump;
        }
        
        //reduce jump cooldown if grounded
        if (curInputs.jump > 0 && grounded)
            --curInputs.jump;

        //Add downwards force if ungrounded (RB3D has drag)
        if (!grounded)
            RB3D.AddForce(Physics.gravity * gravMult, ForceMode.Acceleration);

        curInputs.framesPassed = 0;
    }

    bool isGrounded() {
        int layer = ~(1 << LayerMask.NameToLayer("Player"));
        return Physics.CheckSphere(transform.position + (Vector3.down * jumpCheckYOffset), col.bounds.extents.x * jumpCheckRadOffset, layer);
    }
}
