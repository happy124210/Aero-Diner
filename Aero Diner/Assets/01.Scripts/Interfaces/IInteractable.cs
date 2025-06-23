public interface IInteractable
{
    void Interact(PlayerInventory inventory);
    void OnHoverEnter();
    void OnHoverExit();


    //사용 예시
    //public class Chest : MonoBehaviour, IInteractable
    //{
    //    public void Interact(PlayerInventory inventory)
    //    {
    //        그래서 이걸 사용하면 뭔가가 됨.
    //    }

    //    public void OnHoverEnter()
    //    {
    //        Debug.Log("드디어 날 바라봐 주는구나");
    //        GetComponent<SpriteRenderer>().color = Color.yellow;
    //    }

    //    public void OnHoverExit()
    //    {
    //        Debug.Log("내게 더 이상 관심이 없구나 어떻게 그럴 수가 있어?");
    //        GetComponent<SpriteRenderer>().color = Color.white;
    //    }
    //}
}