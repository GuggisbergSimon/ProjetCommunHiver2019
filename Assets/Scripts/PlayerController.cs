using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    Rigidbody2D rb;

    float inputHorizontal;
    [SerializeField] float playerHorizontalSpeed;

    [SerializeField] float gravity = 1;
    [SerializeField] float jumpSpeed;

    [SerializeField] private Transform groundCheck;
    private float groundRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] GameObject Camera;
    const float normalGravity = 9.81f;

    enum GravityDirection
    {
        NORTH,
        SOUTH,
        EAST,
        WEST
    }

    GravityDirection gravityDirection;

    bool turnLeft;
    bool turnRight;
    bool jump;
    bool canTurn;
    bool grounded;



    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityDirection = GravityDirection.SOUTH;
        canTurn = true;
        gravity *= normalGravity;
    }

    private void Update()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        inputHorizontal = Input.GetAxis("Horizontal");
        if (canTurn)
        {
            if (Input.GetButtonDown("TurnLeft"))
                turnLeft = true;
            if (Input.GetButtonDown("TurnRight"))
                turnRight = true;
            if (Input.GetButtonDown("Jump") && grounded)
                jump = true;
        }
        Debug.Log(jump);
    }

    private void FixedUpdate()
    {
        if (turnLeft)
        {
            gravityDirection = TurnLeft(gravityDirection);
            StartCoroutine("TurnLeftCoroutine");
            turnLeft = false;
        }

        if (turnRight)
        {
            gravityDirection = TurnRight(gravityDirection);
            StartCoroutine("TurnRightCoroutine");
            turnRight = false;
        }

        switch (gravityDirection)
        {
            case GravityDirection.SOUTH:
                gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
                rb.AddForce(new Vector2(0, -gravity));
                rb.velocity = new Vector2(inputHorizontal * playerHorizontalSpeed, rb.velocity.y);
                if (jump && grounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                    jump = false;
                }


                if (rb.velocity.y < 0)
                {
                    rb.velocity += Vector2.up * -gravity * (fallMultiplier + 1) * Time.deltaTime;
                }
                else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector2.up * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
                }
                break;
            case GravityDirection.NORTH:
                gameObject.transform.eulerAngles = new Vector3(0, 0, 180);
                rb.AddForce(new Vector2(0, gravity));
                rb.velocity = new Vector2(-inputHorizontal * playerHorizontalSpeed, rb.velocity.y);
                if (jump && grounded)
                {
                    rb.velocity = new Vector2(rb.velocity.x, -jumpSpeed);
                    jump = false;
                }


                if (rb.velocity.y > 0)
                {
                    rb.velocity += Vector2.down * -gravity * (fallMultiplier + 1) * Time.deltaTime;
                }
                else if (rb.velocity.y < 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector2.down * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
                }
                break;
            case GravityDirection.WEST:
                gameObject.transform.eulerAngles = new Vector3(0, 0, 270);
                rb.AddForce(new Vector2(-gravity, 0));
                rb.velocity = new Vector2(rb.velocity.x, -inputHorizontal * playerHorizontalSpeed);
                if (jump && grounded)
                {
                    rb.velocity = new Vector2(jumpSpeed, rb.velocity.y);
                    jump = false;
                }

                if (rb.velocity.x < 0)
                {
                    rb.velocity += Vector2.right * -gravity * (fallMultiplier + 1) * Time.deltaTime;
                }
                else if (rb.velocity.x > 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector2.right * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
                }
                break;
            case GravityDirection.EAST:
                gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
                rb.AddForce(new Vector2(gravity, 0));
                rb.velocity = new Vector2(rb.velocity.x, inputHorizontal * playerHorizontalSpeed);
                if (jump && grounded)
                {
                    rb.velocity = new Vector2(-jumpSpeed, rb.velocity.y);
                    jump = false;
                }

                if (rb.velocity.x > 0)
                {
                    rb.velocity += Vector2.left * -gravity * (fallMultiplier + 1) * Time.deltaTime;
                }
                else if (rb.velocity.x < 0 && !Input.GetButton("Jump"))
                {
                    rb.velocity += Vector2.left * -gravity * (lowJumpMultiplier + 1) * Time.deltaTime;
                }
                break;
        }
    }

    GravityDirection TurnLeft(GravityDirection gravityDirection)
    {
        switch (gravityDirection)
        {
            case GravityDirection.SOUTH:
                gravityDirection = GravityDirection.EAST;
                break;
            case GravityDirection.NORTH:
                gravityDirection = GravityDirection.WEST;
                break;
            case GravityDirection.WEST:
                gravityDirection = GravityDirection.SOUTH;
                break;
            case GravityDirection.EAST:
                gravityDirection = GravityDirection.NORTH;
                break;
        }
        return gravityDirection;
    }

    GravityDirection TurnRight(GravityDirection gravityDirection)
    {
        switch (gravityDirection)
        {
            case GravityDirection.SOUTH:
                gravityDirection = GravityDirection.WEST;
                break;
            case GravityDirection.NORTH:
                gravityDirection = GravityDirection.EAST;
                break;
            case GravityDirection.WEST:
                gravityDirection = GravityDirection.NORTH;
                break;
            case GravityDirection.EAST:
                gravityDirection = GravityDirection.SOUTH;
                break;
        }
        return gravityDirection;
    }

    IEnumerator TurnLeftCoroutine()
    {
        canTurn = false;
        for (int i = 0; i < 90; i += 2)
        {
            Camera.transform.eulerAngles = new Vector3(0, 0, Camera.transform.eulerAngles.z + 2);
            yield return new WaitForSeconds(0.01f);
        }
        canTurn = true;
    }

    IEnumerator TurnRightCoroutine()
    {
        canTurn = false;
        for (int i = 0; i < 90; i += 2)
        {
            Camera.transform.eulerAngles = new Vector3(0, 0, Camera.transform.eulerAngles.z - 2);
            yield return new WaitForSeconds(0.01f);
        }
        canTurn = true;
    }

}
