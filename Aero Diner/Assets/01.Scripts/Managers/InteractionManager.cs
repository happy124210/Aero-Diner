public class InteractionManager : Singleton<InteractionManager>
{
    public void Interact(PlayerInventory player, IInteractable target)
    {
        target.Interact(player);
    }
}