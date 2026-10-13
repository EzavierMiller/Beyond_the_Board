using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

//base class that each piece can inherit from
public abstract class BoardPiece : MonoBehaviour
{
    public enum PieceControl
    {
        None,
        Player,
        AI
    }

    [Header("Piece Stats")]
    public int health = 10;
    public int dmg = 1;

    [Header("Piece Settings")]
    public PieceControl control = PieceControl.None;
    public BoardPiece targetPiece = null;
    public int xLoc = 0;
    public int yLoc = 0;

    private RectTransform rect;
    private int unitSize = 25;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        //update position
        if(rect)
            rect.anchoredPosition = new Vector2((xLoc-1) * unitSize, -1*((yLoc-1) * unitSize));


        Logic();

        //always keep pieces within board
        xLoc = Mathf.Clamp(xLoc, 1, 8);
        yLoc = Mathf.Clamp(yLoc, 1, 8);


    }
    public abstract void Logic();
    public virtual void Attack(BoardPiece target)
    {
        target.health -= dmg;
    }
    public void Move(int x, int y)
    {
        //yeah
        xLoc = x;
        yLoc = y;
    }
    public int GetX() { return xLoc; }
    public int GetY() { return yLoc; }
    public virtual void Advance()
    {
        //moves this piece in any direction
        int tempX = xLoc;
        int tempY = yLoc;

        if (targetPiece != null) {
            if (targetPiece.GetY() > yLoc)
            {
                tempY = Mathf.Clamp(++yLoc, 1, 8);
            }

            if (targetPiece.GetY() < yLoc)
            {
                tempY = Mathf.Clamp(--yLoc, 1, 8);
            }

            if (targetPiece.GetX() > xLoc)
            {
                tempX = Mathf.Clamp(++xLoc, 1, 8);
            }

            if (targetPiece.GetX() < xLoc)
            {
                tempX = Mathf.Clamp(--xLoc, 1, 8);
            }

            Move(tempX, tempY);
        }

    }
}
