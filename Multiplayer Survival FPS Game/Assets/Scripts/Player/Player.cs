using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

namespace HybridJK.MultiplayerSurvival.PlayerCore
{
    public class Player : NetworkBehaviour
    {
        [Header("Referances")]
        [SerializeField] private TextMeshProUGUI healthObject;
        [SerializeField] private Camera playerCamera;

        [Header("Player Settings")]
        [SerializeField] int playerLayer = 6;
        [SerializeField] int maxHealth = 100; 
        [SyncVar(hook = nameof(UpdateHealthUI))] private int health;

        private void Update()
        {
            if (!isLocalPlayer) { return; } //Make sure client is not changing other player data
            if (Input.GetMouseButtonDown(0)) //If left click
            {
                RaycastHit hit;
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); //New raycast from center of camera

                if (Physics.Raycast(ray, out hit, Mathf.Infinity)) //If raycast has hit an object
                {
                    if (hit.transform.gameObject.layer == playerLayer) //If the object hit is a player
                    {
                        DamagePlayer(hit.transform); //Damage the other player
                    }
                }
            }
        }
        private void Start() //Update UI to current player starting health
        {
            health = maxHealth;
            healthObject.text = health.ToString();
        }
        public void UpdateHealthUI(int oldHealth, int newHealth) //Method is called if health value is changed, will update health UI
        {
            healthObject.text = newHealth.ToString();
        }
        public int GetHealth() //Get player health
        {
            return health;
        }
        private int ValidateHealth(int health) //Make sure the new health value is not over max health and not below zero
        {
            if (health > maxHealth)
            {
                return 100;
            }
            else if (health < 0)
            {
                return 0;
            }
            else
            {
                return health;
            }
        }
        [Command]
        public void DamagePlayer(Transform otherPlayer) //Send command to server to update other client health
        {
            int newHealth = otherPlayer.GetComponent<Player>().GetHealth() - 1;
            otherPlayer.GetComponent<Player>().health = ValidateHealth(newHealth);
        }
    }
}