using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SCR_Scoreboard : MonoBehaviour, ILevelItemInteractive
{
    [SerializeField] private TextMeshPro targetsLeftText = null;
    [SerializeField] private TextMeshPro timeLeftText = null;
    [SerializeField] private TextMeshPro winConditionText = null;

    private bool bAddedToInteractiveList;

    void Start()
    {
        if (!bAddedToInteractiveList)
        {
            SCR_LevelEditorManager.instance.AddPrefabsToInteractive(this.gameObject);
            bAddedToInteractiveList = true;
        }
    }

    void OnDisable()
    {
        SCR_LevelEditorManager.instance.RemovePrefabsFromInteractive(this.gameObject);
    }

    public void Enable()
    {
        Start();

        if (SCR_ScoreboardManager.instance)
        {
            SCR_ScoreboardManager.instance.RegisterScoreboard(targetsLeftText, timeLeftText, winConditionText);
        }
        
    }

    public void Disable()
    {
        //nothing
    }

}
