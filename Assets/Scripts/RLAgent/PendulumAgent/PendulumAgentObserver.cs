using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumAgentObserver : AgentObserverBase
{
    public Transform joint_prismatic;
    public Transform joint_revolute;

    private ArticulationBody articulationBody_joint_prismatic;
    private ArticulationBody articulationBody_joint_revolute;

    void Start()
    {
        articulationBody_joint_prismatic = joint_prismatic.GetComponent<ArticulationBody>();
        articulationBody_joint_revolute = joint_revolute.GetComponent<ArticulationBody>();
    }
    void Update()
    {

    }

    public override List<float> GetObservations()
    {
        ClearObservationList();
        AddToObservationList(articulationBody_joint_prismatic.jointPosition[0],name:"[articulationBody_joint_prismatic.jointPosition[0]]");
        AddToObservationList(articulationBody_joint_prismatic.jointVelocity[0],name:"[articulationBody_joint_prismatic.jointVelocity[0]]");
        AddToObservationList(articulationBody_joint_revolute.jointPosition[0],name:"[articulationBody_joint_revolute.jointPosition[0]]");
        AddToObservationList(articulationBody_joint_revolute.jointVelocity[0],name:"[articulationBody_joint_revolute.jointVelocity[0]]");
        return observation_list;
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
