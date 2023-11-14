using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    public int health = 100;
    public int moveSpeed;
    public GameObject text;
    public GameObject self;
    public Camera playerCamera;
    public CharacterController enemyController;
    public PlayerMovement playerScript;
    public LevelLogic logicScript;

    // Start is called before the first frame update
    void Start()
    {
        ChangeDirection();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        logicScript = GameObject.FindGameObjectWithTag("Logic").GetComponent<LevelLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        text.transform.LookAt(GameObject.FindGameObjectWithTag("MainCamera").transform);
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        text.GetComponent<TMP_Text>().text = Convert.ToString(health);
        if (health <= 0)
        {
            Die();
        }
    }
    public void Move()
    {
        if(playerScript.isAiming)
            enemyController.Move(transform.forward * moveSpeed * Time.deltaTime / playerScript.slowingStrength);
        if (!playerScript.isAiming)
            enemyController.Move(transform.forward * moveSpeed * Time.deltaTime);
    }
    public void ChangeDirection()
    {
        self.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0f, 360f), 0);
        Invoke("ChangeDirection", 1f);
    }
    public void Die()
    {
        Destroy(gameObject);
        logicScript.SpawnEnemy();
        playerScript.CancelInvoke("ReloadAnimation");
        playerScript.isReloading = false;
    }
}
