using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] private SkillUpgradeSO skillUpgrade;
    
    private bool unlocked, activated;
    [SerializeField] private SkillTreeNode[] precedingNodes;
    [SerializeField] private SkillTreeNode[] nextNodes;
    
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image buttonImage;
    

    private void Awake()
    {
        _nameText.text = skillUpgrade.SkillUpgradeName;
    }

    private void Start()
    {
        IsThisUnlocked();
    }

    private void IsThisUnlocked()
    {
        unlocked = true;
        ChangeButtonColor(Color.white);

        foreach (var node in precedingNodes)
        {
            if (!node.activated)
            {
                unlocked = false;
                ChangeButtonColor(Color.grey);
                return;
            }
        }
    }

    private void CheckUnlocks()
    {
        foreach (var node in nextNodes)
        {
            node.IsThisUnlocked();
        }
    }

    private void ChangeButtonColor(Color changeTo)
    {
        buttonImage.color = changeTo;
    }

    public void OnClicked()
    {
        if(unlocked && !activated)
            ActivateUpgrade();
    }

    public static event Action<SkillUpgradeSO> upgradeActivation;
    private void ActivateUpgrade()
    {
        if(activated)return;
        activated = true;
        
        
        //Send signal
        upgradeActivation?.Invoke(skillUpgrade);
        //
        
        ChangeButtonColor(Color.cyan);
        CheckUnlocks();
    }

    
    
}
