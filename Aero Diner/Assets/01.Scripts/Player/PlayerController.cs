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

    private float idleTimer = 0f;
    private float idleBreakTime = 5f;
    private bool hasTriggeredIdleBreak = false;

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
        interactActionRef.action.canceled += OnInteractCancel;
    }

    private void OnDisable()
    {
        moveActionRef.action.Disable();
        interactActionRef.action.Disable();
        pickupActionRef.action.Disable();

        interactActionRef.action.performed -= OnInteract;
        pickupActionRef.action.performed -= OnPickupDown;
        interactActionRef.action.canceled += OnInteractCancel;
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

        if (moveInput.sqrMagnitude < 0.01f)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleBreakTime)
            {
                int random = Random.Range(0, 2); // 0 또는 1
                animator.SetInteger("IdleBreakIndex", random);
                animator.SetTrigger("TriggerIdleBreak");

                idleTimer = 0f; // 다시 5초 후 실행 가능
            }
        }
        else
        {
            idleTimer = 0f;
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
                EventBus.OnSFXRequested(SFXType.ItemLaydown);
            }
        }
        else
        {
            var pickupTarget = FindBestInteractable(InteractionType.Pickup);
            if (pickupTarget != null)
            {
                SetDirectionParams();
                animator.SetTrigger("PickUp");

                playerInventory.TryPickup(pickupTarget);
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

        IInteractable best = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(origin, hit.point);

            // Pickup (EditStation phase에서는 IMovableStation 감지)
            if (interactionType == InteractionType.Pickup)
            {
                if (GameManager.Instance.CurrentPhase == GamePhase.EditStation)
                {
                    if (interactable is IMovableStation)
                    {
                        if (dist < closestDist)
                        {
                            best = interactable;
                            closestDist = dist;
                        }
                        continue;
                    }
                }

                // 기본 FoodDisplay 감지
                if (interactable is FoodDisplay food && food.CanPickup())
                {
                    if (dist < closestDist)
                    {
                        best = interactable;
                        closestDist = dist;
                    }
                }
            }

            // Use / Stop
            else if (interactionType == InteractionType.Use || interactionType == InteractionType.Stop)
            {
                if (interactable is PassiveStation || interactable is AutomaticStation || interactable is IngredientStation)
                {
                    if (dist < closestDist)
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

    private void OnInteractCancel(InputAction.CallbackContext context)
    {
        // J 키에서 손을 뗐을 때 Stop 처리
        currentTarget?.Interact(playerInventory, InteractionType.Stop);
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;

        Vector2 origin = rb.position;
        Vector2 direction = lastMoveDir == Vector2.zero ? Vector2.down : lastMoveDir;
        int rayCount = 5;
        float spread = 0.3f;
        float distance = interactionRadius;

        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        for (int i = 0; i < rayCount; i++)
        {
            float t = (i / (float)(rayCount - 1)) - 0.5f; // -0.5 ~ 0.5
            Vector2 offset = perpendicular * t * spread;
            Vector2 rayOrigin = origin + offset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rayOrigin, rayOrigin + direction.normalized * distance);
        }
#endif
    }
}
