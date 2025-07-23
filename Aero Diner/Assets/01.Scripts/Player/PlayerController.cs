using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
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
    private TilemapController tilemapController;


    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        playerInventory = GetComponent<PlayerInventory>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        tilemapController = FindObjectOfType<TilemapController>();
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
        interactActionRef.action.canceled -= OnInteractCancel;
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

        if (isInteracting)
        {
            currentTarget.Interact(playerInventory, interactionType);
        }

        if (moveInput.sqrMagnitude < 0.01f)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleBreakTime)
            {
                int random = Random.Range(0, 2);
                animator.SetInteger("IdleBreakIndex", random);
                animator.SetTrigger("TriggerIdleBreak");
                idleTimer = 0f;
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
        if (!context.performed || playerInventory == null) return;

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
        var hits = CastAll(transform.position, lastMoveDir, interactionRadius, interactableLayer);

        IInteractable stationTarget = null;
        IInteractable gridTarget = null;

        foreach (var hit in hits)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            if (hit.collider.CompareTag("Station") && stationTarget == null)
                stationTarget = interactable;
            else if (hit.collider.CompareTag("GridCell") && gridTarget == null)
                gridTarget = interactable;
        }

        //조건 분기: 들고 있는 Station이 있으면 GridCell 우선, 아니면 Station 우선
        bool holdingStation = playerInventory.heldStation != null;

        IInteractable newTarget = holdingStation ? gridTarget ?? stationTarget
                                                 : stationTarget ?? gridTarget;

        if (newTarget != currentTarget)
        {
            currentTarget?.OnHoverExit();
            newTarget?.OnHoverEnter();
            currentTarget = newTarget;
        }
    }

    private IInteractable FindBestInteractable(InteractionType interactionType)
    {
        var hits = CastAll(transform.position, lastMoveDir, interactionRadius, interactableLayer);
        IInteractable best = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(transform.position, hit.point);

            if (interactionType == InteractionType.Pickup)
            {
                if (GameManager.Instance.CurrentPhase == GamePhase.EditStation && interactable is IMovableStation)
                {
                    if (dist < closestDist)
                    {
                        best = interactable;
                        closestDist = dist;
                    }
                    continue;
                }
                if (interactable is FoodDisplay food && food.CanPickup())
                {
                    if (dist < closestDist)
                    {
                        best = interactable;
                        closestDist = dist;
                    }
                }
                if (interactable is IngredientStation)
                {
                    if (dist < closestDist)
                    {
                        best = interactable;
                        closestDist = dist;
                    }
                }

                else if (interactionType == InteractionType.Use || interactionType == InteractionType.Stop)
            
                if (interactable is PassiveStation || interactable is AutomaticStation)
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

    public Transform FindGridCellInFront()
    {
        Vector2 origin = transform.position;
        Vector2 direction = lastMoveDir == Vector2.zero ? Vector2.down : lastMoveDir.normalized;
        float distance = 2f;

        var hit = CastSingle(origin, direction, distance, LayerMask.GetMask("IInteractable"));
        Debug.DrawRay(origin, direction * distance, Color.green);

        // GridCell 감지됨
        if (hit.HasValue && hit.Value.collider.CompareTag("GridCell"))
        {
            GameObject hitCell = hit.Value.collider.gameObject;

            // 현재 선택된 셀과 다를 경우에만 갱신
            if (tilemapController != null)
            {
                tilemapController.HighlightSelectedCell(hitCell);
            }

            return hitCell.transform;
        }

        return null;
    }

    private RaycastHit2D? CastSingle(Vector2 origin, Vector2 direction, float distance, LayerMask layer)
    {
        var hit = Physics2D.Raycast(origin, direction, distance, layer);
        return hit.collider != null ? hit : null;
    }

    private RaycastHit2D[] CastAll(Vector2 origin, Vector2 direction, float distance, LayerMask layer)
    {
        return Physics2D.RaycastAll(origin, direction, distance, layer);
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
            float t = (i / (float)(rayCount - 1)) - 0.5f;
            Vector2 offset = perpendicular * t * spread;
            Vector2 rayOrigin = origin + offset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rayOrigin, rayOrigin + direction.normalized * distance);
        }
#endif
    }

}
