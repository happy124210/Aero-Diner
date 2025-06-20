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

            Collider2D largest = null;
            float maxArea = 0f;

            foreach (var col in hits)
            {
                Vector2 size = col.bounds.size;
                float area = size.x * size.y;

                if (area > maxArea)
                {
                    maxArea = area;
                    largest = col;
                }
            }

            if (largest != null)
            {
                var interactable = largest.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
