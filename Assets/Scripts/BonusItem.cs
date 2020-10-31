using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusItem : MonoBehaviour
{
    float randomLifeExpectancy;
    float currentLifeTime;
    // Start is called before the first frame update
    void Start()
    {
        randomLifeExpectancy = Random.Range(9f, 10f);
        int posX = Mathf.RoundToInt(this.gameObject.transform.position.x * 2);
        int posY = Mathf.RoundToInt(this.gameObject.transform.position.y * 2);
        GameObject.Find("Game").GetComponent<GameBoard>().board[posX, posY] = this.gameObject;
        this.name = "bonusItem";
    }

    // Update is called once per frame
    void Update()
    {
        if (currentLifeTime < randomLifeExpectancy)
            currentLifeTime += Time.deltaTime;
        else
            Destroy(this.gameObject);
    }
}
