using UnityEngine;

[CreateAssetMenu(fileName = "SkillUnlockScriptableObject", menuName = "Scriptable Objects/SkillUnlockScriptableObject")]
public class SkillUpgradeSO : ScriptableObject
{
   public string SkillUpgradeName;
   public UpgradeGeneralClassification classification;
   public int pointCost = 0;
   public SkillUpgradeDataObject[] SkillUpgrades;
   
}
