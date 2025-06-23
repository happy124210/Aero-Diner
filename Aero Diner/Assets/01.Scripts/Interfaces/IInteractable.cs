public interface IInteractable
{
    void Interact(PlayerInventory inventory);
    void OnHoverEnter();
    //{
    //Debug.Log("하이라이트 ON");
    //GetComponent<SpriteRenderer>().color = Color.yellow;
    //}
void OnHoverExit();
    //{
    //Debug.Log("하이라이트 OFF");
    //GetComponent<SpriteRenderer>().color = Color.white;
    //}
}