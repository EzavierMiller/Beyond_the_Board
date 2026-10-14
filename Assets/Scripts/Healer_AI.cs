using UnityEngine;

public class Healer_AI : BoardPiece
{
    public int healAmount = 2;
    public int range = 2;
    public int maxHealth = 10;
    public BoardPiece[] allAllies;

    void Update()
    {
        if (control == PieceControl.AI)
        {
            reflexAction();
        }
    }

    private void reflexAction()
    {
        BoardPiece weakest = null;
        int lowest = int.MaxValue;

        foreach (BoardPiece piece in allAllies)
        {
            if (piece == null)
            {
                continue;
            }

            if (piece.control == this.control)
            {
                if (piece.health < lowest)
                {
                    lowest = piece.health;
                    weakest = piece;
                }
            }
        }

        if (weakest != null)
        {
            int distance = Mathf.Abs(weakest.GetX() - xLoc) + Mathf.Abs(weakest.GetY() - yLoc);

            if (distance <= 1)
            {
                weakest.health += healAmount;
            }

            else
            {
                int targetX = xLoc;
                int targetY = yLoc;

                if (weakest.GetX() < xLoc)
                {
                    xLoc--;
                }
                if (weakest.GetX() > xLoc)
                {
                    xLoc++;
                }

                if (weakest.GetY() < yLoc)
                {
                    yLoc--;
                }
                if (weakest.GetY() > yLoc)
                {
                    yLoc++;
                }

                Move(targetX, targetY);
            }
        }
    }
}
