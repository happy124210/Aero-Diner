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

    public InteractionType interactionType;
    public IInteractable currentTarget;
    private IInteractable previousTarget;

    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Rigidbody2D rb;

    public PlayerInputActions inputActions;
    private InputAction interactAction;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInventory = GetComponent<PlayerInventory>();

        inputActions = new PlayerInputActions();
        inputActions.Enable(); // 중요!
        animator = GetComponent<Animator>();

        interactAction = inputActions.Player.Interact;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    private void Update()
    {
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;

        Animate();
        UpdateItemSlotPosition();
        RaycastForInteractable();

        UpdateItemSlotPosition();
        RaycastForInteractable();
        animator.SetBool("IsCarrying", playerInventory.IsHoldingItem);
        if (interactAction == null) return;

        bool isHolding = interactAction.IsPressed();
        bool justPressed = interactAction.WasPressedThisFrame();

        if (currentTarget != null)
        {
            if (interactionType == InteractionType.Use)
            {
                if (isHolding)
                {
                    currentTarget.Interact(playerInventory, interactionType);
                }
            }
            else
            {
                if (justPressed)
                {
                    currentTarget.Interact(playerInventory, interactionType);
                }
            }
        }


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

        var target = FindBestInteractable(InteractionType.Use);
        target?.Interact(playerInventory, InteractionType.Use);
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
            // PutDown 처리
            if (currentTarget != null)
            {
                // 애니메이션 처리
                SetDirectionParams();
                animator.SetTrigger("PutDown");

                // 실제 내려놓기 처리
                playerInventory.DropItem(currentTarget);
            }
            else
            {
                Debug.Log("내려놓을 대상이 없습니다.");
            }
        }
        else
        {
            // PickUp 처리
            var pickupTarget = FindBestInteractable(InteractionType.Pickup);
            if (pickupTarget != null)
            {
                // 애니메이션 처리
                SetDirectionParams();
                animator.SetTrigger("PickUp");

                // 실제 줍기 처리
                playerInventory.TryPickup(pickupTarget);
            }
            else
            {
                Debug.Log("줍기 대상이 없습니다.");
            }
        }
    }
    //j키와 k키 상호작용 분리를 위한 함수
    private IInteractable FindBestInteractable(InteractionType interactionType)
    {
        Vector2 origin = transform.position;
        Vector2 direction = lastMoveDir;
        float distance = interactionRadius;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, interactableLayer);

        IInteractable fallback = null;

        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            //여기서 상호작용 분리
            switch (interactionType)
            {
                case InteractionType.Use:
                    if (interactable is PassiveStation || interactable is AutomaticStation || interactable is IngredientStation)
                        return interactable;
                    break;

                case InteractionType.Pickup:
                    if (interactable is FoodDisplay)
                        return interactable;
                    break;
            }

            // Fallback 후보 (없을 경우 대비)
            if (fallback == null)
                fallback = interactable;
        }

        return fallback;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        Vector3 forward = (Vector3)(Application.isPlaying ? lastMoveDir : Vector2.down);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + forward.normalized * interactionRadius);
    }
    private void Animate()
    {
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
        animator.SetBool("IsMoving", moveInput != Vector2.zero);
        animator.SetBool("IsHolding", playerInventory.IsHoldingItem);

        // Optional: idle 방향 유지
        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("LastMoveX", lastMoveDir.x);
            animator.SetFloat("LastMoveY", lastMoveDir.y);
        }
    }
    private void SetDirectionParams()
    {
        animator.SetFloat("LastMoveX", lastMoveDir.x);
        animator.SetFloat("LastMoveY", lastMoveDir.y);
    }
}
