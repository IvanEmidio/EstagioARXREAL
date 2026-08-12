using UnityEngine;

[CreateAssetMenu(fileName = "Objects", menuName = "Scriptable Objects/Objects")]
public class Objects : ScriptableObject
{
    public enum Type { Meteor, Planet, Ship}
    public enum AmounToHit { Single, all};

    public Type type;
    public float Health;
    public GameObject GameObject;
    public AmounToHit amounToHit;
    

}
