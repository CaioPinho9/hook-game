using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public Rigidbody2D playerRigidBody;
    public DistanceJoint2D distanceJoint;
    public GameObject hookPrefab; // Prefab for the hook
    public Player player; // Reference to the Player class

    public float hookSpeed = 10f;
    public float springForce = 50f;
    public float damping = 5f;
    public float maxDistance = 10f;
    public float distanceChangeSpeed = 0.1f;
    public float currentDistance;

    public Vector2 grapplePoint;
    public Vector2 currentGrapplePosition;
    public bool isGrappling;

    private GameObject _currentHook;
    private int _curveSegments = 20; // Increase the number of segments

    private void Start()
    {
        distanceJoint.enabled = false;
    }

    private void Update()
    {
        if (!isGrappling)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartGrapple();
            }

            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
            return;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            currentDistance += distanceChangeSpeed;
            currentDistance = Mathf.Min(currentDistance, maxDistance);
        }

        if (Input.GetKey(KeyCode.E))
        {
            currentDistance -= distanceChangeSpeed;
            currentDistance = Mathf.Max(currentDistance, 1);
        }

        distanceJoint.distance = currentDistance;

        UpdateGrapple();
    }

    private void UpdateGrapple()
    {
        // Move grapple based on the speed until it reaches the grapple point
        currentGrapplePosition = Vector2.MoveTowards(currentGrapplePosition, grapplePoint, hookSpeed * Time.deltaTime);

        if (Vector2.Distance(currentGrapplePosition, grapplePoint) < 0.1f)
        {
            currentGrapplePosition = grapplePoint;
            distanceJoint.enabled = true;
            distanceJoint.connectedAnchor = grapplePoint;

            if (currentDistance == 0)
            {
                currentDistance = Vector2.Distance(firePoint.position, grapplePoint);
            }
        }

        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        Vector3 startPoint = firePoint.position;
        Vector3 endPoint = currentGrapplePosition;
        Vector3 controlPoint = (startPoint + endPoint) / 2;
        controlPoint.y -= Vector2.Distance(startPoint, endPoint) / 4; // Reduce the gravity effect

        lineRenderer.positionCount = _curveSegments;

        for (int i = 0; i < _curveSegments; i++)
        {
            float t = (float)i / (_curveSegments - 1);
            Vector3 point = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
            lineRenderer.SetPosition(i, point);
        }

        // Ensure the hook follows the grapple point
        if (_currentHook)
        {
            _currentHook.transform.position = currentGrapplePosition;
            _currentHook.transform.up = (currentGrapplePosition - (Vector2)firePoint.position).normalized;
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2; // t^2 * P2
        return p;
    }

    private void StartGrapple()
    {
        currentGrapplePosition = firePoint.position;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePosition - (Vector2)firePoint.position;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxDistance);
        if (!hit) return;

        var grapplable = hit.transform.gameObject.GetComponent<Grapplable>();
        if (!grapplable) return;

        isGrappling = true;
        grapplePoint = hit.point;
        player.isGrappling = true;
        player.grapplePoint = grapplePoint;
        lineRenderer.positionCount = 2;

        // Instantiate hook at the grapple point
        if (_currentHook)
        {
            Destroy(_currentHook);
        }

        _currentHook = Instantiate(hookPrefab, grapplePoint, Quaternion.identity);
    }

    private void StopGrapple()
    {
        isGrappling = false;
        distanceJoint.enabled = false;
        player.isGrappling = false;
        lineRenderer.positionCount = 0;
        currentDistance = 0;

        // Destroy the hook
        if (_currentHook)
        {
            Destroy(_currentHook);
        }
    }

    private void FixedUpdate()
    {
        if (isGrappling && currentGrapplePosition == grapplePoint)
        {
            Vector2 directionToGrapplePoint = (grapplePoint - playerRigidBody.position).normalized;
            float distanceToGrapplePoint = Vector2.Distance(playerRigidBody.position, grapplePoint);

            // Apply a spring force to pull the player towards the grapple point
            Vector2 springForceVector = directionToGrapplePoint * ((distanceToGrapplePoint - currentDistance) * springForce);
            playerRigidBody.AddForce(springForceVector);

            // Apply damping to reduce oscillations
            Vector2 dampingForce = -playerRigidBody.velocity * damping;
            playerRigidBody.AddForce(dampingForce);

            // Apply gravity
            playerRigidBody.AddForce(Physics2D.gravity * playerRigidBody.mass);

            // Apply pendulum momentum
            Vector2 tangent = new Vector2(-directionToGrapplePoint.y, directionToGrapplePoint.x);
            float tangentSpeed = Vector2.Dot(playerRigidBody.velocity, tangent);
            playerRigidBody.AddForce(tangent * (tangentSpeed * damping));
        }
    }
}
