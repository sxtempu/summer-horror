using System.Collections.Generic;
using UnityEngine;

namespace HorrorEngine
{
    public class PlayerStateAiming : ActorState
    {
        private static readonly int k_AimVerticalityHash = Animator.StringToHash("AimVerticality");

        [SerializeField] private SightCheck m_EnemySightCheck;
        [SerializeField] private ActorState m_AttackState;
        [SerializeField] private ActorState m_MotionState;
        [SerializeField] private ActorState m_ReloadState;
        [SerializeField] public bool m_AllowManualReload = true;

        [SerializeField] private PlayerMovement.MovementConstrain MovementConstrains = PlayerMovement.MovementConstrain.Movement;

        [Header("AutoAiming")]
        [SerializeField] private bool m_AutoAiming;
        [SerializeField] private float m_AutoAimingRange = 10f;
        [SerializeField] private float m_AutoAimingConeAngle = 30f;
        [SerializeField] private float m_AutoAimingDuration = 0.5f;
        [SerializeField] private LayerMask m_AutoAimingMask;

        [Header("Vertical Aiming")]
        [SerializeField] private bool m_CanAimVertically = true;
        [Tooltip("Input value from which verticality will to be considered to be triggered and therefore at its max value")]
        [SerializeField] private float m_AimVerticalInputThreshold;
        [SerializeField] private float m_AimVerticalLerpSpeed;
        [Tooltip("When enabled aiming will only be available at its higher or lower points")]
        [SerializeField] private bool m_AimVerticalAnalog;

        private IPlayerInput m_Input;
        private PlayerMovement m_Movement;
        private PlayerLookAtLookable m_LookAt;

        private Aimable m_AutoRotatingAtAimable;
        private Aimable m_LastAimedAt;
        private List<Aimable> m_DetectedAimables = new List<Aimable>();

        private Vector3 m_AutoAimingDir;
        private float m_AutoAimingRotationAngle;
        private float m_AimVerticality;
        private WeaponData m_Weapon;

        private Aimable m_LockedOnTarget;
        private bool m_IsLockedOn;

        public float Verticality => m_AimVerticality;
        public bool IsAiming => m_Input.IsAimingHeld();

        protected override void Awake()
        {
            base.Awake();

            m_Input = GetComponentInParent<IPlayerInput>();
            m_Movement = GetComponentInParent<PlayerMovement>();
            m_LookAt = Actor.MainAnimator.GetComponent<PlayerLookAtLookable>();
        }

        public override void StateEnter(IActorState fromState)
        {
            var weaponEntry = GameManager.Instance.Inventory.GetEquippedWeapon();
            m_Weapon = weaponEntry.Item as WeaponData;
            m_AnimationState = m_Weapon.AimingAnim;

            base.StateEnter(fromState);

            m_AimVerticality = Actor.MainAnimator.GetFloat(k_AimVerticalityHash);
            m_AutoRotatingAtAimable = null;
            if (m_AutoAiming)
            {
                SetInitialAimTarget();
            }

            if (!m_AutoAiming || m_AutoRotatingAtAimable == null)
            {
                m_Movement.enabled = true;
                m_Movement.AddConstrain(MovementConstrains);
            }

            m_LookAt.LookIntensity = 0f;
        }

        private void SetInitialAimTarget()
        {
            RefreshAvailableTargets(true);
            if (m_LastAimedAt) // Aim again last aimed target
            {
                int index = m_DetectedAimables.IndexOf(m_LastAimedAt);
                if (index >= 0)
                    SetAimAt(m_DetectedAimables[index]);
                else
                    m_LastAimedAt = null;
            }

            if (!m_LastAimedAt) // LastAimed was cleared since it was no longer a valid target
            {
                Aimable aimable = GetCloserTarget();
                if (aimable)
                    SetAimAt(aimable);
            }
        }

