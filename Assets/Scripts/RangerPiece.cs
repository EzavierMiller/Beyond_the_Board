using UnityEngine;

public class RangerPiece : BoardPiece
{
    public int range = 3;
    public float distance = 0;

    public override void Logic()
    {
        if (targetPiece == null || targetPiece.health <= 0 || !targetPiece.gameObject.activeInHierarchy || targetPiece.control == control)
        {
            targetPiece = null;
            int nearest = int.MaxValue;
            foreach (BoardPiece candidate in Manager != null ? Manager.pieces : AllPieces)
            {
                if (candidate == null || candidate == this || candidate.health <= 0 || !candidate.gameObject.activeInHierarchy ||
                    candidate.control == control || candidate.control == Team.None) continue;
                int d = Mathf.Abs(candidate.GetX() - xLoc) + Mathf.Abs(candidate.GetY() - yLoc);
                if (d < nearest) { nearest = d; targetPiece = candidate; }
            }
        }
        distance = targetPiece == null ? float.PositiveInfinity : Vector3.Distance(transform.position, targetPiece.transform.position);
    }

    public override void Advance()
    {
        if (Manager != null && Manager.ActingPiece != this) { Manager.IssueOrder(this); return; }
        Logic();
        if (targetPiece == null) return;
        if (distance <= range * 75 && distance > 75) { Attack(targetPiece); return; }
        if (distance > 75) return;
        int x = xLoc, y = yLoc;
        if (targetPiece.GetX() == xLoc && (yLoc == 1 || yLoc == 8)) x += xLoc < 4 ? 1 : -1;
        else if (targetPiece.GetX() > xLoc) x--;
        else if (targetPiece.GetX() < xLoc) x++;
        else if (targetPiece.GetY() > yLoc) y--;
        else if (targetPiece.GetY() < yLoc) y++;
        Move(Mathf.Clamp(x, 1, 8), Mathf.Clamp(y, 1, 8));
    }
}
