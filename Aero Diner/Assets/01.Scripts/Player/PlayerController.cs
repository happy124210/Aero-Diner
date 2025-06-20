using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 속도")]
    public float moveSpeed = 5f;

    [Header("키 설정")]
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode interactKey = KeyCode.E;

    [Header("상호작용 설정")]
    public float interactionRadius = 1.5f;
    public LayerMask interactableLayer;

    private Vector2 moveInput;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
        TryInteract();
    }

    void FixedUpdate()
    {
        Move();
    }

    void HandleInput()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(leftKey)) h -= 1f;
        if (Input.GetKey(rightKey)) h += 1f;
        if (Input.GetKey(downKey)) v -= 1f;
        if (Input.GetKey(upKey)) v += 1f;

        moveInput = new Vector2(h, v).normalized;
    }

    void Move()
    {
        Vector2 newPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);
    }
    void TryInteract()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius, interactableLayer);

            Collider2D closest = null;
            float minDistance = Mathf.Infinity;

            Vector2 forward = moveInput; // 현재 이동 방향을 기준으로 바라보는 방향 설정
            if (forward == Vector2.zero) forward = Vector2.down; // 정지 중일 땐 아래를 기본 방향으로 설정

            foreach (var col in hits)
            {
                Vector2 toTarget = (Vector2)col.transform.position - (Vector2)transform.position;
                float angle = Vector2.Angle(forward, toTarget);

                if (angle < 60f) // 플레이어가 바라보는 방향 앞쪽 120도 범위 안에 있을 때만
                {
                    float distance = toTarget.magnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = col;
                    }
                }
            }

            if (closest != null)
            {
                var interactable = closest.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        // 상호작용 범위 디버그
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
