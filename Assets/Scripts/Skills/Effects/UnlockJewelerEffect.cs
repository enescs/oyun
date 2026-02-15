[System.Serializable]
public class UnlockJewelerEffect : SkillEffect
{
    public int jewelerCost = 5000; //bir kuyumcu satın alma maliyeti

    public override void Apply()
    {
        SkillTreeManager.Instance.UnlockJeweler(jewelerCost);
    }
}
