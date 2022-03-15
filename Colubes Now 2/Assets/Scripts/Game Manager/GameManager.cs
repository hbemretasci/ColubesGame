using UnityEngine;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    public GameBaseState[] gameStates;
    public GameBaseState topState
    {
        get
        {
            if (m_State_List.Count == 0) return null;
            return m_State_List[m_State_List.Count - 1];
        }
    }

    protected List<GameBaseState> m_State_List = new List<GameBaseState>();
    protected Dictionary<string, GameBaseState> m_State_Dictionary = new Dictionary<string, GameBaseState>();

    protected void OnEnable()
    {
        GameDataPrefs.CreateData();

        m_State_Dictionary.Clear();

        if (gameStates.Length == 0) return;

        for (int i = 0; i < gameStates.Length; i++)
        {
            gameStates[i].gameManager = this;
            m_State_Dictionary.Add(gameStates[i].GetName(), gameStates[i]);
        }

        m_State_List.Clear();

        PushState(gameStates[0].GetName());
    }

    protected void Update()
    {
        if (m_State_List.Count > 0) m_State_List[m_State_List.Count - 1].Tick();
    }

    public void SwitchState(string newStateName)
    {
        GameBaseState gameState = FindState(newStateName);
        if (gameState == null)
        {
            Debug.LogError("[GameManager] Can't find the state named " + newStateName);
            return;
        }

        m_State_List[m_State_List.Count - 1].Exit(gameState);
        gameState.Enter(m_State_List[m_State_List.Count - 1]);
        m_State_List.RemoveAt(m_State_List.Count - 1);
        m_State_List.Add(gameState);
    }

    public void PushState(string stateName)
    {
        GameBaseState gameState;

        if (!m_State_Dictionary.TryGetValue(stateName, out gameState))
        {
            Debug.LogError("[GameManager] Can't find the state named " + stateName);
            return;
        }

        if (m_State_List.Count > 0)
        {
            m_State_List[m_State_List.Count - 1].Exit(gameState);
            gameState.Enter(m_State_List[m_State_List.Count - 1]);
        }
        else
        {
            gameState.Enter(null);
        }
        m_State_List.Add(gameState);
    }

    public void PopState()
    {
        if (m_State_List.Count < 2)
        {
            Debug.LogError("[GameManager] Can't pop game states, only one in List.");
            return;
        }

        m_State_List[m_State_List.Count - 1].Exit(m_State_List[m_State_List.Count - 2]);

        //m_State_List[m_State_List.Count - 2].Enter(m_State_List[m_State_List.Count - 2]);
        //yukarýdaki satýr aþaðýdaki þekilde deðiþtirildi.
        m_State_List[m_State_List.Count - 2].Enter(m_State_List[m_State_List.Count - 1]);

        m_State_List.RemoveAt(m_State_List.Count - 1);
    }

    public GameBaseState FindState(string stateName)
    {
        GameBaseState gameState;

        if (!m_State_Dictionary.TryGetValue(stateName, out gameState)) return null;

        return gameState;
    }  
}
