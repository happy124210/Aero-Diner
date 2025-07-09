using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerOrderPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject customerEntryPrefab;

    private readonly List<Customer> customers = new();
    private readonly Dictionary<Customer, CustomerOrderEntryUI> uiMap = new();


    public void ShowOrderPanel(Customer customer)
    {
        if (customers.Contains(customer)) return;

        customers.Add(customer);
        var go = Instantiate(customerEntryPrefab, contentParent);
        var entry = go.GetComponent<CustomerOrderEntryUI>();
        if (entry == null) return;

        entry.Init(customer.CurrentOrder.foodIcon);
        uiMap[customer] = entry;
    }

    public void HideOrderPanel(Customer customer)
    {
        if (!customers.Contains(customer)) return;

        customers.Remove(customer);
        if (uiMap.TryGetValue(customer, out var entry))
        {
            Destroy(entry.gameObject);
            uiMap.Remove(customer);
        }
    }

    private void Update()
    {
        foreach (var customer in customers)
        {
            if (!customer.HasPatience || !uiMap.ContainsKey(customer)) continue;

            uiMap[customer].UpdatePatienceColor(customer.CurrentPatience / customer.MaxPatience);
        }
    }
}
