using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PacMan : MonoBehaviour {
	public AudioClip chomp1;
	public AudioClip chomp2;
	public Vector2 orientation;
	public float speed = 9f;
	public Sprite idleSprite;
	private Vector2 direction = Vector2.left;
	private Vector2 nDirection;
	private Node pNode, cNode, dNode;
	private bool playedChomp1 = false;
	private AudioSource audioS;
	private Vector2 startingPos = new Vector2(13.5f, 7);
	public bool canMove = true;
	public RuntimeAnimatorController chompAnimation;
	public RuntimeAnimatorController deathAnimation;

	// Use this for initialization
	void Start () {
		audioS = transform.GetComponent<AudioSource>();
		pNode = GetNodeAtPos(GameObject.FindGameObjectWithTag("Init_pNode").transform.position);
		cNode = null;
		dNode = GetNodeAtPos(GameObject.FindGameObjectWithTag("Init_dNode").transform.position);
		nDirection = direction;
		Move();
		UpdateOrientation();
		SetDifficultyForLevel(GameBoard.level);
	}

	public void MoveToStart()
    {
		transform.GetComponent<Animator>().runtimeAnimatorController = chompAnimation;
		transform.GetComponent<Animator>().enabled = true;
		transform.position = startingPos;
		direction = Vector2.left;
		pNode = GetNodeAtPos(GameObject.FindGameObjectWithTag("Init_pNode").transform.position);
		cNode = null;
		dNode = GetNodeAtPos(GameObject.FindGameObjectWithTag("Init_dNode").transform.position);
		nDirection = direction;
		UpdateOrientation();
	}

	void SetDifficultyForLevel (int level)
    {
		if (level == 1)
			speed = 8;
		else if (level >= 5 && level < 20)
			speed = 10;
		else
			speed = 9;
    }

	public void Restart()
	{
		canMove = true;
		audioS = transform.GetComponent<AudioSource>();
		Move();
	}

	// Update is called once per frame
	void Update () {
		if (canMove)
        {
			CheckInput();
			Move();
			UpdateOrientation();
			UpdateAnimation();
			ConsumePellet();
		}
	}

	void PlaySound()
    {
		if (playedChomp1)
        {
			audioS.PlayOneShot(chomp2);
			playedChomp1 = false;
        }
        else
        {
			audioS.PlayOneShot(chomp1);
			playedChomp1 = true;
		}
	}

	void CheckInput () {

		if (Input.GetKeyDown (KeyCode.J)) {
			ChangePosition(Vector2.left);
		} else if (Input.GetKeyDown (KeyCode.L)) {
			ChangePosition(Vector2.right);
		}
		else if (Input.GetKeyDown (KeyCode.I)) {
			ChangePosition(Vector2.up);
		}
		else if (Input.GetKeyDown (KeyCode.M)) {
			ChangePosition(Vector2.down);
		}
	}

	void ChangePosition(Vector2 d)
    {
		if (d != direction)
			nDirection = d;
		if (cNode != null)
        {
			Node moveToNode = CanMove(d);
			if (moveToNode != null)
            {
				direction = d;
				dNode = moveToNode;
				pNode = cNode;
				cNode = null;
            }
        }
        else
        {
			if (nDirection == -direction)
            {
				Node tNode = pNode;
				pNode = dNode;
				dNode = tNode;
				direction = nDirection;
            }
        }
    }

	void Move () {
		if (dNode != cNode && dNode != null)
        {
			if (OverShotTarget())
            {
				cNode = dNode;
				transform.localPosition = cNode.transform.position;
				GameObject otherPortal = GetPortal(cNode.transform.position);
				if (otherPortal != null)
                {
					transform.localPosition = otherPortal.transform.position;
					cNode = otherPortal.GetComponent<Node>();
                }
				Node moveToNode = CanMove(nDirection);
				if (moveToNode != null)
					direction = nDirection;
				if (moveToNode == null)
					moveToNode = CanMove(direction);
				if (moveToNode != null)
				{
					dNode = moveToNode;
					pNode = cNode;
					cNode = null;
				}
				else
					direction = Vector2.zero;
			}
			else
				transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;
		}
	}

	void UpdateOrientation () {

		if (direction == Vector2.left) {
			orientation = Vector2.left;
			transform.localScale = new Vector3 (-1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 0);

		} else if (direction == Vector2.right) {
			orientation = Vector2.right;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 0);

		} else if (direction == Vector2.up) {
			orientation = Vector2.up;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 90);

		} else if (direction == Vector2.down) {
			orientation = Vector2.down;
			transform.localScale = new Vector3 (1, 1, 1);
			transform.localRotation = Quaternion.Euler (0, 0, 270);
		}
	}

	void UpdateAnimation()
    {
		if (direction == Vector2.zero)
		{
			GetComponent<Animator>().enabled = false;
			GetComponent<SpriteRenderer>().sprite = idleSprite;
		}
		else
			GetComponent<Animator>().enabled = true;
    }

	Node CanMove (Vector2 d)
    {
		Node moveToNode = null;
		for (int i = 0; i < cNode.neighbors.Length; i++)
        {
			if (cNode.validDirections[i] == d)
            {
				moveToNode = cNode.neighbors[i];
				break;
            }
        }
		return moveToNode;
    }

	Node GetNodeAtPos (Vector2 pos)
    {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)(pos.x * 2), (int)(pos.y * 2)];
		if (tile != null)
        {
			return tile.GetComponent<Node>();
        }
		return null;
    }

	bool OverShotTarget()
    {
		float nodeToTarget = LengthFromNode(dNode.transform.position);
		float nodeToSelf = LengthFromNode(transform.localPosition);
		return nodeToSelf > nodeToTarget;
    }

	float LengthFromNode (Vector2 pos)
    {
		Vector2 vec = pos - (Vector2)pNode.transform.position;
		return vec.sqrMagnitude;
    }

	GameObject GetPortal (Vector2 pos)
    {
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[(int)(pos.x * 2), (int)(pos.y * 2)];
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

	GameObject GetTileAtPosition (Vector2 pos)
    {
		int tileX = Mathf.RoundToInt(pos.x * 2);
		int tileY = Mathf.RoundToInt(pos.y * 2);
		GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board[tileX, tileY];
		if (tile != null)
			return tile;
		return null;
	}

	void ConsumePellet()
	{
		GameObject o = GetTileAtPosition(transform.position);
		{
			if (o != null)
			{
				Tile tile = o.GetComponent<Tile>();
				if (tile != null)
                {
					if (!tile.consumed && (tile.isPellet || tile.isPowerUp))
                    {
						o.GetComponent<SpriteRenderer>().enabled = false;
						tile.consumed = true;
						if (tile.isPowerUp)
                        {
							GameBoard.score += 50;
							GameBoard.oneUp += 50;
						}
						else
                        {
							GameBoard.score += 10;
							GameBoard.oneUp += 10;
						}
						GameObject.Find("Game").transform.GetComponent<GameBoard>().pelletsConsumed++;
						PlaySound();
						if (tile.isPowerUp)
                        {
							GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
							foreach (GameObject go in ghosts)
							{
								go.GetComponent<Ghost>().StartFrightenedMode();
							}
						}
					}
					if (tile.isBonusItem && tile.bonusItemActive)
                    {
						ConsumeBonusItem(tile);

					}
				}
			}
		}
	}
	void ConsumeBonusItem(Tile bonusItem)
    {
		GameBoard.score += bonusItem.pointValue;
		GameBoard.oneUp += bonusItem.pointValue;
		GameObject.Find("Game").transform.GetComponent<GameBoard>().ConsumingBonusItem(bonusItem.gameObject, bonusItem.pointValue);
	}
}
