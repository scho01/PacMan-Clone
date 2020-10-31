using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public bool isPortal;
    public bool isPellet;
    public bool isPowerUp;
    public bool consumed;
    public GameObject portalReceiver;
    public bool isGhostHouseEntrance;
    public bool isGhostHouse;
    public bool isBonusItem;
    public bool bonusItemActive;
    public int pointValue;
    float randomLifeExpectancy;
    float currentLifeTime;
    // Start is called before the first frame update
    void Start()
    {
        if (isBonusItem)
            randomLifeExpectancy = Random.Range(9f, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isBonusItem && bonusItemActive)
        {
            if (currentLifeTime < randomLifeExpectancy)
                currentLifeTime += Time.deltaTime;
            else
                bonusItemActive = false;
        }
    }
}
