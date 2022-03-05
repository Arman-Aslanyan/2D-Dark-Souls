using System.Collections;
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
}
