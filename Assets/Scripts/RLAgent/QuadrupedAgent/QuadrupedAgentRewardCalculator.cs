using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedAgentRewardCalculator : AgentRewardCalculatorBase
{
    private Agent agent;
    private QuadrupedAgentController agentController;
    private QuadrupedAgentObserver agentObserver;
    public override List<float> CalculateReward()
    {

        //1. Touching Ground Reward
        if (agentController.BodyTouchingGround())
        {
            SetStepReward(-1f);
            agent.Reset();
        }
        else
        {
            AddToStepReward(0.1f);
        }
        //2. Panelize energy cost
        float total_velocity=0;
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RH_HIP().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RH_THIGH().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RH_SHANK().jointVelocity[0]);

        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RF_HIP().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RF_THIGH().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_RF_SHANK().jointVelocity[0]);

        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LH_HIP().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LH_THIGH().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LH_SHANK().jointVelocity[0]);

        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LF_HIP().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LF_THIGH().jointVelocity[0]);
        total_velocity += Mathf.Abs(agentObserver.GetArticulationBody_LF_SHANK().jointVelocity[0]);
        float velocity_reward = (float)(-0.01 * total_velocity);
        // Debug.Log($"[INFO][velocity_reward]{velocity_reward}");
        AddToStepReward(velocity_reward);

        episode_reward += step_reward;
        return new List<float>{step_reward};

    }

    public override void OnAgentStart()
    {

    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("[INFO][QuadrupedAgentRewardCalculator]OnEpisodeBegin");
    }

    public override void Reset()
    {
        this.ResetEpisodeReward();
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<Agent>();
        agentController = gameObject.GetComponent<QuadrupedAgentController>();
        agentObserver = gameObject.GetComponent<QuadrupedAgentObserver>();
        episode_reward = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

