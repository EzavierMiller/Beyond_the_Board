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
        allAllies = Manager != null ? Manager.pieces : AllPieces;
    }

    public override void Advance()
    {
        if (Manager != null && Manager.ActingPiece != this) { Manager.IssueOrder(this); return; }
        Logic();
        BoardPiece weakest = null;
        int lowest = int.MaxValue;

        foreach (BoardPiece piece in allAllies)
        {
            if (piece == null || piece.health <= 0 || !piece.gameObject.activeInHierarchy)
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
                if (Manager != null) Manager.TryHeal(this, weakest, healAmount, maxHealth);
                else weakest.health = Mathf.Min(maxHealth, weakest.health + healAmount);
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
