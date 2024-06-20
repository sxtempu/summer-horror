using UnityEngine;
using HorrorEngine;

public class PlayerLockOn : MonoBehaviour
{
    [SerializeField] private SightCheck sightCheck;
    [SerializeField] private Transform[] enemies; // Array of potential enemy targets
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private PlayerStateAiming aimingState; // Reference to the aiming state
    private Transform currentTarget;
    private bool isAiming;

    private IPlayerInput m_Input; // Reference to input handling

    void Awake()
    {
        m_Input = GetComponentInParent<IPlayerInput>();
    }

    void Update()
    {
        if (m_Input.IsLockOnDown()) // Custom input method for lock-on
        {
            LockOnToClosestTarget();
        }

        if (currentTarget != null)
        {
            // Handle player strafing input
            float strafeInput = m_Input.GetPrimaryAxis().x; // Assuming horizontal axis for strafing
            if (strafeInput != 0)
            {
                RotateAroundTarget(strafeInput);
            }

            if (isAiming)
            {
                // Ensure player is always looking at the target
                transform.LookAt(currentTarget);
            }

            if (m_Input.IsAimingHeld()) // Custom input method for aiming
            {
                StartAiming();
            }

            if (!m_Input.IsAimingHeld()) // Custom input method for aiming release
            {
                StopAiming();
            }
        }
    }

    void LockOnToClosestTarget()
    {
        currentTarget = sightCheck.GetClosestTargetInSight(enemies);
        if (currentTarget != null)
        {
            Debug.Log("Locked on to " + currentTarget.name);
        }
        else
        {
            Debug.Log("No target in sight");
        }
    }

    void RotateAroundTarget(float strafeInput)
    {
        // Calculate the direction to strafe around the target
        Vector3 directionToTarget = transform.position - currentTarget.position;
        directionToTarget.y = 0; // Keep the rotation in the horizontal plane

        // Calculate the new position
        Quaternion strafeRotation = Quaternion.Euler(0, strafeInput * rotationSpeed * Time.deltaTime, 0);
        Vector3 newPosition = strafeRotation * directionToTarget;
        newPosition += currentTarget.position;

        // Move the player to the new position
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * rotationSpeed);

        // Ensure the player looks at the target
        transform.LookAt(currentTarget.position);
    }

    void StartAiming()
    {
        if (!isAiming)
        {
            isAiming = true;
            aimingState.StateEnter(null); // Enter aiming state
        }
    }

    void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;
            aimingState.StateExit(null); // Exit aiming state
        }
    }
}
