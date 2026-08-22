using UnityEngine;

[CreateAssetMenu(fileName = "NewTimeObjectType", menuName = "Game/Time Object Type")]
public class TimeObjectTypeSO : ScriptableObject
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
    public string vitaminType;

    public TimeObjectType ToTimeObjectType()
    {
        return new TimeObjectType
        {
            typeName = this.typeName,
            sprite = this.sprite,
            color = this.color,
            pointValue = this.pointValue
        };
    }
}