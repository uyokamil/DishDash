// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class OrderManager : MonoBehaviour
{
    // per level order manager. handles the types of orders that can be received, and the order queue

    [Header("Orders")]
    [SerializeField]
    private float newOrderSpeed;
    [SerializeField]
    private float orderTimeoutSpeed;
    
    [SerializeField]
    private FoodObject[] availableOrders;
    private List<Order> orderQueue = new List<Order>();

    [Header("UI")]
    [SerializeField]
    private Transform orderPositionPanel;
    [SerializeField]
    private Transform orderVisualPanel;
    [SerializeField]
    private Order orderPrefab;
    [SerializeField]
    private Transform orderPositonTransform;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    public AudioClip newOrderSound;
    public AudioClip orderCompleteSound;
    public AudioClip orderExpireSound;
    public AudioClip angrySound;

    public TextMeshProUGUI ordersLeftText;

    public LevelManager levelManager;

    public void StartOrders()
    {
        StartCoroutine(AddRandomOrderCoroutine());
    }

    public void StopOrders()
    {
        StopCoroutine(AddRandomOrderCoroutine());

        for (int i = orderQueue.Count - 1; i >= 0; i--)
        {
            Order order = orderQueue[i];
            order.Expire();
            orderQueue.Remove(order);
        }
    }

    private IEnumerator AddRandomOrderCoroutine()
    {
        while (true)
        {
            float pauseDuration = Random.Range(50f / newOrderSpeed, 70f / newOrderSpeed); // Calculate pause duration based on orderSpeed
            AddRandomOrder();
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private void AddRandomOrder()
    {
        if (availableOrders.Length == 0 || orderPrefab == null)
        {
            return;
        }

        int randomIndex = Random.Range(0, availableOrders.Length);
        FoodObject randomFoodObject = availableOrders[randomIndex];

        float orderTime = Time.time;
        float maxWaitDuration = Random.Range(orderTimeoutSpeed, orderTimeoutSpeed * 2);

        Transform positionTransform = Instantiate(orderPositonTransform, orderPositionPanel);
        Order newOrder = Instantiate(orderPrefab, orderVisualPanel);
        newOrder.Init(randomFoodObject, orderTime, maxWaitDuration, positionTransform, this);

        orderQueue.Add(newOrder);

        PlaySound(newOrderSound);

        // todo : add new order to the queue
        // orderQueue.Add(newOrder);
    }

    public bool CompleteOrder(FoodObject inFoodObject, bool hasPlate = false)
    {
        for (int i = 0; i < orderQueue.Count; i++)
        {
            Order order = orderQueue[i];

            if (inFoodObject.foodType == FoodType.Pizza && order.FoodObject.foodType == FoodType.Pizza && !order.FoodObject.foodIdentifier.Contains("raw") && !order.FoodObject.foodIdentifier.Contains("burn"))
            {
                PizzaObject inPizzaObject = inFoodObject as PizzaObject;
                PizzaObject orderPizzaObject = order.FoodObject as PizzaObject;

                if (inPizzaObject.toppings.SequenceEqual(orderPizzaObject.toppings))
                {
                    order.Complete();
                    orderQueue.RemoveAt(i);

                    PlaySound(orderCompleteSound);

                    if (levelManager)
                    {
                        levelManager.AddScore(Random.Range(10, 30));
                        levelManager.OrdersCompleted++;
                    }

                    return true;
                }
            }
            else if (order.FoodObject.foodIdentifier == inFoodObject.foodIdentifier)
            {
                if (hasPlate || inFoodObject.foodType == FoodType.Drink)
                {
                    order.Complete();
                    orderQueue.RemoveAt(i);

                    PlaySound(orderCompleteSound);

                    if (levelManager)
                    {
                        levelManager.AddScore(Random.Range(10, 30));
                        levelManager.OrdersCompleted++;
                    }

                    return true;
                }
                else
                {
                    order.NoPlateWarning();
                    return false;
                }
            }
        }

        return false;
    }

    public void ExpireOrder(Order expiredOrder)
    {
        if (orderQueue.Find(x => x == expiredOrder)) {
            orderQueue.Find(x => x == expiredOrder).Expire();
            orderQueue.Remove(expiredOrder);

            PlaySound(orderExpireSound);

            if (levelManager)
            {
                levelManager.OrdersMissed++;
            }
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
