using System.Collections.Generic;
using UnityEngine;

public class Healer_AI : BoardPiece
{
    public int healAmount = 2;
    public int range = 2;
    public int maxHealth = 10;
    public List<BoardPiece> allAllies;

    public override void Logic()
    {
        allAllies = GameManager.instance.pieces;
        if (control == PieceControl.AI)
        {
            //reflexAction();
        }
    }

    public override void Advance()
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
                    targetX--;
                }
                if (weakest.GetX() > xLoc)
                {
                    targetX++;
                }

                if (weakest.GetY() < yLoc)
                {
                    targetY--;
                }
                if (weakest.GetY() > yLoc)
                {
                    targetY++;
                }

                if(CanMove(targetX, targetY)) 
                    Move(targetX, targetY);
            }
        }
    }
}
