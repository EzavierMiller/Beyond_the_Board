using UnityEngine;


public class TankAI : BoardPiece
{
   //Tank Ai 
    public BoardPiece protectedPiece;


    //Assigning the tank piece's stats
    private void Awake()
    {
        health = 30;
        dmg = 1;
    }


    // Called when it is the AI's turn
    public void TakeTurn()
    {
        // Find the weakest ally nearby
        BoardPiece weakestAlly = FindWeakestAlly();

        if (weakestAlly == null)
        {
            protectedPiece = null;
            return;
        }

        protectedPiece = weakestAlly;

        // Find an enemy threatening the ally
        BoardPiece nearestEnemy = FindNearestEnemy(weakestAlly);

        if (nearestEnemy == null)
            return;

        // Find the square between the idenified weak ally
        int blockX = weakestAlly.GetX()
            + System.Math.Sign(nearestEnemy.GetX() - weakestAlly.GetX());

        int blockY = weakestAlly.GetY()
            + System.Math.Sign(nearestEnemy.GetY() - weakestAlly.GetY());

        // Only move if the square in front of the piece is empty
        if (IsSquareEmpty(blockX, blockY))
        {
            // Move one square toward the defensive position
            int nextX = xLoc + System.Math.Sign(blockX - xLoc);
            int nextY = yLoc + System.Math.Sign(blockY - yLoc);

            if (IsSquareEmpty(nextX, nextY))
            {
                Move(nextX, nextY);
            }
        }
    }

    BoardPiece FindWeakestAlly()
    {
        BoardPiece weakest = null;

        foreach (BoardPiece piece in AllPieces)
        {
            if (piece == null || piece == this)
                continue;

            // Makes sure it is an allied
            if (piece.control != control)
                continue;

            // Search a 3x3 area around itself
            int distanceX = Mathf.Abs(piece.GetX() - xLoc);
            int distanceY = Mathf.Abs(piece.GetY() - yLoc);

            if (distanceX > 1 || distanceY > 1)
                continue;

            if (weakest == null || piece.health < weakest.health)
            {
                weakest = piece;
            }
        }

        return weakest;
    }

    BoardPiece FindNearestEnemy(BoardPiece ally)
    {
        BoardPiece nearest = null;
        int shortestDistance = int.MaxValue;

        foreach (BoardPiece piece in AllPieces)
        {
            if (piece == null || piece == this)
                continue;

            if (piece.control == control)
                continue;

            int distance =
                Mathf.Abs(piece.GetX() - ally.GetX()) +
                Mathf.Abs(piece.GetY() - ally.GetY());

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = piece;
            }
        }

        return nearest;
    }

    bool IsSquareEmpty(int x, int y)
    {
        // Makes sure it doesn't go out of bounds
        if (x < 1 || x > 8 || y < 1 || y > 8)
            return false;

        foreach (BoardPiece piece in AllPieces)
        {
            if (piece == null || piece == this)
                continue;

            if (piece.GetX() == x && piece.GetY() == y)
                return false;
        }

        return true;
    }

   

    
    bool IsAllyBehindTank(BoardPiece ally, BoardPiece attacker)
    {
        //Melee Attack
    if (ally == null || attacker == null)
        return false;

    // The attacker, tank, and ally must be in a straight line.
    int dx1 = xLoc - attacker.GetX();
    int dy1 = yLoc - attacker.GetY();

    int dx2 = ally.GetX() - xLoc;
    int dy2 = ally.GetY() - yLoc;

    // Require the ally to be behind the tank, relative to the attacker.
    if (dx1 * dy2 != dy1 * dx2)
        return false;

    // Both segments must point in the same direction
    return dx1 * dx2 + dy1 * dy2 > 0;
    }


}
