using UnityEngine;

public class Warrior : BoardPiece
{
    public int attackRange = 1;
    
    private BoardPiece currentTarget = null;

    public override void Logic()
    {

    }
    public override void Advance()
    {
        if (control == PieceControl.AI)
        {
            Action();
        }
    }

    private void Action()
    {
        currentTarget = findNearest();

        if (currentTarget != null)
        {
            int distance = Mathf.Abs(xLoc - currentTarget.GetX()) + Mathf.Abs(yLoc - currentTarget.GetY());

            if (distance <= attackRange)
            {
                Attack(currentTarget);
            }

            else
            {
                moveToward(currentTarget);
            }
        }
    }

    private BoardPiece findNearest()
    {
        BoardPiece[] all = FindObjectsByType<BoardPiece>(FindObjectsSortMode.None);
        BoardPiece nearestEnemy = null;
        float shortest = int.MaxValue;

        foreach (BoardPiece piece in all)
        {
            if (piece == this || piece.control == this.control || piece.control == PieceControl.None)
                continue;

            float dist = Vector2.Distance(new Vector2(xLoc, yLoc), new Vector2(piece.GetX(), piece.GetY()));
            if (dist < shortest)
            {
                shortest= dist;
                nearestEnemy = piece;
            }
        }
        return nearestEnemy;
    }

    private void moveToward(BoardPiece target)
    {
        int targetX = xLoc;
        int targetY = yLoc;

        if (Mathf.Abs(target.GetX() - xLoc) > Mathf.Abs(target.GetY() - yLoc))
        {
            targetX += (target.GetX() > xLoc) ? 1 : -1;
        }
        else
        {
            targetY += (target.GetY() > yLoc) ? 1 : -1;
        }

        if(CanMove(targetX,targetY)) 
            Move(targetX, targetY);
    }

}
