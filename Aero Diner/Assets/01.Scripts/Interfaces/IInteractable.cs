using UnityEngine;
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

// 확장 메서드를 정의할 정적 클래스
public static class InteractableExtensions
{
    // IInteractable 인터페이스를 구현하는 객체에서 GameObject를 얻는 확장 메서드
    public static GameObject GetGameObject(this IInteractable interactable)
    {
        // IInteractable 인터페이스를 구현한 클래스는 반드시 MonoBehaviour여야 한다는 전제하에 캐스팅
        return (interactable as MonoBehaviour)?.gameObject;
    }
}
