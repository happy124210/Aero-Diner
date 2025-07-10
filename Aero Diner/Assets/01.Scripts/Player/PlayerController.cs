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
        Vector2 direction = lastMoveDir;
        float distance = interactionRadius;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance, interactableLayer);
        IInteractable fallback = null;

        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            switch (interactionType)
            {
                case InteractionType.Use:
                    if (interactable is PassiveStation || interactable is AutomaticStation || interactable is IngredientStation)
                        return interactable;
                    break;
                case InteractionType.Pickup:
                    if (interactable is FoodDisplay || interactable is IngredientStation)
                        return interactable;
                    break;
            }

            if (fallback == null) fallback = interactable;
        }

        return fallback;
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
