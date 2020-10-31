using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Ghost : MonoBehaviour
{
    public float speed = 9.5f;
    public float nSpeed = 9.5f;
    public float fSpeed = 9.5f;
    public float tSpeed = 5f;
    public float cSpeed = 20f;
    public Node startingPosition;
    public Node homeNode;
    public int scatterTimer1 = 7;
    public int chaseTimer1 = 20;
    public int scatterTimer2 = 7;
    public int chaseTimer2 = 20;
    public int scatterTimer3 = 5;
    public int chaseTimer3 = 20;
    public float scatterTimer4 = 5;
    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;
    private float frightenedModeTimer = 0;
    public int frightenedModeDuration = 10;
    private float blinkTimer = 0;
    public int startBlinking = 7;
    private bool frightenedWhite = false;
    public float ghostReleaseTimer = 0;
    public int pinkyReleaseTimer = 5;
    public int inkyReleaseTimer = 14;
    public int clydeReleaseTimer = 21;
    public bool inGhostHouse = false;
    public RuntimeAnimatorController ghostUp;
    public RuntimeAnimatorController ghostLeft;
    public RuntimeAnimatorController ghostRight;
    public RuntimeAnimatorController ghostDown;
    public RuntimeAnimatorController scared;
    public RuntimeAnimatorController scared2;
    public Node ghostHouse;
    private AudioSource bgm;
    public Sprite eyesDown;
    public Sprite eyesLeft;
    public Sprite eyesRight;
    public Sprite eyesUp;
    private bool frightenedInit = false;
    public bool canMove = true;
    public enum Mode
    {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }
    public enum GhostType
    {
        Blinky,
        Pinky,
        Inky,
        Clyde
    }
    public GhostType ghostType = GhostType.Blinky;
    Mode cMode = Mode.Scatter;
    Mode pMode;
    private GameObject pacMan;
    private Node pNode, cNode, dNode;
    private Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        SetDifficultyForLevel(GameBoard.level);
        bgm = GameObject.Find("Game").transform.GetComponent<AudioSource>();
        pacMan = GameObject.FindGameObjectWithTag("PacMan");
        Vector2 tPos = transform.position * 2;
        Node tNode = GetNodeAtPos(tPos);
        if (tNode != null)
            cNode = tNode;
        if (inGhostHouse)
        {
            direction = cNode.validDirections[0];
            dNode = cNode.neighbors[0];
        }
        else
        {
            direction = Vector2.right;
            dNode = ChooseNextNode();
        }
        pNode = cNode;
        UpdateAnimatorController();
    }
    void SetDifficultyForLevel(int level)
    {
        if (level == 1)
        {
            speed = 7.5f;
            nSpeed = 7.5f;
            fSpeed = 5;
            tSpeed = 4;
            scatterTimer1 = 7;
            chaseTimer1 = 20;
            scatterTimer2 = 7;
            chaseTimer2 = 20;
            scatterTimer3 = 5;
            chaseTimer3 = 20;
            scatterTimer4 = 5;
            frightenedModeDuration = 10;
            startBlinking = 7;
            pinkyReleaseTimer = 0;
            inkyReleaseTimer = 6;
            clydeReleaseTimer = 18;
        }
        else if (level == 2)
        {
            speed = 8.5f;
            nSpeed = 8.5f;
            fSpeed = 5.5f;
            tSpeed = 4.5f;
            scatterTimer1 = 7;
            chaseTimer1 = 20;
            scatterTimer2 = 7;
            chaseTimer2 = 20;
            scatterTimer3 = 5;
            chaseTimer3 = 1033;
            scatterTimer4 = 1 / 60;
            frightenedModeDuration = 9;
            startBlinking = 6;
            pinkyReleaseTimer = 0;
            inkyReleaseTimer = 0;
            clydeReleaseTimer = 12;
        }
        else if (level >= 3 && level < 5)
        {
            speed = 8.5f;
            nSpeed = 8.5f;
            fSpeed = 5.5f;
            tSpeed = 4.5f;
            scatterTimer1 = 7;
            chaseTimer1 = 20;
            scatterTimer2 = 7;
            chaseTimer2 = 20;
            scatterTimer3 = 5;
            chaseTimer3 = 1033;
            scatterTimer4 = 1 / 60;
            frightenedModeDuration = 9;
            startBlinking = 6;
            pinkyReleaseTimer = 0;
            inkyReleaseTimer = 0;
            clydeReleaseTimer = 0;
        }
        else if (level >= 5 && level < 20)
        {
            speed = 9.5f;
            nSpeed = 9.5f;
            fSpeed = 6f;
            tSpeed = 5;
            scatterTimer1 = 5;
            chaseTimer1 = 20;
            scatterTimer2 = 5;
            chaseTimer2 = 20;
            scatterTimer3 = 5;
            chaseTimer3 = 1037;
            scatterTimer4 = 1 / 60;
            frightenedModeDuration = 8;
            startBlinking = 5;
            pinkyReleaseTimer = 0;
            inkyReleaseTimer = 0;
            clydeReleaseTimer = 0;
        }
        else
        {
            speed = 9.5f;
            nSpeed = 9.5f;
            fSpeed = 9.5f;
            tSpeed = 5;
            scatterTimer1 = 5;
            chaseTimer1 = 20;
            scatterTimer2 = 5;
            chaseTimer2 = 20;
            scatterTimer3 = 5;
            chaseTimer3 = 1037;
            scatterTimer4 = 1 / 60;
            frightenedModeDuration = 1/60;
            startBlinking = 1/120;
            pinkyReleaseTimer = 0;
            inkyReleaseTimer = 0;
            clydeReleaseTimer = 0;
        }
    }

    public void MoveToStart()
    {
        transform.position = startingPosition.transform.position;
        if (transform.name != "Blinky")
            inGhostHouse = true;
        cNode = startingPosition;
        if (inGhostHouse)
        {
            direction = cNode.validDirections[0];
            dNode = cNode.neighbors[0];
        }
        else
        {
            direction = Vector2.right;
            dNode = ChooseNextNode();
        }
        pNode = cNode;
        cMode = Mode.Scatter;
        UpdateAnimatorController();
    }

    public void Restart()
    {
        canMove = true;
        speed = nSpeed;
        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            CheckIsInGhostHouse();
            ModeUpdate();
            ReleaseGhost();
            Move();
            CheckCollision();
        }
    }

    void UpdateAnimatorController()
    {
        if (cMode != Mode.Frightened && cMode != Mode.Consumed)
        {
            if (direction == Vector2.down)
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostDown;
            else if (direction == Vector2.left)
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostLeft;
            else if (direction == Vector2.right)
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostRight;
            else
                transform.GetComponent<Animator>().runtimeAnimatorController = ghostUp;
        }
        else if (cMode == Mode.Frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = scared;
        }
        else
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = null;
            if (direction == Vector2.up)
                transform.GetComponent<SpriteRenderer>().sprite = eyesUp;
            else if (direction == Vector2.left)
                transform.GetComponent<SpriteRenderer>().sprite = eyesLeft;
            else if (direction == Vector2.right)
                transform.GetComponent<SpriteRenderer>().sprite = eyesRight;
            else
                transform.GetComponent<SpriteRenderer>().sprite = eyesDown;
        }
    }

    void ModeUpdate()
    {
        if (cMode != Mode.Frightened)
        {
            if (frightenedInit)
            {
                frightenedModeTimer += Time.deltaTime;
                if (frightenedModeTimer >= frightenedModeDuration)
                {
                    frightenedModeTimer = 0;
                    bgm.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().bgmNormal;
                    bgm.Play();
                    frightenedInit = false;
                }

            }
            modeChangeTimer += Time.deltaTime;
            if (modeChangeIteration == 1)
            {
                if (cMode == Mode.Scatter && modeChangeTimer > scatterTimer1)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (cMode == Mode.Chase && modeChangeTimer > chaseTimer1)
                {
                    modeChangeIteration = 2;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 2)
            {
                if (cMode == Mode.Scatter && modeChangeTimer > scatterTimer2)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (cMode == Mode.Chase && modeChangeTimer > chaseTimer2)
                {
                    modeChangeIteration = 3;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 3)
            {
                if (cMode == Mode.Scatter && modeChangeTimer > scatterTimer3)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (cMode == Mode.Chase && modeChangeTimer > chaseTimer3)
                {
                    modeChangeIteration = 4;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            }
            else if (modeChangeIteration == 4)
            {
                if (cMode == Mode.Scatter && modeChangeTimer > scatterTimer4)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        }
        else
        {
            frightenedInit = true;
            frightenedModeTimer += Time.deltaTime;
            if (frightenedModeTimer >= frightenedModeDuration)
            {
                frightenedModeTimer = 0;
                bgm.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().bgmNormal;
                bgm.Play();
                ChangeMode(pMode);
            }
            if (frightenedModeTimer >= startBlinking)
            {
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= 0.3f)
                {
                    blinkTimer = 0f;
                    if (frightenedWhite)
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = scared;
                        frightenedWhite = false;
                    }
                    else
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = scared2;
                        frightenedWhite = true;
                    }
                }
            }
        }
    }
    void ChangeMode (Mode m)
    {
        if (m == Mode.Frightened)
        {
            speed = fSpeed;
        }
        else if (m == Mode.Consumed)
        {
            speed = cSpeed;
        }
        else
        {
            speed = nSpeed;
        }
        if (cMode != Mode.Frightened && cMode != Mode.Consumed)
        {
            direction = -direction;
            Node tNode = dNode;
            dNode = pNode;
            pNode = tNode;
        }
        if (cMode != m)
            pMode = cMode;
        else
        {
            frightenedModeTimer = 0;
            transform.GetComponent<Animator>().runtimeAnimatorController = scared;
            frightenedWhite = false;
            blinkTimer = 0f;
        }
        cMode = m;
        UpdateAnimatorController();
    }
    public void StartFrightenedMode()
    {
        if (cMode != Mode.Consumed)
        {
            GameBoard.numGhostsConsumed = 200;
            frightenedModeTimer = 0;
            bgm.clip = GameObject.Find("Game").transform.GetComponent<GameBoard>().bgmFrightened;
            bgm.Play();
            ChangeMode(Mode.Frightened);
        }
    }
    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        if (ghostRect.Overlaps(pacManRect))
        {
            if (cMode == Mode.Frightened)
                Consumed();
            else if (cMode == Mode.Chase || cMode == Mode.Scatter)
            {
                GameObject.Find("Game").transform.GetComponent<GameBoard>().StartDeath();
            }
        }
    }
    void Consumed()
    {
        GameBoard.score += GameBoard.numGhostsConsumed;
        GameBoard.oneUp += GameBoard.numGhostsConsumed;
        ChangeMode(Mode.Consumed);
        UpdateAnimatorController();
        GameObject.Find("Game").transform.GetComponent<GameBoard>().StartConsumed(this.GetComponent<Ghost>());
        GameBoard.numGhostsConsumed *= 2;
    }

    Node GetNodeAtPos(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            return tile.GetComponent<Node>();
        }
        return null;
    }
    GameObject GetPortal(Vector2 pos)
    {
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];
        if (tile != null)
        {
            if (tile.GetComponent<Tile>() != null)
            {
                if (tile.GetComponent<Tile>().isPortal)
                {
                    GameObject otherPortal = tile.GetComponent<Tile>().portalReceiver;
                    return otherPortal;
                }
            }
        }
        return null;
    }
    float LengthFromNode(Vector2 pos)
    {
        Vector2 vec = pos - (Vector2)pNode.transform.position;
        return vec.sqrMagnitude;
    }
    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(dNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);
        return nodeToSelf > nodeToTarget;
    }
    float GetDistance(Vector2 a, Vector2 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        return distance;
    }

    Vector2 GetBlinkyTargetTile()
    {
        Vector2 pacmanPosition = pacMan.transform.position;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacmanPosition.x), Mathf.RoundToInt(pacmanPosition.y));
        return targetTile;
    }
    Vector2 GetPinkyTargetTile()
    {
        Vector2 pacmanPosition = pacMan.transform.position;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;
        int pacManPosX = Mathf.RoundToInt(pacmanPosition.x);
        int pacManPosY = Mathf.RoundToInt(pacmanPosition.y);
        Vector2 pacManTile = new Vector2(pacManPosX, pacManPosY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);
        return targetTile;
    }
    Vector2 GetInkyTargetTile()
    {
        Vector2 pacmanPosition = pacMan.transform.position;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;
        int pacManPosX = Mathf.RoundToInt(pacmanPosition.x);
        int pacManPosY = Mathf.RoundToInt(pacmanPosition.y);
        Vector2 pacManTile = new Vector2(pacManPosX, pacManPosY);
        Vector2 targetTile = pacManTile + (2 * pacManOrientation);
        Vector2 blinkyPosition = GameObject.Find("Blinky").transform.position;
        int blinkyPosX = Mathf.RoundToInt(blinkyPosition.x);
        int blinkyPosY = Mathf.RoundToInt(blinkyPosition.y);
        blinkyPosition = new Vector2(blinkyPosX, blinkyPosY);
        targetTile = new Vector2((2 * targetTile.x - blinkyPosition.x), (2 * targetTile.y - blinkyPosition.y));
        return targetTile;
    }
    Vector2 GetClydeTargetTile()
    {
        Vector2 pacmanPosition = pacMan.transform.position;
        float distance = GetDistance(transform.position, pacmanPosition);
        Vector2 targetTile = Vector2.zero;
        if (distance > 8)
            targetTile = new Vector2(Mathf.RoundToInt(pacmanPosition.x), Mathf.RoundToInt(pacmanPosition.y));
        else
            targetTile = homeNode.transform.position;
        return targetTile;
    }
    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;
        if (ghostType == GhostType.Blinky)
            targetTile = GetBlinkyTargetTile();
        else if (ghostType == GhostType.Pinky)
            targetTile = GetPinkyTargetTile();
        else if (ghostType == GhostType.Inky)
            targetTile = GetInkyTargetTile();
        else if (ghostType == GhostType.Clyde)
            targetTile = GetClydeTargetTile();
        return targetTile;
    }

    void ReleasePinky()
    {
        if (ghostType == GhostType.Pinky && inGhostHouse)
        {
            inGhostHouse = false;
            direction = cNode.validDirections[0];
            dNode = cNode.neighbors[0];
            pNode = cNode;
        }
    }
    void ReleaseInky()
    {
        if (ghostType == GhostType.Inky && inGhostHouse)
        {
            inGhostHouse = false;
            direction = cNode.validDirections[0];
            dNode = cNode.neighbors[0];
            pNode = cNode;
        }
    }
    void ReleaseClyde()
    {
        if (ghostType == GhostType.Clyde && inGhostHouse)
        {
            inGhostHouse = false;
            direction = cNode.validDirections[0];
            dNode = cNode.neighbors[0];
            pNode = cNode;
        }
    }
    void ReleaseGhost()
    {
        ghostReleaseTimer += Time.deltaTime;
        if (ghostReleaseTimer > pinkyReleaseTimer)
            ReleasePinky();
        if (ghostReleaseTimer > inkyReleaseTimer)
            ReleaseInky();
        if (ghostReleaseTimer > clydeReleaseTimer)
            ReleaseClyde();
    }

    GameObject GetTileAtPos(Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);
        GameObject tile = GameObject.Find("Game").transform.GetComponent<GameBoard>().board[tileX, tileY];
        if (tile != null)
            return tile;
        return null;
    }

    void CheckIsInGhostHouse()
    {
        if (cMode == Mode.Consumed)
        {
            Node node = GetNodeAtPos(transform.position * 2);
            if (node != null)
            {
                if(node == ghostHouse)
                {
                    speed = nSpeed;
                    cNode = node;
                    direction = node.validDirections[0];
                    dNode = cNode.neighbors[0];
                    pNode = cNode;
                    frightenedModeTimer = 0;
                    blinkTimer = 0;
                    frightenedWhite = false;
                    ChangeMode(Mode.Chase);
                    UpdateAnimatorController();
                }
            }
        }
    }

    Vector2 GetRandomTile()
    {
        int x = Random.Range(0, 28);
        int y = Random.Range(0, 36);
        return new Vector2(x, y);
    }

    Node ChooseNextNode()
    {
        Vector2 targetTile = Vector2.zero;
        if (cMode == Mode.Chase)
            targetTile = GetTargetTile();
        else if (cMode == Mode.Scatter)
            targetTile = homeNode.transform.position;
        else if (cMode == Mode.Frightened)
            targetTile = GetRandomTile();
        else
            targetTile = ghostHouse.transform.position;
        Node moveToNode = null;
        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];
        int nodeCounter = 0;
        for(int i = 0; i < cNode.neighbors.Length; i++)
        {
            if (cNode.validDirections[i] != -direction)
            {
                if (cMode != Mode.Consumed)
                {
                    GameObject tile = GetTileAtPos(cNode.transform.position * 2);
                    if (tile.transform.GetComponent<Tile>().isGhostHouseEntrance == true)
                    {
                        if (tile.transform.GetComponent<Tile>().isGhostHouse != true)
                        {
                            if (i < 2)
                            {
                                foundNodes[nodeCounter] = cNode.neighbors[i];
                                foundNodesDirection[nodeCounter] = cNode.validDirections[i];
                                nodeCounter++;
                            }
                        }
                        else
                        {
                            foundNodes[nodeCounter] = cNode.neighbors[0];
                            foundNodesDirection[nodeCounter] = cNode.validDirections[0];
                            nodeCounter++;
                        }
                    }
                    else
                    {
                        foundNodes[nodeCounter] = cNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = cNode.validDirections[i];
                        nodeCounter++;
                    }
                }
                else
                {
                    foundNodes[nodeCounter] = cNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = cNode.validDirections[i];
                    nodeCounter++;
                }
            }
        }
        float leastDistance = 100000f;
        for (int i = 0; i < 4; i++)
        {
            if (foundNodesDirection[i] != Vector2.zero)
            {
                float distance = GetDistance(foundNodes[i].transform.position, targetTile);
                if (distance < leastDistance)
                {
                    leastDistance = distance;
                    moveToNode = foundNodes[i];
                    direction = foundNodesDirection[i];
                }
            }
        }
        return moveToNode;
    }

    void Move()
    {
        if (dNode != cNode && dNode != null && !inGhostHouse)
        {
            if (OverShotTarget())
            {
                cNode = dNode;
                transform.localPosition = cNode.transform.position;
                GameObject otherPortal = GetPortal(cNode.transform.position * 2);
                if (otherPortal != null)
                {
                    transform.localPosition = otherPortal.transform.position;
                    cNode = otherPortal.GetComponent<Node>();
                }
                dNode = ChooseNextNode();
                pNode = cNode;
                cNode = null;
                UpdateAnimatorController();
            }
            else
            {
                if (pNode != null && cMode != Mode.Consumed)
                {
                    if (GetTileAtPos(pNode.transform.position * 2).GetComponent<Tile>().isPortal || GetTileAtPos(dNode.transform.position * 2).GetComponent<Tile>().isPortal)
                        speed = tSpeed;
                    else
                    {
                        if (cMode == Mode.Frightened)
                            speed = fSpeed;
                        else
                            speed = nSpeed;
                    }
                }
                transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;

            }
        }
    }
}
