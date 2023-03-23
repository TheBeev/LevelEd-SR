using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PlayerData : MonoBehaviour
{

	[SerializeField] private int startingHealth = 3;
	[SerializeField] private float invulnerabilityDuration = 1.5f;

	private int currentHealth;
	private bool bInvulnerable;
	private WaitForSeconds damageVibrationGap = new WaitForSeconds(0.25f);

	public int PlayerHealth
    {
		get { return currentHealth; }
    }


	// Use this for initialization
	void Start () 
	{
		currentHealth = startingHealth;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Enable()
    {
		currentHealth = startingHealth;
	}

	public void Disable()
    {
		currentHealth = startingHealth;
	}

	public void TakeDamage(int damageAmount)
    {
        if (!bInvulnerable)
        {
            if (currentHealth > 0)
            {
				currentHealth -= damageAmount;
				bInvulnerable = true;
				StartCoroutine(DamageVibration());
				Invoke("FinishInvulnerability", invulnerabilityDuration);

				if (SCR_ScoreboardManager.instance != null)
				{
					SCR_ScoreboardManager.instance.UpdatePlayerHealth(currentHealth);
				}

				print("Took Damage. Healh at: " + currentHealth);
			}
		}
    }

	IEnumerator DamageVibration()
    {
		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.LeftHand);
		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.RightHand);

		yield return damageVibrationGap;

		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.LeftHand);
		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.RightHand);

		yield return damageVibrationGap;

		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.LeftHand);
		SCR_OculusControllerVibrations.instance.ControllerVibrations(0.1f, 0.3f, ControllerHand.RightHand);

		yield return null;
    }

	void FinishInvulnerability()
    {
		bInvulnerable = false;
    }
}
