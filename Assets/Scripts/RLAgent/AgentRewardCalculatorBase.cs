using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentRewardCalculatorBase : MonoBehaviour
{
    public abstract List<float> CalculateReward();
    protected float step_reward ;

    protected float episode_reward;

    public abstract void Reset();
    public abstract void OnEpisodeBegin();
    public abstract void OnAgentStart();
    public void SetStepReward(float reward)
    {
        this.step_reward = reward;
    }

    public void AddToStepReward(float reward)
    {
        this.step_reward += reward;
    }



    public void ResetEpisodeReward()
    {
        episode_reward = 0;
    }
    public float GetEpisodeReward()
    {
        return episode_reward;
    }

}
