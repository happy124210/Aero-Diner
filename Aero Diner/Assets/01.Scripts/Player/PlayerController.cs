using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("입력 액션 참조")]
    public InputActionReference moveActionRef;
    public InputActionReference interactActionRef;
    public InputActionReference pickupActionRef;

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
    [SerializeField] private AudioSource moveSFXSource;
    [SerializeField] private AudioClip moveSFXClip;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;

    public InteractionType interactionType;
    public IInteractable currentTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInventory = GetComponent<PlayerInventory>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        moveActionRef.action.Enable();
        interactActionRef.action.Enable();
        pickupActionRef.action.Enable();

        interactActionRef.action.performed += OnInteract;
        pickupActionRef.action.performed += OnPickupDown;
    }

    private void OnDisable()
    {
        moveActionRef.action.Disable();
        interactActionRef.action.Disable();
        pickupActionRef.action.Disable();

        interactActionRef.action.performed -= OnInteract;
        pickupActionRef.action.performed -= OnPickupDown;
    }

    private void Update()
    {
        moveInput = moveActionRef.action.ReadValue<Vector2>();
        Animate();
        UpdateItemSlotPosition();
        RaycastForInteractable();

        animator.SetBool("IsCarrying", playerInventory.IsHoldingItem);

        bool isHolding = interactActionRef.action.IsPressed();
        bool isInteracting = currentTarget != null && interactionType == InteractionType.Use && isHolding;
        animator.SetBool("IsInteract", isInteracting);

        if (currentTarget != null && interactionType == InteractionType.Use && isHolding)
        {
            currentTarget.Interact(playerInventory, interactionType);
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        var target = FindBestInteractable(InteractionType.Use);
        target?.Interact(playerInventory, InteractionType.Use);
    }

    private void OnPickupDown(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (playerInventory == null) return;

        if (playerInventory.IsHoldingItem)
        {
            if (currentTarget != null)
            {
                SetDirectionParams();
                animator.SetTrigger("PutDown");

                playerInventory.DropItem(currentTarget);
                EventBus.OnSFXRequested(SFXType.Itemlaydown);
            }
        }
        else
        {
            var pickupTarget = FindBestInteractable(InteractionType.Pickup);
            if (pickupTarget != null)
            {
                SetDirectionParams();
                animator.SetTrigger("PickUp");

                if (pickupTarget is IngredientStation)
                {
                    pickupTarget.Interact(playerInventory, InteractionType.Pickup);
                }
                else
                {
                    playerInventory.TryPickup(pickupTarget);
                }

                EventBus.OnSFXRequested(SFXType.ItemPickup);
            }
        }
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

        if (newTarget != currentTarget)
        {
            currentTarget?.OnHoverExit();
            newTarget?.OnHoverEnter();
            currentTarget = newTarget;
        }
    }

    private IInteractable FindBestInteractable(InteractionType interactionType)
    {
        Vector2 origin = transform.position;
        float distance = interactionRadius;
        float fanAngle = 105f;
        int rayCount = 7;

        Vector2 forward = lastMoveDir == Vector2.zero ? Vector2.down : lastMoveDir;
        float startAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - fanAngle / 2f;

        IInteractable best = null;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = startAngle + (fanAngle / (rayCount - 1)) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, distance, interactableLayer);
            Debug.DrawRay(origin, dir * distance, Color.magenta, 0.2f);

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable == null) continue;

                // 거리 측정
                float dist = Vector2.Distance(origin, hit.point);
                if (dist >= closestDist) continue;

                // InteractionType에 따라 타입 우선순위 판단
                if (interactionType == InteractionType.Pickup)
                {
                    if (interactable is FoodDisplay)
                    {
                        best = interactable;
                        closestDist = dist;
                        break; // 최우선, 더 이상 탐색 X
                    }
                    else if (interactable is IngredientStation)
                    {
                        best = interactable;
                        closestDist = dist;
                        // 계속 탐색: 혹시 더 가까운 FoodDisplay가 있을 수도 있으므로
                    }
                }
                else if (interactionType == InteractionType.Use)
                {
                    if (interactable is PassiveStation || interactable is AutomaticStation || interactable is IngredientStation)
                    {
                        best = interactable;
                        closestDist = dist;
                    }
                }
            }
        }

        return best;
    }


    private void Animate()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetBool("IsMoving", true);

            lastMoveDir = moveInput.normalized;
            animator.SetFloat("LastMoveX", lastMoveDir.x);
            animator.SetFloat("LastMoveY", lastMoveDir.y);

            if (!moveSFXSource.isPlaying)
            {
                moveSFXSource.clip = moveSFXClip;
                moveSFXSource.loop = true;
                moveSFXSource.Play();
            }
        }
        else
        {
            animator.SetBool("IsMoving", false);
            if (moveSFXSource.isPlaying)
            {
                moveSFXSource.Stop();
            }
        }
    }

    private void UpdateItemSlotPosition()
    {
        if (itemSlotTransform == null) return;

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

    private void SetDirectionParams()
    {
        if (lastMoveDir == Vector2.zero)
            lastMoveDir = Vector2.down;

        animator.SetFloat("LastMoveX", lastMoveDir.x);
        animator.SetFloat("LastMoveY", lastMoveDir.y);
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
