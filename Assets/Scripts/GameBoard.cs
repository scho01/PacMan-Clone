using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameBoard : MonoBehaviour
{
    private static int boardWidth = 56;
    private static int boardHeight = 72;
    public int totalPellets = 0;
    public int pelletsConsumed = 0;
    public static int score = 0;
    public static int level = 1;
    public static int pacManLives = 3;
    public static bool oneUpTriggered = false;
    public bool shouldBlink = false;
    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;
    public static int numGhostsConsumed = 0;
    public GameObject[,] board = new GameObject[boardWidth, boardHeight];
    public AudioClip bgmNormal;
    public AudioClip bgmFrightened;
    public AudioClip pacManDeathAudio;
    private bool didStartDeath = false;
    public Text playerText;
    public Text readyText;
    private bool didStartConsumed = false;
    public static int oneUp = 0;
    public AudioClip consumedGhostAudio;
    public Text highScoreText;
    public Text oneUpText;
    public Text oneUpScoreText;
    public Image playerLives2;
    public Image playerLives3;
    public Text consumedGhostScoreText;
    public Sprite mazeBlue;
    public Sprite mazeWhite;
    public Sprite[] bonusItemSprite;
    public Image[] levelImages;
    bool didSpawnBonusItem1;
    bool didSpawnBonusItem2;

    // Start is called before the first frame update
    void Start()
    {
        if (level <= 20)
        {
            for (int i = 0; i < level; i++)
                levelImages[i].enabled = true;
        }
        else
        {
            for (int i = 0; i < 20; i++)
                levelImages[i].enabled = true;
        }
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach(GameObject o in objects)
        {
            Vector2 pos = o.transform.position;
            if (!o.CompareTag("PacMan") && !o.CompareTag("Ghost") && !o.CompareTag("Maze") && !o.CompareTag("GhostHome") && !o.CompareTag("UI") && !o.CompareTag("MainCamera"))
            {
                if (o.GetComponent<Tile>() != null)
                {
                    if (o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isPowerUp)
                        totalPellets++;
                }
                Vector2 tPos = pos * 2;
                board[(int)tPos.x, (int)tPos.y] = o;
            }
        }
        if (level == 1)
            GetComponent<AudioSource>().Play();
        StartGame();
    }

    void Update()
    {
        UpdateUI();
        CheckShouldBlink();
        BonusItems();
    }

    void BonusItems()
    {
        if (pelletsConsumed >= 70 && pelletsConsumed < 170)
        {
            if (!didSpawnBonusItem1)
            {
                didSpawnBonusItem1 = true;
                ActivateBonusItem(level);
            }
        }
        else if (pelletsConsumed >= 170)
        {
            if (!didSpawnBonusItem2)
            {
                ActivateBonusItem(level);
                didSpawnBonusItem2 = true;
            }
        }
    }

    void ActivateBonusItem(int level)
    {
        GameObject bonusItem = GameObject.FindGameObjectWithTag("BonusItem");
        if (level == 1)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[0];
            bonusItem.transform.GetComponent<Tile>().pointValue = 100;
        }
        else if (level == 2)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[1];
            bonusItem.transform.GetComponent<Tile>().pointValue = 300;
        }
        else if (level == 3 || level == 4)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[2];
            bonusItem.transform.GetComponent<Tile>().pointValue = 500;
        }
        else if (level == 5 || level == 6)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[3];
            bonusItem.transform.GetComponent<Tile>().pointValue = 700;
        }
        else if (level == 7 || level == 8)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[4];
            bonusItem.transform.GetComponent<Tile>().pointValue = 1000;
        }
        else if (level == 9 || level == 10)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[5];
            bonusItem.transform.GetComponent<Tile>().pointValue = 2000;
        }
        else if (level == 11 || level == 12)
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[6];
            bonusItem.transform.GetComponent<Tile>().pointValue = 3000;
        }
        else
        {
            bonusItem.transform.GetComponent<SpriteRenderer>().sprite = bonusItemSprite[7];
            bonusItem.transform.GetComponent<Tile>().pointValue = 5000;
        }
        bonusItem.transform.GetComponent<SpriteRenderer>().enabled = true;
        bonusItem.transform.GetComponent<Tile>().bonusItemActive = true;

    }

    void UpdateUI()
    {
        if (totalPellets == pelletsConsumed)
        {
            PlayerWin();
        }
        highScoreText.text = score.ToString();
        if (oneUp >= 10000)
        {
            pacManLives += 1;
            oneUp -= 10000;
        }
        oneUpScoreText.text = oneUp.ToString();
        if (pacManLives == 3)
        {
            playerLives3.enabled = true;
            playerLives2.enabled = true;
        }
        else if (pacManLives == 2)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = true;
        }
        else if (pacManLives == 1)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = false;
        }
    }

    void PlayerWin()
    {
        StartCoroutine(ProcessWin(2));
    }

    IEnumerator ProcessWin (float delay)
    {
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;
        transform.GetComponent<AudioSource>().Stop();
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
            ghost.transform.GetComponent<Animator>().enabled = false;
        }
        yield return new WaitForSeconds(delay);
        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard (float delay)
    {
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        shouldBlink = true;
        yield return new WaitForSeconds(delay);
        shouldBlink = false;
        StartNextLevel();
    }

    private void StartNextLevel()
    {
        level += 1;
        if (level > 255)
            SceneManager.LoadScene("VictoryScreen");
        pelletsConsumed = 0;
        didSpawnBonusItem1 = false;
        didSpawnBonusItem2 = false;
        SceneManager.LoadScene("Level1");
    }

    private void CheckShouldBlink()
    {
        if (shouldBlink)
        {
            if (blinkIntervalTimer < blinkIntervalTime)
                blinkIntervalTimer += Time.deltaTime;
            else
            {
                blinkIntervalTimer = 0;
                if (GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite == mazeBlue)
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeWhite;
                else
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;
            }
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartBlinking(oneUpText));
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(ShowObjectsAfter(2.25f));
    }

    public void StartConsumed (Ghost consumedGhost)
    {
        if (!didStartConsumed)
        {
            didStartConsumed = true;
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }
            GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
            consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            Vector2 pos = consumedGhost.transform.position;
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;
            consumedGhostScoreText.text = numGhostsConsumed.ToString();
            consumedGhostScoreText.GetComponent<Text>().enabled = true;
            transform.GetComponent<AudioSource>().PlayOneShot(consumedGhostAudio);
            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    public void ConsumingBonusItem (GameObject bonusItem, int scoreVal)
    {
        Vector2 pos = bonusItem.transform.position;
        Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);
        consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
        consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;
        consumedGhostScoreText.text = scoreVal.ToString();
        consumedGhostScoreText.GetComponent<Text>().enabled = true;
        bonusItem.transform.GetComponent<Tile>().bonusItemActive = false;
        bonusItem.transform.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(ProcessConsumedBonusItem(0.75f));
    }

    IEnumerator ProcessConsumedBonusItem (float delay)
    {
        yield return new WaitForSeconds(delay);
        consumedGhostScoreText.GetComponent<Text>().enabled = false;
    }

    IEnumerator StartBlinking (Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);
        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }

    IEnumerator ProcessConsumedAfter (float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);
        consumedGhostScoreText.GetComponent<Text>().enabled = false;
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        pacMan.transform.GetComponent<PacMan>().canMove = true;
        consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }
        transform.GetComponent<AudioSource>().Play();
        didStartConsumed = false;
    }

    IEnumerator ShowObjectsAfter (float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        playerText.transform.GetComponent<Text>().enabled = false;
        StartCoroutine(StartGameAfter(2));
    }

    IEnumerator StartGameAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
            ghost.transform.GetComponent<Ghost>().canMove = true;
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = true;
        readyText.transform.GetComponent<Text>().enabled = false;
        transform.GetComponent<AudioSource>().clip = bgmNormal;
        transform.GetComponent<AudioSource>().Play();
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            GameObject bonusItem = GameObject.Find("bonusItem");
            if (bonusItem)
                Destroy(bonusItem.gameObject);
            didStartDeath = true;
            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in o)
                ghost.transform.GetComponent<Ghost>().canMove = false;
            GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;
            pacMan.transform.GetComponent<Animator>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            StartCoroutine(ProcessDeathAfter(2));

        }

    }

    IEnumerator ProcessDeathAfter (float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(ProcessDeathAnimation(1.9f));
    }

    IEnumerator ProcessDeathAnimation (float delay)
    {
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.localScale = new Vector3 (1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;
        transform.GetComponent<AudioSource>().clip = pacManDeathAudio;
        transform.GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(delay);
        StartCoroutine(ProcessRestart(1));


    }

    IEnumerator ProcessRestart (float delay)
    {
        pacManLives -= 1;
        if (pacManLives == 0)
        {
            readyText.transform.GetComponent<Text>().text = "GAME OVER";
            readyText.transform.GetComponent<Text>().color = Color.red;
            readyText.transform.GetComponent<Text>().enabled = true;
            GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            StartCoroutine(ProcessGameOver(5));
        }
        else
        {
            playerText.transform.GetComponent<Text>().enabled = true;
            readyText.transform.GetComponent<Text>().enabled = true;
            GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            yield return new WaitForSeconds(delay);
            StartCoroutine(ProcessRestartShowObjects(1));
        }
    }

    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("GameMenu");
    }

    IEnumerator ProcessRestartShowObjects (float delay)
    {
        playerText.transform.GetComponent<Text>().enabled = false;
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
            ghost.transform.GetComponent<Ghost>().MoveToStart();

        }
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        pacMan.transform.GetComponent<PacMan>().MoveToStart();
        yield return new WaitForSeconds(delay);
        Restart();
    }

    public void Restart()
    {
        readyText.transform.GetComponent<Text>().enabled = false;
        GameObject pacMan = GameObject.FindGameObjectWithTag("PacMan");
        pacMan.transform.GetComponent<PacMan>().Restart();
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in o)
            ghost.transform.GetComponent<Ghost>().Restart();
        transform.GetComponent<AudioSource>().clip = bgmNormal;
        transform.GetComponent<AudioSource>().Play();
        didStartDeath = false;
    }

}
