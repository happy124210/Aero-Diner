public enum InteractionType
{
    Use,    // J 키
    Pickup,  // K 키
    Stop    // J 키 뗐을 때
}
public interface IInteractable
{
    void Interact(PlayerInventory inventory, InteractionType interactionType); //키 타입에 따라 동작 다르게
    void OnHoverEnter();
    void OnHoverExit();
}