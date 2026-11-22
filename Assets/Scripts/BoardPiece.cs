using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
//base class that each piece can inherit from
public class BoardPiece : MonoBehaviour
{
    //What I added//
    public static List<BoardPiece> AllPieces = new List<BoardPiece>();

    protected virtual void OnEnable()
    {
    if (!AllPieces.Contains(this))

    {
        AllPieces.Add(this);
    }
    }

    protected virtual void OnDisable()
    {
        AllPieces.Remove(this);
    }
    //

    public BoardPos GetPos()
    {
        return (GetX()-1, GetY()-1, control);
    }



    [Header("Piece Stats")]
    public int health = 10;
    public int dmg = 1;

    [Header("Piece Settings")]
    public Team control = Team.None;
    public BoardPiece targetPiece = null;
    public int xLoc = 0;
    public int yLoc = 0;

    private RectTransform rect;
    private int unitSize = 25;
    [SerializeField] private string pieceId;
    public string PieceId => string.IsNullOrEmpty(pieceId) ? GetEntityId().ToString() : pieceId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rect != null)
            rect.anchoredPosition = new Vector2((xLoc - 1) * unitSize, -((yLoc - 1) * unitSize));
        if (Manager == null) Logic();
        xLoc = Mathf.Clamp(xLoc, 1, 8);
        yLoc = Mathf.Clamp(yLoc, 1, 8);
    }

    public GameManager Manager => GameManager.instance != null &&
        GameManager.instance.gameObject.scene == gameObject.scene &&
        GameManager.instance.pieces.Contains(this) ? GameManager.instance : null;

    public bool CanMove(int x, int y)
    {
        if (x < 1 || x > 8 || y < 1 || y > 8) return false;
        if (Manager != null) return Manager.GetBoard().IsLegal(new BoardPos(x - 1, y - 1, Team.None));
        foreach (BoardPiece piece in AllPieces)
            if (piece != null && piece != this && piece.gameObject.activeInHierarchy && piece.health > 0 &&
                piece.xLoc == x && piece.yLoc == y) return false;
        return true;
    }
    public virtual void Logic() { }
    public virtual void Attack(BoardPiece enemy)
    {
        if (enemy == null || enemy.health <= 0 || enemy.control == control) return;
        if (Manager != null) { Manager.TryDamage(this, enemy, dmg); return; }
        enemy.health = Mathf.Max(0, enemy.health - dmg);
        if (enemy.health == 0)
        {
            PieceMoraleAgent.PublishCapture(new CaptureEvent(0, enemy, control));
            enemy.gameObject.SetActive(false);
            Destroy(enemy.gameObject);
        }
    }

    public void Move(int x, int y)
    {
        if (Manager != null) { Manager.TryMove(this, x, y); return; }
        if (!CanMove(x, y)) return;
        xLoc = x;
        yLoc = y;
    }
    public int GetX() { return xLoc; }
    public int GetY() { return yLoc; }
    public virtual void Advance()
    {
        if (Manager != null && !Manager.IsDispatching) { Manager.IssueOrder(this); return; }
        if (targetPiece == null) return;
        int nextX = xLoc + System.Math.Sign(targetPiece.GetX() - xLoc);
        int nextY = yLoc + System.Math.Sign(targetPiece.GetY() - yLoc);
        Move(nextX, nextY);
    }
}
