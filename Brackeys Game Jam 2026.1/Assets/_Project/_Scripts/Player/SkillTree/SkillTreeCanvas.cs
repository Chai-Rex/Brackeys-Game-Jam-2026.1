
using System;
using TMPro;
using UnityEngine;

public class SkillTreeCanvas : MonoBehaviour
{
    private PlayerBlackboardHandler _blackboardHandler;
    [SerializeField] private TextMeshProUGUI skillPointText;

    private void Awake()
    {
        _blackboardHandler = PlayerHandler.Instance.Blackboard;
    }

    private void Start()
    {
        UpdateSkillPoints();
        
        //I know It's too much
        SkillTreeNode.upgradeDrill += UpdateByEvent;
        SkillTreeNode.upgradeHealth += UpdateByEvent;
        SkillTreeNode.upgradeMovement += UpdateByEvent;
    }

    private void OnDestroy()
    {
        SkillTreeNode.upgradeDrill -= UpdateByEvent;
        SkillTreeNode.upgradeHealth -= UpdateByEvent;
        SkillTreeNode.upgradeMovement -= UpdateByEvent;
    }

    private void UpdateByEvent(SkillUpgradeSO dummy)
    {
        UpdateSkillPoints();
        PanelOpened();
    }

    public void UpdateSkillPoints()
    {
        skillPointText.text = _blackboardHandler.skillPoints.ToString();
    }

    public static event Action panelOpened;
    public void PanelOpened()
    {
        UpdateSkillPoints();
        panelOpened?.Invoke();
    }
}