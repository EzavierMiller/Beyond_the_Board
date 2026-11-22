using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPiece : BoardPiece
{
    private UnityEngine.InputSystem.InputAction moveAction;
    private UnityEngine.InputSystem.InputAction endTurnAction;

    private void Awake()
    {
        moveAction = new UnityEngine.InputSystem.InputAction("Board Move", UnityEngine.InputSystem.InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        endTurnAction = new UnityEngine.InputSystem.InputAction("End Turn", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/e");
        moveAction.performed += OnMove;
        endTurnAction.performed += OnEndTurn;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        moveAction?.Enable();
        endTurnAction?.Enable();
    }

    protected override void OnDisable()
    {
        moveAction?.Disable();
        endTurnAction?.Disable();
        base.OnDisable();
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        endTurnAction?.Dispose();
    }

    private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (control != Team.Player || health <= 0) return;
        Vector2 direction = context.ReadValue<Vector2>();
        if (direction.sqrMagnitude < 0.1f) return;
        int x = xLoc + System.Math.Sign(direction.x);
        int y = yLoc - System.Math.Sign(direction.y);
        if (Manager != null) Manager.IssueMove(this, x, y);
        else Move(x, y);
    }

    private void OnEndTurn(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (control == Team.Player && Manager != null && Manager.player == this) Manager.EndPlayerTurn();
    }

    public override void Logic() { }
}
