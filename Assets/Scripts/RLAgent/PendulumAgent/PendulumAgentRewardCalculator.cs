using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumAgentRewardCalculator : AgentRewardCalculatorBase
{
    public Transform joint_revolute;
    private ArticulationBody articulationBody_joint_revolute;

    public float revolute_joint_reference = 0.0f;


    // Start is called before the first frame update
    void Start()
    {
        articulationBody_joint_revolute = joint_revolute.GetComponent<ArticulationBody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override List<float> CalculateReward()
    {
        float sr_error_revoluteJoint = Mathf.Sqrt(Mathf.Pow(articulationBody_joint_revolute.jointPosition[0] - revolute_joint_reference,2));
        float reward_revoluteJoint = 1/(1 + sr_error_revoluteJoint);
        return new List<float>{reward_revoluteJoint};
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override void OnEpisodeBegin()
    {
        throw new System.NotImplementedException();
    }

    public override void OnAgentStart()
    {
        throw new System.NotImplementedException();
    }
}
