using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : Singleton<AgentManager>
{
    public Dictionary<int,Agent> idToAgentDict = new Dictionary<int, Agent>();

    public void AddAgentToDict(int id, Agent agent)
    {
        if(idToAgentDict.ContainsKey(id))
        {
            Debug.LogWarning("Agent with id " + id + " already exists in the dictionary");
            return;
        }
        else
        {
            idToAgentDict.Add(id, agent);
            Debug.Log("Agent with id " + id + " added to the dictionary");
        } 
    }

}
