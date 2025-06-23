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
    [SerializeField] private Transform itemSlotTransform;
    [SerializeField] private Vector2 slotOffsetUp = new Vector2(0, 0.5f);
    [SerializeField] private Vector2 slotOffsetDown = new Vector2(0, -0.5f);
    [SerializeField] private Vector2 slotOffsetLeft = new Vector2(-0.5f, 0);
    [SerializeField] private Vector2 slotOffsetRight = new Vector2(0.5f, 0);

    public IInteractable currentTarget;
    private IInteractable previousTarget;

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
        UpdateItemSlotPosition();
        RaycastForInteractable(); //매 프레임 상호작용 대상 탐색
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
    private void RaycastForInteractable()
    {
        IInteractable newTarget = null;

        Vector2 origin = transform.position;
        Vector2 direction = lastMoveDir;
        float distance = interactionRadius;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, interactableLayer);

        if (hit.collider)
            newTarget = hit.collider.GetComponent<IInteractable>();

        //변화가 있을 때만 후처리
        if (newTarget != currentTarget)
        {
            // 이전 대상 정리
            if (currentTarget != null)
                currentTarget.OnHoverExit();

            // 새 대상 처리
            if (newTarget != null)
                newTarget.OnHoverEnter();

            currentTarget = newTarget;
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
    private void UpdateItemSlotPosition()
    {
        if (itemSlotTransform == null) return;

        // 4방향 중 가장 가까운 방향 결정
        Vector2 dir = lastMoveDir;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            itemSlotTransform.localPosition = dir.x > 0 ? slotOffsetRight : slotOffsetLeft;
        }
        else
        {
            itemSlotTransform.localPosition = dir.y > 0 ? slotOffsetUp : slotOffsetDown;
        }
    }
    public void OnPickupDown(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (playerInventory == null) return;

        if (playerInventory.IsHoldingItem)
        {
            if (currentTarget != null)
            {
                playerInventory.DropItem(currentTarget);
                Debug.Log("아이템을 내려놓음");
            }
            else
            {
                Debug.Log("내려놓을 대상이 없습니다."); // 또는 피드백 UI
            }
        }
        else
        {
            playerInventory.TryPickup(currentTarget);
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
