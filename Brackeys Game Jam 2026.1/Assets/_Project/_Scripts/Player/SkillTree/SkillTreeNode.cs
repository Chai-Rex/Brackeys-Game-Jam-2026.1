using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] private SkillUpgradeSO skillUpgrade;
    
    private bool unlocked=false, activated=false;
    
    [Header("Nodes")]
    public List<SkillTreeNode> precedingNodes;
    public List<SkillTreeNode> nextNodes;
    
    [Header("Button")]
    [SerializeField] private TextMeshProUGUI _nameText, resourceCostText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Color buttonDefault;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite movementIcon, drillIcon, healthIcon, movementBG,drillBG, healthBG;
    
    [Header("Connector UI")]
    [SerializeField] private GameObject connectorUIPrefab;
    public Transform connectorParent;
    public List<SkillNodeConnector> connectedConnectors;


    private void Awake()
    {
        _blackboard = PlayerHandler.Instance.Blackboard;
        AdjustToScriptableObject();
    }

    private PlayerBlackboardHandler _blackboard;
    private void Start()
    {
        IsThisUnlocked();
        SkillTreeCanvas.panelOpened +=RefreshResourceText;
    }

    private void OnDestroy()
    {
        SkillTreeCanvas.panelOpened -=RefreshResourceText;
    }

    private void RefreshResourceText()
    {
        if (activated) return;
        if (_blackboard.skillPoints < skillUpgrade.pointCost)
        {
            resourceCostText.color = Color.pink;
        }
        else
        {
            resourceCostText.color = Color.white;
        }
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
        timer = 0f;
        isLerping = true;
        startColor = buttonImage.color;
        endColor = changeTo;
        //buttonImage.color = changeTo;
    }

    private bool isLerping = false;
    private float timer = 0f;
    private Color startColor, endColor;
    private float duration = 0.5f;
    private void Update()
    {
        if (!isLerping) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        buttonImage.color = Color.Lerp(startColor, endColor, t);

        if (t >= 1f)
            isLerping = false;
    }

    public void OnClicked()
    {
        //Debug.Log($"Clicked unlocked:{unlocked} activated{activated}");

        if (_blackboard.skillPoints < skillUpgrade.pointCost)
        {//not enough points to spend on upgrade
            return;
        }

        if (unlocked && !activated)
        {
            _blackboard.skillPoints -= skillUpgrade.pointCost;
            ActivateUpgrade();
        }

    }

    //I realized too late that the player handler is a singleton where you can just grab and set the stats. 
    public static event Action<SkillUpgradeSO> upgradeMovement, upgradeDrill, upgradeHealth;
    private void ActivateUpgrade()
    {
        if(activated)return;
        activated = true;
        resourceCostText.text = "Unlocked";

        //Send event assuming the SO classifications are accurate and the SOs only have that type of upgrade
        switch (skillUpgrade.classification)
        {
            case UpgradeGeneralClassification.Drill:
                upgradeDrill?.Invoke(skillUpgrade);
                break;
            case UpgradeGeneralClassification.Health:
                upgradeHealth?.Invoke(skillUpgrade);
                break;
            case UpgradeGeneralClassification.Movement:
                upgradeMovement?.Invoke(skillUpgrade);
                break;
        }
        //
        
        ChangeButtonColor(Color.darkGreen);
        CheckUnlocks();
    }

    public void AdjustToScriptableObject()
    {
        if (skillUpgrade == null) return;
        
        //_nameText.text = skillUpgrade.SkillUpgradeName;
        resourceCostText.text = skillUpgrade.pointCost.ToString();
        
        switch (skillUpgrade.classification)
        {
            case UpgradeGeneralClassification.Drill:
                iconImage.sprite = drillIcon;
                buttonImage.sprite = drillBG;
                break;
            case UpgradeGeneralClassification.Health:
                iconImage.sprite = healthIcon;
                buttonImage.sprite = healthBG;
                break;
            case UpgradeGeneralClassification.Movement:
                iconImage.sprite = movementIcon;
                buttonImage.sprite = movementBG;
                break;
        }
    }
    
    #region Editor Functions
    
    public void SpawnNextNode()
    {
#if UNITY_EDITOR
        GameObject prefabRoot =
            UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

        if (prefabRoot == null)
        {
            Debug.LogWarning("This object is not a prefab instance.");
            return;
        }

        Transform thisParent = this.transform.parent;
        GameObject clone = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefabRoot,thisParent);

        clone.transform.position = transform.position + Vector3.right * 150f;
        clone.transform.rotation = transform.rotation;

        SkillTreeNode cloneSTN = clone.GetComponent<SkillTreeNode>();
        
        //add this node to the new node's preceding list
        cloneSTN.precedingNodes.Add(this);
        //add new node to next nodes of this node
        nextNodes.Add(cloneSTN);
        //spawn connector UI
        GameObject connectorUI =  (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(connectorUIPrefab, connectorParent);
        SkillNodeConnector skillNodeConnector = connectorUI.GetComponent<SkillNodeConnector>();
        
        skillNodeConnector.target = clone.GetComponent<RectTransform>();
        skillNodeConnector.from = GetComponent<RectTransform>();

        skillNodeConnector.fromSTN = this;
        skillNodeConnector.toSTN = cloneSTN;
        
        connectedConnectors.Add(skillNodeConnector);
        cloneSTN.connectedConnectors.Add(skillNodeConnector);
        cloneSTN.connectorParent = connectorParent;
#endif
    }

    

    #endregion
}
