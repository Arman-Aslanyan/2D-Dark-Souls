/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroundDetector
{
    public Vector3 pos = Vector3.zero;
    public Vector3 scale = Vector3.one;
    public LayerMask mask = 2005;

    public GroundDetector(Vector3 pos, Vector3 scale, LayerMask mask)
    {
        this.pos = pos;
        this.scale = scale;
        this.mask = mask;
    }

    public GroundDetector()
    {
        pos = Vector3.zero;
        scale = Vector3.one;
        mask = 2005;
    }
}

public class PlayerController : MonoBehaviour
{
    //For ground checking
    private bool isGrounded;
    public GroundDetector gCheck = new GroundDetector();

    //movement vars
    public float speed;
    public float airSpeed;
    private float moveInputH;
    private float moveInputV;
    private Rigidbody2D rb;

    //Jump vars
    public int extraJumps;
    private int jumps;
    public float jumpForce;
    private bool jumpPressed;
    private float jumpTimer;
    public float jumpTime;
    public float gravScale = 5;
    public float groundDrag = 5;
    public float airDrag = 1;

    //Respawn info
    [HideInInspector]
    public Vector3 RespawnPoint = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumps = extraJumps;
        RespawnPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        moveInputH = Input.GetAxisRaw("Horizontal");
        if (isGrounded) jumps = extraJumps;
        //Check if jump can be triggered
        if (Input.GetAxisRaw("Jump") == 1 && !jumpPressed && isGrounded)
            Jump();
        else if (Input.GetAxisRaw("Jump") == 1 && !jumpPressed && jumps > 0)
        {
            Jump();
            jumps--;
        }
        else if (Input.GetAxisRaw("Jump") == 0)
        {
            jumpPressed = false;
            jumpTimer = 0;
        }
        else if (jumpPressed && jumpTimer < jumpTime)
        {
            jumpTimer += Time.deltaTime;
            rb.drag = airDrag;
            rb.velocity = (Vector2.up * jumpForce) + new Vector2(rb.velocity.x, 0);
            jumpPressed = true;
        }
    }

    //FixedUpdate is called once per physics frame
    void FixedUpdate()
    {
        //check for ground
        isGrounded = GetGrounded();
    }

    public void Jump()
    {
        rb.drag = airDrag;
        if ((rb.velocity.x < 0 && moveInputH > 0) || (rb.velocity.x > 0 && moveInputH < 0))
            rb.velocity = Vector2.up * jumpForce;
        else
            rb.velocity = (Vector2.up * jumpForce) + new Vector2(rb.velocity.x, 0);
        jumpPressed = true;
    }

    public bool GetGrounded()
    {
        return Physics2D.OverlapBox(gCheck.pos, gCheck.scale, gCheck.mask);
    }

    //Ground check visualizer
    private void OnDrawGizmosSelected()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.white;
        }
        Gizmos.DrawCube(gCheck.pos + transform.position, gCheck.scale);
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GroundDetector
{
    public Vector3 pos = new Vector3();
    public Vector3 scale = new Vector3();
    public LayerMask mask = 4160;

    public GroundDetector(Vector3 pos, Vector3 scale, LayerMask mask)
    {
        this.pos = pos;
        this.scale = scale;
        this.mask = mask;
    }
}

[System.Serializable]
public class GravCoefficients
{
    [Header("G is for grounded | J is for Jumping")]
    public float G_GravCoef = 1;
    public float J_GravCoef = 0.7420f;

    public void G_SetGravCoef(Rigidbody2D plyr)
    {
        plyr.gravityScale = G_GravCoef;
    }

    public void J_SetGravCoef(Rigidbody2D plyr)
    {
        plyr.gravityScale = J_GravCoef;
    }
}

public class PlayerController : MonoBehaviour
{
    public float speed = 275;
    public float accCap = 20;
    public float jumpPower = 100;
    public float maxJumpTime = 0.35f;
    private float curJumpTime;
    public bool isGrounded;
    public bool isJumping;
    private Rigidbody2D rb;
    [Range(0, 1)]
    public float A_Coef;
    [Header("Gravity Coefficients")]
    public GravCoefficients gCoeffs = new GravCoefficients();
    [Header("GroundDetector")]
    public GroundDetector gCheck = new GroundDetector(new Vector3(0, -0.505f), new Vector3(1, 0.05f, 0), 4160);

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        float xSpeed = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        if (Mathf.Abs(rb.velocity.x) >= accCap)
            xSpeed = xSpeed / Mathf.Abs(rb.velocity.x) / accCap;
        rb.AddForce(Vector2.right * xSpeed);

        //Jumping
        if (Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0)
            if (isGrounded && !isJumping) Jump();
        if (Input.GetButton("Jump") && isJumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower);
            curJumpTime -= Time.deltaTime;
        }
        if (Input.GetButtonUp("Jump") && curJumpTime > 0)
        {
            StopJump();
            rb.AddForce(Vector2.down * 75);
        }
        else if (curJumpTime <= 0) StopJump();
        if (GetGrounded())
            Grounded();
    }

    private void FixedUpdate()
    {
        if (!GetGrounded())
            rb.velocity -= new Vector2(rb.velocity.x * A_Coef, 0);
    }

    public void Jump()
    {
        isJumping = true;
        isGrounded = false;
        gCoeffs.J_SetGravCoef(rb);
        rb.velocity = new Vector3(rb.velocity.x, jumpPower);
        float temp = gCheck.pos.y;
        gCheck.pos.y = 0;
        curJumpTime = maxJumpTime;
        StartCoroutine(ResetGCheckPos(0.05f, temp));
    }

    private void StopJump()
    {
        curJumpTime = 0;
        isJumping = false;
    }

    public void Grounded()
    {
        isJumping = false;
        isGrounded = true;
        gCoeffs.G_SetGravCoef(rb);
    }

    public bool GetGrounded()
    {
        return Physics2D.OverlapBox(gCheck.pos + transform.position, gCheck.scale, 0, gCheck.mask);
    }

    public IEnumerator ResetGCheckPos(float time, float prevPos)
    {
        yield return new WaitForSeconds(time);
        gCheck.pos.y = prevPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (isGrounded)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.white;
        Gizmos.DrawCube(gCheck.pos + transform.position, gCheck.scale);
    }
}
