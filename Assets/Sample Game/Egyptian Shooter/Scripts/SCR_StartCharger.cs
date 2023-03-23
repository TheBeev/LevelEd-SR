using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SCR_StartCharger : MonoBehaviour, ILevelItemInteractive
{
	[SerializeField] private Material gameStartedMaterial;
	[SerializeField] private Material gameOverMaterial;
	[SerializeField] private Image chargeCircleImage;
	[SerializeField] private TextMeshProUGUI startText;
	[SerializeField] private Renderer rend;
	[SerializeField] private float timeToCharge = 1f;

	private bool bAddedToInteractiveList;
	private bool bCurrentlyCharging;
	private bool bGameMode;
	private float currentChargeAmount;

	// Use this for initialization
	void Start () 
	{
		if (!bAddedToInteractiveList)
		{
			SCR_LevelEditorManager.instance.AddPrefabsToInteractive(this.gameObject);
			bAddedToInteractiveList = true;
		}
	}

	public void Enable()
	{
		Start();

		if (SCR_ScoreboardManager.instance)
		{
			SCR_ScoreboardManager.instance.RegisterChargers(this);
		}

		bGameMode = true;
	}

	public void Disable()
    {
		startText.text = "Start";
		chargeCircleImage.gameObject.SetActive(true);
		rend.sharedMaterial = gameOverMaterial;
		currentChargeAmount = 0f;
		bGameMode = false;
	}

	public void ChangeColour(bool bGameStarted)
    {
        if (bGameStarted)
        {
			chargeCircleImage.fillAmount = 0f;
			currentChargeAmount = 0f;
			chargeCircleImage.gameObject.SetActive(false);
			startText.text = "Go!";
			rend.sharedMaterial = gameStartedMaterial;
		}
        else
        {
			startText.text = "Start";
			chargeCircleImage.gameObject.SetActive(true);
			rend.sharedMaterial = gameOverMaterial;
        }
    }

	void OnTriggerEnter(Collider other)
    {
		if (bGameMode)
		{
			if (other.CompareTag("GGControllerCollisionLeft"))
			{
				bCurrentlyCharging = true;
				float durationRemaining = timeToCharge - currentChargeAmount;
				SCR_OculusControllerVibrations.instance.ControllerVibrations(durationRemaining, 0.7f, ControllerHand.LeftHand);
			}
			else if (other.CompareTag("GGControllerCollisionRight"))
			{
				bCurrentlyCharging = true;
				float durationRemaining = timeToCharge - currentChargeAmount;
				SCR_OculusControllerVibrations.instance.ControllerVibrations(durationRemaining, 0.7f, ControllerHand.RightHand);
			}
		}
    }

	void OnTriggerExit(Collider other)
	{
        if (bGameMode)
        {
			if (other.CompareTag("GGControllerCollisionLeft"))
			{
				bCurrentlyCharging = false;
				SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.LeftHand);
			}
			else if (other.CompareTag("GGControllerCollisionRight"))
			{
				bCurrentlyCharging = false;
				SCR_OculusControllerVibrations.instance.ControllerVibrations(0.01f, 0.1f, ControllerHand.RightHand);
			}
		}
	}

	void OnDisable()
    {
		SCR_LevelEditorManager.instance.RemovePrefabsFromInteractive(this.gameObject);
	}

	// Update is called once per frame
	void Update () 
	{
        if (SCR_LevelEditorManager.instance.CurrentEditorState == EditorState.Game)
        {
            if (SCR_ScoreboardManager.instance != null)
            {
				if (SCR_ScoreboardManager.instance.GameOver && bCurrentlyCharging)
				{
                    if (SCR_ScoreboardManager.instance.NeedsResetting)
                    {
						SCR_LevelEditorManager.instance.ToggleEditorState();
						SCR_LevelEditorManager.instance.ToggleEditorState();
						SCR_ScoreboardManager.instance.NeedsResetting = false;
					}

					currentChargeAmount += Time.deltaTime;

					if (currentChargeAmount >= timeToCharge)
					{
						currentChargeAmount = 0f;
						SCR_ScoreboardManager.instance.StartGameFromChargers();
					}
				}
                else
                {
					currentChargeAmount -= Time.deltaTime;
					currentChargeAmount = Mathf.Max(0, currentChargeAmount);

				}

				UpdateChargeUI();
			}
            
        }
	}

	void UpdateChargeUI()
    {
		chargeCircleImage.fillAmount = currentChargeAmount / timeToCharge;
	}
}
