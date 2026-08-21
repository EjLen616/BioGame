using UnityEngine;

[CreateAssetMenu(fileName = "NewEndlessObjectType", menuName = "Game/Endless Object Type")]
public class EndlessObjectTypeSO : ScriptableObject
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
    public string vitaminType; // A, B, C, D, E, K

    public EndlessObjectType ToEndlessObjectType()
    {
        return new EndlessObjectType
        {
            typeName = this.typeName,
            sprite = this.sprite,
            color = this.color,
            pointValue = this.pointValue
        };
    }
}