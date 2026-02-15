using UnityEngine;

[CreateAssetMenu(menuName = "SkillTree/Products/JewelerProduct")]
public class JewelerProduct : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public JewelerLocationType location;
    public int cost; //satın alma fiyatı
}
