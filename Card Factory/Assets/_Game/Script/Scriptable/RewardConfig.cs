using UnityEngine;

[CreateAssetMenu(fileName = "RewardCOnfig", menuName = "Config/RewardCOnfig")]
public class GoldConfig : ScriptableObject
{
    public int coinReward;
    public int reviveCost;
    public int perHeartCost;
}