        private void RefreshAvailableTargets(bool clearDetected)
        {
            if (clearDetected)
            {
                m_DetectedAimables.Clear();
            }
            else
            {
                ClearDeadTargets();
            }

            Debug.Log("Casting rays to detect enemies...");

            // Cast multiple rays in a cone to detect enemies
            Vector3 forward = transform.forward;
            float halfAngle = m_AutoAimingConeAngle / 2;
            for (float angle = -halfAngle; angle <= halfAngle; angle += 5f)
            {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
                Debug.DrawRay(transform.position, direction * m_AutoAimingRange, Color.red, 1f);
                RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, m_AutoAimingRange, m_AutoAimingMask);
                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Monster"))
                    {
                        Debug.Log($"Detected Monster: {hit.collider.name}");
                        Aimable aimable = hit.collider.GetComponent<Aimable>();
                        if (aimable && IsAimableInSight(aimable))
                        {
                            Health health = aimable.GetComponentInParent<Health>();
                            if (!health.IsDead)
                            {
                                if (!m_DetectedAimables.Contains(aimable))
                                    m_DetectedAimables.Add(aimable);
                            }
                        }
                    }
                }
            }
        }

        private bool IsAimableInSight(Aimable aimable)
        {
            Vector3 directionToTarget = aimable.transform.position - transform.position;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            Debug.Log($"Checking visibility for: {aimable.name}, Angle: {angleToTarget}");

            if (angleToTarget <= m_AutoAimingConeAngle / 2)
            {
                if (!Physics.Linecast(transform.position, aimable.transform.position, out RaycastHit hit, m_AutoAimingMask) || hit.collider.CompareTag("Monster"))
                {
                    Debug.Log($"Aimable in sight: {aimable.name}");
                    return true;
                }
                else
                {
                    Debug.Log($"Linecast hit something else: {hit.collider.name}");
                }
            }
            else
            {
                Debug.Log($"Target out of angle: {aimable.name}");
            }
            return false;
        }

        private Aimable GetCloserTarget()
        {
            Aimable aimed = null;
            float minDist = int.MaxValue;
            foreach (Aimable aimable in m_DetectedAimables)
            {
                float distance = Vector3.Distance(Actor.transform.position, aimable.transform.position);
                if (distance < minDist)
                {
                    aimed = aimable;
                    minDist = distance;
                }
            }

            return aimed;
        }

        private void ClearDeadTargets()
        {
            for (int i = m_DetectedAimables.Count - 1; i >= 0; --i)
            {
                Aimable aimable = m_DetectedAimables[i];
                Health health = aimable.GetComponentInParent<Health>();
                if (health.IsDead)
                {
                    m_DetectedAimables.Remove(aimable);
                }
            }
        }

        private void ChangeAimTarget()
        {
            RefreshAvailableTargets(false);

            if (m_DetectedAimables.Count > 0)
            {
                int index = m_LastAimedAt ? m_DetectedAimables.IndexOf(m_LastAimedAt) : 0;
                ++index;

                if (index >= m_DetectedAimables.Count)
                    index = 0;

                SetAimAt(m_DetectedAimables[index]);
            }
            else
            {
                m_LastAimedAt = null;
                m_AutoRotatingAtAimable = null;
            }
        }

        private void SetAimAt(Aimable aimable)
        {
            m_AutoRotatingAtAimable = aimable;
            m_LastAimedAt = aimable;
            m_AutoAimingDir = m_AutoRotatingAtAimable.transform.position - Actor.transform.position;
            m_AutoAimingDir.y = 0;
            m_AutoAimingDir.Normalize();
            m_AutoAimingRotationAngle = Vector3.Angle(m_AutoAimingDir, Actor.transform.forward);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();

            if (m_AutoRotatingAtAimable)
                RotateToAutoAimingTarget();

            if (m_CanAimVertically)
                UpdateAimingVerticality();

            if (m_Input.IsLockOnDown())
                ToggleLockOn();

            if (m_IsLockedOn && m_LockedOnTarget)
                MaintainLockOnTarget();

            if (m_Input.IsAttackDown())
                SetState(m_AttackState);
            else if (!m_Input.IsAimingHeld() && !m_IsLockedOn)
                SetState(m_MotionState);
            else if (m_AllowManualReload && m_Input.IsReloadDown() && GameManager.Instance.Inventory.CanReloadEquippedWeapon())
                SetState(m_ReloadState);
            else if (m_Input.IsChangeAimTargetDown())
                ChangeAimTarget();
        }

        private void UpdateAimingVerticality()
        {
            float verticality = m_Input.GetPrimaryAxis().y;
            if (Mathf.Abs(verticality) > m_AimVerticalInputThreshold)
            {
                verticality = Mathf.Sign(verticality);
            }
            else if (!m_AimVerticalAnalog)
            {
                verticality = 0f;
            }

            m_AimVerticality = Mathf.MoveTowards(m_AimVerticality, verticality, Time.deltaTime * m_AimVerticalLerpSpeed);
            Actor.MainAnimator.SetFloat(k_AimVerticalityHash, m_AimVerticality);
        }

        private void RotateToAutoAimingTarget()
        {
            Actor.transform.rotation = Quaternion.RotateTowards(Actor.transform.rotation, Quaternion.LookRotation(m_AutoAimingDir), m_AutoAimingRotationAngle * (Time.deltaTime / m_AutoAimingDuration));
            float angleToTarget = Vector3.Angle(m_AutoAimingDir, Actor.transform.forward);
            if (angleToTarget < Mathf.Epsilon)
            {
                m_AutoRotatingAtAimable = null;
                m_Movement.enabled = true;
            }
        }

        public void ToggleLockOn()
        {
            if (m_IsLockedOn)
            {
                m_IsLockedOn = false;
                m_LockedOnTarget = null;
                m_Movement.RemoveConstrain(MovementConstrains);
            }
            else
            {
                Aimable aimable = GetClosestVisibleTarget();
                if (aimable)
                {
                    m_IsLockedOn = true;
                    m_LockedOnTarget = aimable;
                    m_Movement.AddConstrain(MovementConstrains);
                }
            }
        }

        private Aimable GetClosestVisibleTarget()
        {
            RefreshAvailableTargets(true);

            Aimable closestAimable = null;
            float closestDistance = float.MaxValue;

            foreach (var aimable in m_DetectedAimables)
            {
                float distance = Vector3.Distance(Actor.transform.position, aimable.transform.position);
                if (distance < closestDistance && IsAimableInSight(aimable))
                {
                    closestDistance = distance;
                    closestAimable = aimable;
                }
            }

            return closestAimable;
        }

        private void MaintainLockOnTarget()
        {
            if (m_LockedOnTarget != null)
            {
                Vector3 directionToTarget = m_LockedOnTarget.transform.position - Actor.transform.position;
                directionToTarget.y = 0; // Maintain horizontal lock
                Actor.transform.rotation = Quaternion.LookRotation(directionToTarget);
                m_Movement.ConstrainToStrafe();
            }
        }

        public override void StateExit(IActorState intoState)
        {
            m_Movement.enabled = false;
            m_Movement.RemoveConstrain(MovementConstrains);

            m_LookAt.LookIntensity = 1f;

            if ((ActorState)intoState != m_AttackState)
            {
                m_LastAimedAt = null;
            }

            base.StateExit(intoState);
        }

        private void OnDrawGizmos()
        {
            if (m_AutoAiming)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_AutoAimingRange);
                Vector3 forward = transform.forward * m_AutoAimingRange;
                Vector3 right = Quaternion.Euler(0, m_AutoAimingConeAngle / 2, 0) * forward;
                Vector3 left = Quaternion.Euler(0, -m_AutoAimingConeAngle / 2, 0) * forward;
                Gizmos.DrawLine(transform.position, transform.position + forward);
                Gizmos.DrawLine(transform.position, transform.position + right);
                Gizmos.DrawLine(transform.position, transform.position + left);
            }
        }
    }
}
