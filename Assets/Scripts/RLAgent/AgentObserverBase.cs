using System.Collections.Generic;
using UnityEngine;

public abstract class AgentObserverBase : MonoBehaviour
{
    public abstract List<float> GetObservations();
    protected List<float> observation_list;
    public int Observation_dim;
    public abstract void Reset();
    public abstract void OnEpisodeBegin();
    public abstract void OnAgentStart();
    //Utilities
    public void CheckObservationListDim()
    {
        if (observation_list.Count != Observation_dim)
        {
            Debug.LogError("[ERROR][AgentObserver][CheckObservationListDim]observation_list.Count=" + observation_list.Count + " is not equal to Observation_dim=" + Observation_dim);
        }
    }
    public void ClearObservationList()
    {
        observation_list = new List<float>();
    }
    void AddFloatObs(float obs, string name)
    {
        if (float.IsNaN(obs))
        {
            Debug.LogError("[ERROR][AgentObserver][AddFloatObs]" + name + " is NaN !!!");
        }
        if (float.IsInfinity(obs))
        {
            Debug.LogError("[ERROR][AgentObserver][AddFloatObs]" + name + " is Infinity !!!");
        }

        observation_list.Add(obs);
        //Print Info
        // Debug.Log("[INFO][AgentObserver][AddFloatObs]"+ name + " is " + obs);
    }

    // Compatibility methods with Agent observation. These should be removed eventually.

    /// <summary>
    /// Adds a float observation to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(float observation, string name)
    {
        AddFloatObs(observation, name);
    }

    /// <summary>
    /// Adds an integer observation to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(int observation, string name)
    {
        AddFloatObs(observation, name);
    }

    /// <summary>
    /// Adds an Vector3 observation to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(Vector3 observation, string name)
    {
        AddFloatObs(observation.x, name + "[x]");
        AddFloatObs(observation.y, name + "[y]");
        AddFloatObs(observation.z, name + "[z]");
    }

    /// <summary>
    /// Adds an Vector2 observation to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(Vector2 observation, string name)
    {
        AddFloatObs(observation.x, name + "[x]");
        AddFloatObs(observation.y, name + "[y]");
    }

    /// <summary>
    /// Adds a list or array of float observations to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(IList<float> observation, string name)
    {
        for (var i = 0; i < observation.Count; i++)
        {
            AddFloatObs(observation[i], name +"["+i+"]");
        }
    }

    /// <summary>
    /// Adds a quaternion observation to the vector observations of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(Quaternion observation, string name)
    {
        AddFloatObs(observation.x, name + "[x]");
        AddFloatObs(observation.y, name + "[y]");
        AddFloatObs(observation.z, name + "[z]");
        AddFloatObs(observation.w, name + "[w]");
    }

    /// <summary>
    /// Adds a boolean observation to the vector observation of the agent.
    /// </summary>
    /// <param name="observation">Observation.</param>
    public void AddToObservationList(bool observation, string name)
    {
        AddFloatObs(observation ? 1f : 0f, name);
    }
}
