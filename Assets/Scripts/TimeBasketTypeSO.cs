using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTimeBasketType", menuName = "Game/Time Basket Type")]
public class TimeBasketTypeSO : ScriptableObject
{
    public string basketName;
    public Sprite[] healthStages;
    public Color[] stageColors;
    public List<string> requiredVitamins;
    public GameObject checkmarkPrefab;
    public GameObject redXPrefab;

    public bool AcceptsVitamin(string vitaminType)
    {
        if (string.IsNullOrEmpty(vitaminType))
            return false;

        return requiredVitamins.Contains(vitaminType);
    }
}