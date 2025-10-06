// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public enum FoodType
{
    Food,
    Drink,
    Pizza
}

public enum PreparationMethod
{
    None,
    Wash,
    Chop,
    Fry,
    DeepFry,
    Combine,
    Soda,
    Coffee,
    Grate,
    Oven
}

public enum PreparationSpeed
{
    Normal,
    Instant
}

[CreateAssetMenu(fileName = "FoodObject", menuName = "Kitchen/Food Object", order = 1)]
public class FoodObject : ScriptableObject
{
    [Header("Food Details")]
    public string foodIdentifier;
    public FoodType foodType;

    [Header("Food Object")]
    public GameObject foodPrefab;
    public Sprite foodSprite;

    [Header("Preparation")]
    public PreparationMethod preparationMethod;
    public PreparationSpeed preparationSpeed;
    public FoodObject preparationResult;

    [Header("Combinations")]
    public FoodObject[] combineWith;
    public FoodObject[] combineResult;
}