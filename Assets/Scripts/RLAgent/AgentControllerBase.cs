using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentControllerBase : MonoBehaviour
{


    protected List<float> action_list;
    public abstract void ExecuteAction();
    public abstract void Reset();
    public abstract void OnEpisodeBegin();
    public abstract void OnAgentStart();

    public void SetAction(List<float> _action_list)
    {
        action_list =_action_list;

    }

    public List<float> GetAction()
    {
        return action_list;
    }
    protected void initializeAction(int action_dim)
    {
        // Initialize actionList with zeros
        List<float> floatsList = new List<float>(action_dim);
        for (int i = 0; i < action_dim; i++)
        {
            floatsList.Add(0.0f);
        }
        SetAction(floatsList);
    }

    public static float MapValue(float value, float fromLow, float fromHigh, float toLow, float toHigh)
    {
        // Normalize the value to the range [0, 1]
        float normalizedValue = (value - fromLow) / (fromHigh - fromLow);
        
        // Map the normalized value to the target range
        float mappedValue = toLow + normalizedValue * (toHigh - toLow);
        
        return mappedValue;
    }

}
