using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7;
    public bool isGrounded = true; // To check if the player is on the ground
    public bool isGrappling = false; // To check if the player is grappling

    [FormerlySerializedAs("grapplingPoint")]
    public Vector2 grapplePoint; // The point where the player is grappling

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Horizontal movement
        if (!isGrappling || isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            Vector2 movement = new Vector2(moveX * moveSpeed, rb.velocity.y);
            rb.velocity = movement;
        }
        else
        {
            // Move horizontally by swinging in the circle of grappling point
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector2 swingDirection = perpendicular * -Input.GetAxis("Horizontal");
            rb.velocity = swingDirection * moveSpeed;
        }

        // Check for jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
        }

        if (isGrounded)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.1f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private
        void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
