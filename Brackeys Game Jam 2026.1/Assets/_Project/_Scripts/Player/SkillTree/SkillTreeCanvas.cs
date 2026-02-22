
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeCanvas : MonoBehaviour
{
    private PlayerBlackboardHandler _blackboardHandler;
    [SerializeField] private TextMeshProUGUI skillPointText;
    [SerializeField] private Button _iCloseButton;

    private void Awake()
    {
        _blackboardHandler = PlayerHandler.Instance.Blackboard;
        _iCloseButton.onClick.AddListener(CloseSkills);
    }

    private void Start()
    {
        //UpdateSkillPoints();
        
        //I know It's too much
        SkillTreeNode.upgradeDrill += UpdateByEvent;
        SkillTreeNode.upgradeHealth += UpdateByEvent;
        SkillTreeNode.upgradeMovement += UpdateByEvent;
    }

    private void OnEnable()
    {
        PanelOpened();
    }

    private void OnDestroy()
    {
        SkillTreeNode.upgradeDrill -= UpdateByEvent;
        SkillTreeNode.upgradeHealth -= UpdateByEvent;
        SkillTreeNode.upgradeMovement -= UpdateByEvent;

        _iCloseButton.onClick.RemoveAllListeners();
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

    public void CloseSkills() {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}