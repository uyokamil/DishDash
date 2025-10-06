// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special version of the FoodObject for Pizza use. It contains a list of toppings that the pizza has (or should have, in the case of it being used as an Order object).
/// It derives all the normal FoodObject properties and methods, but also has a special list of toppings.
/// </summary>

[CreateAssetMenu(fileName = "FoodObject", menuName = "Kitchen/Pizza Order Object", order = 1)]
public class PizzaObject : FoodObject
{
    [Header("Pizza Order")]
    public List<FoodObject> toppings;
}