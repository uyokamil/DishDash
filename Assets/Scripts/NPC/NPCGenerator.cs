// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using UnityEngine;

public class NPCGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] hairs;

    [SerializeField] private GameObject shirt;

    [SerializeField] private GameObject head;
    [SerializeField] private GameObject hands;
    [SerializeField] private GameObject nose;

    [SerializeField] private Color[] hairColors;
    [SerializeField] private Color[] skinColors;
    [SerializeField] private Color[] noseColors;

    [SerializeField] private Color[] shirtColors;
    [SerializeField] private Color[] pantsColors;

    [SerializeField] private Material materialReference;

    private void Awake()
    {
        GenerateNPC();
    }

    void GenerateNPC()
    {
        int hairIndex = Random.Range(0, hairs.Length);
        foreach (GameObject hair in hairs)
        {
            hair.SetActive(false);
        }
        hairs[hairIndex].SetActive(true);

        Color hairColor = hairColors[Random.Range(0, hairColors.Length)];
        Color shirtColor = shirtColors[Random.Range(0, shirtColors.Length)];
        Color pantsColor = pantsColors[Random.Range(0, pantsColors.Length)];

        // Create dynamic material for the shirt
        Material shirtMaterial = new Material(materialReference);
        shirtMaterial.color = shirtColor;

        Material[] shirtMaterials = shirt.GetComponent<Renderer>().sharedMaterials;
        shirtMaterials[0] = shirtMaterial;
        shirt.GetComponent<Renderer>().sharedMaterials = shirtMaterials;

        // Create dynamic material for the pants
        Material pantsMaterial = new Material(materialReference);
        pantsMaterial.color = pantsColor;

        Material[] pantsMaterials = shirt.GetComponent<Renderer>().sharedMaterials;
        pantsMaterials[1] = pantsMaterial;
        shirt.GetComponent<Renderer>().sharedMaterials = pantsMaterials;

        // Create dynamic material for the hair
        Material hairMaterial = new Material(materialReference);
        hairMaterial.color = hairColor;
        hairs[hairIndex].GetComponent<Renderer>().material = hairMaterial;

        // Create dynamic material for the head and hands
        Material skinMaterial = new Material(materialReference);
        Material noseMaterial = new Material(materialReference);

        int randomSkin = Random.Range(0, skinColors.Length);

        skinMaterial.color = skinColors[randomSkin];
        noseMaterial.color = noseColors[randomSkin];
        head.GetComponent<Renderer>().material = skinMaterial;
        hands.GetComponent<Renderer>().material = skinMaterial;
        nose.GetComponent<Renderer>().material = noseMaterial;
    }
}
