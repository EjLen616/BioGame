using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectType", menuName = "Game/Object Type")]
public class ObjectTypeSO : ScriptableObject
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
    public string[] acceptedBaskets;

    public ObjectType ToObjectType()
    {
        return new ObjectType
        {
            typeName = this.typeName,
            sprite = this.sprite,
            color = this.color,
            pointValue = this.pointValue,
            acceptedBaskets = this.acceptedBaskets
        };
    }
}