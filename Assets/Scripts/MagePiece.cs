using UnityEngine;

public class MagePiece : BoardPiece
{
    public int range = 3;
    public float distance = 0;
    private MageAgent mageAgent;

    private void Awake() => mageAgent = GetComponent<MageAgent>();
    public override void Logic() { }

    public override void Advance()
    {
        if (Manager != null && Manager.ActingPiece != this) { Manager.IssueOrder(this); return; }
        if (mageAgent != null) mageAgent.TakeTurn();
    }
}
