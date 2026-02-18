using UnityEngine;

[CreateAssetMenu(fileName = "SkillUnlockScriptableObject", menuName = "Scriptable Objects/SkillUnlockScriptableObject")]
public class SkillUpgradeSO : ScriptableObject
{
   public string SkillUpgradeName;
   public SkillUpgradeDataObject[] SkillUpgrades;
}
