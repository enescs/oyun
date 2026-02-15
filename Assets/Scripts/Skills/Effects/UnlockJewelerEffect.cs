[System.Serializable]
public class UnlockJewelerEffect : SkillEffect
{
    public override void Apply()
    {
        SkillTreeManager.Instance.UnlockJeweler();
    }
}
