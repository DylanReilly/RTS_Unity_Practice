using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    private void Awake()
    {
        //Subscribe to event
        health.ClientOnHealthUpdate += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        //Unsubscribe from event
        health.ClientOnHealthUpdate -= HandleHealthUpdated;
    }

    //Render health bar on mouse over
    private void OnMouseEnter()
    {
        healthBarParent.SetActive(true);
    }

    //Disable health bar on mouse exit
    private void OnMouseExit()
    {
        healthBarParent.SetActive(false);
    }

    //Update health bar UI using current health
    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
    }
}
