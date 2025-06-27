using Dreamteck;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FeatureUnlockConfig", menuName = "Scriptable Objects/FeatureUnlockConfig")]
public class FeatureUnlockConfig : ScriptableObject
{
    public List<FeatureUnlockData> featureUnlock = new List<FeatureUnlockData>();

    public FeatureUnlockData GetFeatureData(int idcheck)
    {
        foreach (var feature in featureUnlock)
        {
            if(idcheck == feature.featureID)
            {
                return feature;
            }
        }
        return null;
    }
}

[System.Serializable]
public class FeatureUnlockData
{
    public int featureID;
    public string featureName;
    public int levelUnlock;
    public string description;
    public Sprite featureIcon;
}