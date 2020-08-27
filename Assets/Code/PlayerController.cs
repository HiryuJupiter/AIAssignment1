using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code
{
    public class PlayerController : MonoBehaviour
    {
        public int MaxHealth;

        int currentHealth;

        //Property
        public int GetHealth { get { return currentHealth; } }

        public float Speed;

        Rigidbody2D playerRigidbody;
        float moveHorizontal;
        float moveVertical;

        void Start()
        {
            //Initialize
            currentHealth = MaxHealth;

            //Reference
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            //Get the raw player input (for snappy movement)
            moveHorizontal = Input.GetAxisRaw("Horizontal");
            moveVertical = Input.GetAxisRaw("Vertical");

            Vector2 movement = new Vector2(moveHorizontal, moveVertical);

            //Set the rigidbody velocity based on input.
            playerRigidbody.velocity = movement * Speed;
        }


        public void TakeDamage(int dmg)
        {
            currentHealth -= dmg;
        }

        private void OnGUI()
        {
            //Use OnGUI to debug and check player's health.
            GUI.Label(new Rect(20, 40, 200, 20), "Player Health: " + currentHealth);
        }
    }
}