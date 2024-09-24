using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class PendulumAgentController : AgentControllerBase
// {
    
//     public Transform joint_prismatic;
//     public float joint_prismatic_stiffness_ub=200;
//     public float joint_prismatic_stiffness_lb=30;
//     public float joint_prismatic_target_ub=10;
//     public float joint_prismatic_target_lb=-10;
//     public int action_dim=2;


//     public Transform joint_revolute;

//     private ArticulationBody articulationBody_joint_prismatic;
//     private ArticulationBody articulationBody_joint_revolute;



//     void Start()
//     {
//         articulationBody_joint_prismatic = joint_prismatic.GetComponent<ArticulationBody>();
//         articulationBody_joint_revolute = joint_revolute.GetComponent<ArticulationBody>();
//         initializeAction(action_dim:action_dim);
//     }


//     // Update is called once per frame
//     void Update()
//     {
//     }



//     public override void SetAction(List<float> _action_list)
//     {
//         //Each action in action list range from -1 to 1, need to convert to right scale
//         float joint_prismatic_stiffness = MapValue(_action_list[0],fromLow:-1,fromHigh:1,toLow:joint_prismatic_stiffness_lb,toHigh:joint_prismatic_stiffness_ub);
//         float joint_prismatic_target = MapValue(_action_list[1],fromLow:-1,fromHigh:1,toLow:joint_prismatic_target_lb,toHigh:joint_prismatic_target_ub);
//         //Action Space is 2
//         articulationBody_joint_prismatic.SetDriveStiffness(ArticulationDriveAxis.X,joint_prismatic_stiffness);
//         articulationBody_joint_prismatic.SetDriveTarget(ArticulationDriveAxis.X,joint_prismatic_target);

//         //Store the action list
//         action_list =_action_list;
//     }
    

//     public override void Reset()
//     {
//         Debug.Log("[INFO][PendulumAgentController][Reset]Reset!!" );
//         articulationBody_joint_prismatic.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
//         articulationBody_joint_prismatic.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
//         articulationBody_joint_prismatic.SetDriveTarget(ArticulationDriveAxis.X,0f);
//         articulationBody_joint_prismatic.SetDriveStiffness(ArticulationDriveAxis.X,0f);

//         articulationBody_joint_revolute.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
//         articulationBody_joint_revolute.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
//         articulationBody_joint_revolute.SetDriveTarget(ArticulationDriveAxis.X,0f);
//         articulationBody_joint_revolute.SetDriveStiffness(ArticulationDriveAxis.X,0f);
//     }


// }
