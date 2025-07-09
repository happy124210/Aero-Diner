public class CustomerEventBridge
{
    public void Bind(Customer customer)
    {
        customer.OnOrderPlaced += order =>
        {
            EventBus.Raise(UIEventType.ShowOrderPanel, customer);
        };

        customer.OnServedStateChanged += isServed =>
        {
            if (isServed)
                EventBus.Raise(UIEventType.HideOrderPanel, customer);
        };
    }
}
