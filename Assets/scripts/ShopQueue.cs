using System.Collections.Generic;
using UnityEngine;

public class CashierQueue : MonoBehaviour
{
    [Header("Queue Layout")]
    [Tooltip("The position right in front of the counter where the first customer stands.")]
    public Transform lineStartSpot;
    
    [Tooltip("Distance between customers waiting in line.")]
    public float spacingBetweenCustomers = 1.2f;

    [Tooltip("List of customers currently waiting in line.")]
    public List<CustomerAI> waitingCustomers = new List<CustomerAI>();

    public int GetQueueCount()
    {
        waitingCustomers.RemoveAll(c => c == null);
        return waitingCustomers.Count;
    }

    public void JoinQueue(CustomerAI customer)
    {
        if (!waitingCustomers.Contains(customer))
        {
            waitingCustomers.Add(customer);
            UpdateAllCustomerPositions();
        }
    }

    public void LeaveQueue(CustomerAI customer)
    {
        if (waitingCustomers.Contains(customer))
        {
            waitingCustomers.Remove(customer);
            UpdateAllCustomerPositions(); // Shift everyone forward!
        }
    }

    // Calculates target position based on list index
    public Vector3 GetWaitingPosition(CustomerAI customer)
    {
        int index = waitingCustomers.IndexOf(customer);
        if (index <= 0) 
        {
            return lineStartSpot != null ? lineStartSpot.position : transform.position;
        }

        // Calculate position backwards from lineStartSpot along lineStartSpot's backward direction (-forward)
        Vector3 lineDirection = lineStartSpot != null ? -lineStartSpot.forward : -transform.forward;
        Vector3 basePos = lineStartSpot != null ? lineStartSpot.position : transform.position;

        return basePos + (lineDirection * (index * spacingBetweenCustomers));
    }

    private void UpdateAllCustomerPositions()
    {
        for (int i = 0; i < waitingCustomers.Count; i++)
        {
            if (waitingCustomers[i] != null)
            {
                Vector3 targetPos = GetWaitingPosition(waitingCustomers[i]);
                waitingCustomers[i].UpdateQueueTarget(targetPos);
            }
        }
    }
}