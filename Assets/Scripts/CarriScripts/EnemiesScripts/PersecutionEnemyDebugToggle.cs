using UnityEngine;
using UnityEngine.AI;

public class PersecutionEnemyDebugToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PersecutionEnemy persecutionEnemy;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Controls")]
    [SerializeField] private bool startActive = false;
    [SerializeField] private KeyCode startKey = KeyCode.T;
    [SerializeField] private KeyCode stopKey = KeyCode.Y;

    [Header("Animation")]
    [SerializeField] private string walkingBoolName = "IsWalking";

    private void Reset()
    {
        FindReferences();
    }

    private void Awake()
    {
        FindReferences();
        SetPersecution(startActive);
    }

    private void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            SetPersecution(true);
        }

        if (Input.GetKeyDown(stopKey))
        {
            SetPersecution(false);
        }
    }

    private void FindReferences()
    {
        if (persecutionEnemy == null)
            persecutionEnemy = GetComponent<PersecutionEnemy>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
    }

    private void SetPersecution(bool active)
    {
        if (persecutionEnemy != null)
            persecutionEnemy.enabled = active;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = !active;

            if (!active)
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
            }
        }

        if (!active)
            SetWalking(false);
    }

    private void SetWalking(bool value)
    {
        if (string.IsNullOrEmpty(walkingBoolName))
            return;

        if (!CanUseAnimator("SetBool", walkingBoolName))
            return;

        animator.SetBool(walkingBoolName, value);
    }

    private bool CanUseAnimator(string operation, string parameterName)
    {
        if (animator == null)
        {
            Debug.LogWarning($"[PersecutionEnemyDebugToggle] No hay Animator asignado para {operation}('{parameterName}') en '{gameObject.name}'.", this);
            return false;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[PersecutionEnemyDebugToggle] El Animator '{animator.name}' en '{animator.gameObject.name}' no tiene AnimatorController para {operation}('{parameterName}'). Toggle: '{gameObject.name}'.", animator);
            return false;
        }

        if (!animator.isActiveAndEnabled)
        {
            Debug.LogWarning($"[PersecutionEnemyDebugToggle] El Animator '{animator.name}' en '{animator.gameObject.name}' no está activo/habilitado para {operation}('{parameterName}'). Toggle: '{gameObject.name}'.", animator);
            return false;
        }

        return true;
    }
}
