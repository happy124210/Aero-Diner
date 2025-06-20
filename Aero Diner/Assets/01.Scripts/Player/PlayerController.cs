using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("상호작용 설정")]
    public float interactionRadius = 1.5f;
    public LayerMask interactableLayer;

    [SerializeField] private PlayerInventory playerInventory;

    private IInteractable currentTarget;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInventory = GetComponent<PlayerInventory>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void Update()
    {
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;

        RaycastForInteractable(); // ▶ 매 프레임 상호작용 대상 탐색
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
    private void RaycastForInteractable()
    {
        Vector2 origin = transform.position;
        Vector2 direction = lastMoveDir;
        float distance = interactionRadius;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, interactableLayer);

        if (hit.collider != null)
        {
            currentTarget = hit.collider.GetComponent<IInteractable>();
        }
        else
        {
            currentTarget = null;
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentTarget != null && playerInventory != null)
        {
            Debug.Log("상호작용 시도!");
            currentTarget.Interact(playerInventory);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Vector3 forward = (Vector3)(Application.isPlaying ? lastMoveDir : Vector2.down);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + forward.normalized * interactionRadius);
    }
}
