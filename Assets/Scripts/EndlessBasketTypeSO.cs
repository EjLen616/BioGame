using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBasketType", menuName = "Game/Basket Type")]
public class EndlessBasketTypeSO : ScriptableObject
{
    public string basketName;
    public Sprite[] healthStages; // 7 stages: index 0 = -3 (worst), index 6 = +3 (best)
    public Color[] stageColors;
    public List<string> requiredVitamins; // Vitamin types this basket accepts (e.g., "A", "B", "C")
    public GameObject checkmarkPrefab;
    public GameObject redXPrefab;

    public bool AcceptsVitamin(string vitaminType)
    {
        if (string.IsNullOrEmpty(vitaminType))
            return false;

        return requiredVitamins.Contains(vitaminType);
    }
}