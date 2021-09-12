using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

namespace HybridJK.MultiplayerSurvival.Player
{
    public class Player : NetworkBehaviour
    {
        //Variables
        [SyncVar(hook = nameof(UpdateHealthUI))] private int health;
        private int currentHealthReductionTick = 0;
        private int hunger;
        private int saturation;

        [Header("Referances")]
        [SerializeField] private TextMeshProUGUI healthTextMeshPro;
        [SerializeField] private TextMeshProUGUI hungerTextMeshPro;
        [SerializeField] private Camera playerCamera;

        [Header("Player Settings")]
        [SerializeField] int playerLayer = 6;
        [SerializeField] int maxHealth = 100;
        [SerializeField] int maxHunger = 100;
        [SerializeField] int maxSaturation = 10;

        private void Start() //Update UI to current player starting health & hunger
        {
            // ----- Non-Local -----
            health = maxHealth;
            healthTextMeshPro.text = health.ToString();

            // ----- Local -----
            if (isLocalPlayer)
            {
                hunger = maxHunger;
                hungerTextMeshPro.text = hunger.ToString();
                saturation = maxSaturation;
            }
        }
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
            if (Input.GetMouseButtonDown(1)) //If right click
            {
                if (hunger == maxHunger && saturation == maxSaturation) { return; } //If hunger & saturation is over max, do not update
                if (hunger < maxHunger)
                {
                    UpdateHunger(hunger + 1);
                }
                else if (saturation < maxSaturation)
                {
                    UpdateSaturation(saturation + 1);
                }
            }
        }
        private void FixedUpdate()
        {
            if (!isLocalPlayer) { return; } //Kick non-local player from changing other client data
            HealthTick(); //Updates Hunger every tick
        }
        // -------------------- HEALTH METHODS --------------------

        public void UpdateHealthUI(int oldHealth, int newHealth) //Method is called if health value is changed, will update health UI
        {
            healthTextMeshPro.text = newHealth.ToString();
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

        // -------------------- HUNGER & SATURATION METHODS --------------------

        private void UpdateHunger(int amount)
        {
            if (amount > maxHunger)
            {
                hunger = maxHunger;
            }
            else if (hunger < 0)
            {
                hunger = 0;
            }
            else
            {
                hunger = amount;
            }
            hungerTextMeshPro.text = hunger.ToString();
        } //Validates and updates player hunger
        private void UpdateSaturation(int amount)
        {
            if (amount > maxSaturation)
            {
                saturation = maxSaturation;
            }
            else if (amount < 0)
            {
                saturation = 0;
            }
            else
            {
                saturation = amount;
            }
        } //Validates and updates player saturation
        private void HealthTick() //Update hunger system every tick
        {
            currentHealthReductionTick += 1;
            if (currentHealthReductionTick >= 20)
            {
                if (saturation > 0)
                {
                    UpdateSaturation(saturation - 1);
                }
                else if (hunger > 0)
                {
                    UpdateHunger(hunger - 1);
                }
                currentHealthReductionTick = 0;
            }
        }
    }
}