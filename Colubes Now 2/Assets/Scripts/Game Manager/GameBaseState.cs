using UnityEngine;

public abstract class GameBaseState : MonoBehaviour
{
    [HideInInspector]
    public GameManager gameManager;

    public abstract void Enter(GameBaseState from);
    public abstract void Exit(GameBaseState to);
    public abstract void Tick();
    public abstract string GetName();
}
