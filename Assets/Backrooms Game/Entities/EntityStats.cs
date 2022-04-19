using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Place in entity GameObject to control stats / hitboxes

public class EntityStats : MonoBehaviour
{
    //Health
    public int maxHealth;
    public int health;
    public float agrivation;
    public int hunger;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
