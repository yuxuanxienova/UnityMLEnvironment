using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedAgentObserver : AgentObserverBase
{

    // private Vector3 agent_init_position;
    private Transform base_inertia;
    private Transform RH_HIP;
    private Transform RH_THIGH;
    private Transform RH_SHANK;
    private Transform RH_FOOT;
    private Transform LH_HIP;
    private Transform LH_THIGH;
    private Transform LH_SHANK;
    private Transform LH_FOOT;
    private Transform RF_HIP;
    private Transform RF_THIGH;
    private Transform RF_SHANK;
    private Transform RF_FOOT;
    private Transform LF_HIP;
    private Transform LF_THIGH;
    private Transform LF_SHANK;
    private Transform LF_FOOT;

    private GroundContactCustom groundContact_RH_THIGH;
    private GroundContactCustom groundContact_RH_SHANK1;
    private GroundContactCustom groundContact_RH_SHANK2;
    private GroundContactCustom groundContact_RH_FOOT;

    private GroundContactCustom groundContact_LH_THIGH;
    private GroundContactCustom groundContact_LH_SHANK1;
    private GroundContactCustom groundContact_LH_SHANK2;
    private GroundContactCustom groundContact_LH_FOOT;

    private GroundContactCustom groundContact_RF_THIGH;
    private GroundContactCustom groundContact_RF_SHANK1;
    private GroundContactCustom groundContact_RF_SHANK2;
    private GroundContactCustom groundContact_RF_FOOT;

    private GroundContactCustom groundContact_LF_THIGH;
    private GroundContactCustom groundContact_LF_SHANK1;
    private GroundContactCustom groundContact_LF_SHANK2;
    private GroundContactCustom groundContact_LF_FOOT;

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    // public OrientationCubeController m_OrientationCube;

    //The indicator graphic gameobject that points towards the target
    // public DirectionIndicator m_DirectionIndicator;

    private ArticulationBody articulationBody_BaseInertia;

    private ArticulationBody articulationBody_RH_HIP;
    private ArticulationBody articulationBody_RH_THIGH;
    private ArticulationBody articulationBody_RH_SHANK;

    private ArticulationBody articulationBody_LH_HIP;
    private ArticulationBody articulationBody_LH_THIGH;
    private ArticulationBody articulationBody_LH_SHANK;

    private ArticulationBody articulationBody_RF_HIP;
    private ArticulationBody articulationBody_RF_THIGH;
    private ArticulationBody articulationBody_RF_SHANK;

    private ArticulationBody articulationBody_LF_HIP;
    private ArticulationBody articulationBody_LF_THIGH;
    private ArticulationBody articulationBody_LF_SHANK;
    public ArticulationBody GetArticulationBody_RH_HIP()
    {
        return articulationBody_RH_HIP;
    }

    public ArticulationBody GetArticulationBody_RH_THIGH()
    {
        return articulationBody_RH_THIGH;
    }

    public ArticulationBody GetArticulationBody_RH_SHANK()
    {
        return articulationBody_RH_SHANK;
    }

    public ArticulationBody GetArticulationBody_LH_HIP()
    {
        return articulationBody_LH_HIP;
    }

    public ArticulationBody GetArticulationBody_LH_THIGH()
    {
        return articulationBody_LH_THIGH;
    }

    public ArticulationBody GetArticulationBody_LH_SHANK()
    {
        return articulationBody_LH_SHANK;
    }

    public ArticulationBody GetArticulationBody_RF_HIP()
    {
        return articulationBody_RF_HIP;
    }

    public ArticulationBody GetArticulationBody_RF_THIGH()
    {
        return articulationBody_RF_THIGH;
    }

    public ArticulationBody GetArticulationBody_RF_SHANK()
    {
        return articulationBody_RF_SHANK;
    }

    public ArticulationBody GetArticulationBody_LF_HIP()
    {
        return articulationBody_LF_HIP;
    }

    public ArticulationBody GetArticulationBody_LF_THIGH()
    {
        return articulationBody_LF_THIGH;
    }

    public ArticulationBody GetArticulationBody_LF_SHANK()
    {
        return articulationBody_LF_SHANK;
    }
    private QuadrupedAgentController m_QuadrupedAgentController;
    public override List<float> GetObservations()
    {
        observation_list = new List<float>();
        // AddToObservationList(articulationBody_RH_HIP.jointPosition[0],name:"[articulationBody_RH_HIP.jointPosition[0]]"); 
        //--------------------------------------------------------------Observations----------------------------------------------
        //1 Observation Type: Proprioception
        
        //1.1 command [Dim=3]
        var dirToTarget = base_inertia.forward;
        AddToObservationList(dirToTarget,name:"[dirToTarget]");

        //1.2 body orientation [Dim=3]
        AddToObservationList(base_inertia.forward,name:"[base_inertia.forward]");

        //1.3 body velocity(linear + angular) [Dim=6]
        AddToObservationList(articulationBody_BaseInertia.velocity,name:"[articulationBody_BaseInertia.velocity]");
        AddToObservationList(articulationBody_BaseInertia.angularVelocity,name:"[articulationBody_BaseInertia.angularVelocity]");

        //1.4 joint position [Dim=12]
        AddToObservationList(articulationBody_RH_HIP.jointPosition[0],name:"[articulationBody_RH_HIP.jointPosition[0]");
        AddToObservationList(articulationBody_RH_THIGH.jointPosition[0],name:"[articulationBody_RH_THIGH.jointPosition[0]");
        AddToObservationList(articulationBody_RH_SHANK.jointPosition[0],name:"[articulationBody_RH_SHANK.jointPosition[0]");

        AddToObservationList(articulationBody_LH_HIP.jointPosition[0],name:"[articulationBody_LH_HIP.jointPosition[0]");
        AddToObservationList(articulationBody_LH_THIGH.jointPosition[0],name:"[articulationBody_LH_THIGH.jointPosition[0]");
        AddToObservationList(articulationBody_LH_SHANK.jointPosition[0],name:"[articulationBody_LH_SHANK.jointPosition[0]");

        AddToObservationList(articulationBody_RF_HIP.jointPosition[0],name:"[articulationBody_RF_HIP.jointPosition[0]");
        AddToObservationList(articulationBody_RF_THIGH.jointPosition[0],name:"[articulationBody_RF_THIGH.jointPosition[0]");
        AddToObservationList(articulationBody_RF_SHANK.jointPosition[0],name:"[articulationBody_RF_SHANK.jointPosition[0]");

        AddToObservationList(articulationBody_LF_HIP.jointPosition[0],name:"[articulationBody_LF_HIP.jointPosition[0]");
        AddToObservationList(articulationBody_LF_THIGH.jointPosition[0],name:"[articulationBody_LF_THIGH.jointPosition[0]");
        AddToObservationList(articulationBody_LF_SHANK.jointPosition[0],name:"[articulationBody_LF_SHANK.jointPosition[0]");

        //1.5 joint velocity [Dim=12]
        AddToObservationList(articulationBody_RH_HIP.jointVelocity[0],name:"[articulationBody_RH_HIP.jointVelocity[0]");
        AddToObservationList(articulationBody_RH_THIGH.jointVelocity[0],name:"[articulationBody_RH_THIGH.jointVelocity[0]");
        AddToObservationList(articulationBody_RH_SHANK.jointVelocity[0],name:"[articulationBody_RH_SHANK.jointVelocity[0]");

        AddToObservationList(articulationBody_LH_HIP.jointVelocity[0],name:"[articulationBody_LH_HIP.jointVelocity[0]");
        AddToObservationList(articulationBody_LH_THIGH.jointVelocity[0],name:"[articulationBody_LH_THIGH.jointVelocity[0]");
        AddToObservationList(articulationBody_LH_SHANK.jointVelocity[0],name:"[articulationBody_LH_SHANK.jointVelocity[0]");

        AddToObservationList(articulationBody_RF_HIP.jointVelocity[0],name:"[articulationBody_RF_HIP.jointVelocity[0]");
        AddToObservationList(articulationBody_RF_THIGH.jointVelocity[0],name:"[articulationBody_RF_THIGH.jointVelocity[0]");
        AddToObservationList(articulationBody_RF_SHANK.jointVelocity[0],name:"[articulationBody_RF_SHANK.jointVelocity[0]");

        AddToObservationList(articulationBody_LF_HIP.jointVelocity[0],name:"[articulationBody_LF_HIP.jointVelocity[0]");
        AddToObservationList(articulationBody_LF_THIGH.jointVelocity[0],name:"[articulationBody_LF_THIGH.jointVelocity[0]");
        AddToObservationList(articulationBody_LF_SHANK.jointVelocity[0],name:"[articulationBody_LF_SHANK.jointVelocity[0]");

        //1.6 CPG Phase Information[Dim=12]
        
        AddToObservationList(m_QuadrupedAgentController.delta_phi_RH_rad,name:"[delta_phi_RH_rad]");
        AddToObservationList(m_QuadrupedAgentController.delta_phi_RF_rad,name:"[delta_phi_RF_rad]");
        AddToObservationList(m_QuadrupedAgentController.delta_phi_LH_rad,name:"[delta_phi_LH_rad]");
        AddToObservationList(m_QuadrupedAgentController.delta_phi_LF_rad,name:"[delta_phi_LF_rad]");


        AddToObservationList(Mathf.Cos(m_QuadrupedAgentController.phi_RH_rad),name:"[Mathf.Cos(phi_RH_rad)]");
        AddToObservationList(Mathf.Cos(m_QuadrupedAgentController.phi_RF_rad),name:"[Mathf.Cos(phi_RF_rad)]");
        AddToObservationList(Mathf.Cos(m_QuadrupedAgentController.phi_LH_rad),name:"[Mathf.Cos(phi_LH_rad)]");
        AddToObservationList(Mathf.Cos(m_QuadrupedAgentController.phi_LF_rad),name:"[Mathf.Cos(phi_LF_rad)]");

        AddToObservationList(Mathf.Sin(m_QuadrupedAgentController.phi_RH_rad),name:"[Mathf.Sin(phi_RH_rad)]");
        AddToObservationList(Mathf.Sin(m_QuadrupedAgentController.phi_RF_rad),name:"[Mathf.Sin(phi_RF_rad)]");
        AddToObservationList(Mathf.Sin(m_QuadrupedAgentController.phi_LH_rad),name:"[Mathf.Sin(phi_LH_rad)]");
        AddToObservationList(Mathf.Sin(m_QuadrupedAgentController.phi_LF_rad),name:"[Mathf.Sin(phi_LF_rad)]");

        //2 Observation Type: Exteroception

        //2.1 height sample [Dim=1]
        RaycastHit hit;
        float maxRaycastDist = 10;
        if (Physics.Raycast(base_inertia.position, Vector3.down, out hit, maxRaycastDist))
        {
            AddToObservationList(hit.distance / maxRaycastDist,name:"[hit.distance / maxRaycastDist]");
        }
        else
        {
            AddToObservationList(1,name:"[hit.distance / maxRaycastDist]");
        }

        //3 Observation Type: Privileged info

        //3.1 Foot Ground Contact State [Dim=4]
        AddToObservationList(groundContact_RH_FOOT.touchingGround,name:"[groundContact_RH_FOOT.touchingGround]");
        AddToObservationList(groundContact_LH_FOOT.touchingGround,name:"[groundContact_LH_FOOT.touchingGround]");
        AddToObservationList(groundContact_RF_FOOT.touchingGround,name:"[groundContact_RF_FOOT.touchingGround]");
        AddToObservationList(groundContact_LF_FOOT.touchingGround,name:"[groundContact_LF_FOOT.touchingGround]");

        //3.2 Drive Force [Dim=12]
        AddToObservationList(articulationBody_RH_HIP.driveForce[0],name:"[articulationBody_RH_HIP.driveForce[0]");
        AddToObservationList(articulationBody_RH_THIGH.driveForce[0],name:"[articulationBody_RH_THIGH.driveForce[0]");
        AddToObservationList(articulationBody_RH_SHANK.driveForce[0],name:"[articulationBody_RH_SHANK.driveForce[0]");

        AddToObservationList(articulationBody_LH_HIP.driveForce[0],name:"[articulationBody_LH_HIP.driveForce[0]");
        AddToObservationList(articulationBody_LH_THIGH.driveForce[0],name:"[articulationBody_LH_THIGH.driveForce[0]");
        AddToObservationList(articulationBody_LH_SHANK.driveForce[0],name:"[articulationBody_LH_SHANK.driveForce[0]");

        AddToObservationList(articulationBody_RF_HIP.driveForce[0],name:"[articulationBody_RF_HIP.driveForce[0]");
        AddToObservationList(articulationBody_RF_THIGH.driveForce[0],name:"[articulationBody_RF_THIGH.driveForce[0]");
        AddToObservationList(articulationBody_RF_SHANK.driveForce[0],name:"[articulationBody_RF_SHANK.driveForce[0]");

        AddToObservationList(articulationBody_LF_HIP.driveForce[0],name:"[articulationBody_LF_HIP.driveForce[0]");
        AddToObservationList(articulationBody_LF_THIGH.driveForce[0],name:"[articulationBody_LF_THIGH.driveForce[0]");
        AddToObservationList(articulationBody_LF_SHANK.driveForce[0],name:"[articulationBody_LF_SHANK.driveForce[0]");

        //3.3 Thigh and Shank Contact State [Dim=8]
        AddToObservationList(groundContact_RH_THIGH.touchingGround,name:"[groundContact_RH_THIGH.touchingGround]");
        AddToObservationList(groundContact_RH_SHANK1.touchingGround,name:"[groundContact_RH_SHANK1.touchingGround]");

        AddToObservationList(groundContact_LH_THIGH.touchingGround,name:"[groundContact_LH_THIGH.touchingGround]");
        AddToObservationList(groundContact_LH_SHANK1.touchingGround,name:"[groundContact_LH_SHANK1.touchingGround]");

        AddToObservationList(groundContact_RF_THIGH.touchingGround,name:"[groundContact_RF_THIGH.touchingGround]");
        AddToObservationList(groundContact_RF_SHANK1.touchingGround,name:"[groundContact_RF_SHANK1.touchingGround]");

        AddToObservationList(groundContact_LF_THIGH.touchingGround,name:"[groundContact_LF_THIGH.touchingGround]");
        AddToObservationList(groundContact_LF_SHANK1.touchingGround,name:"[groundContact_LF_SHANK1.touchingGround]");

        return observation_list;
    }
    public override void Reset()
    {
        
    }

    public override void OnAgentStart()
    {
        m_QuadrupedAgentController = GetComponent<QuadrupedAgentController>();

    }
    public override void OnEpisodeBegin()
    {
        Debug.Log("[INFO][QuadrupedAgentObserver]OnEpisodeBegin");
        InitializeObserverFields();
    }

    public void InitializeObserverFields()
    {
        //Get all needed Transform
        Transform[] allChildren = this.GetComponentsInChildren<Transform>();

        foreach (Transform child_tf in allChildren)
        {
            // Debug.Log(child_tf.name);
            if (child_tf.name == "base_inertia")
            {
                base_inertia = child_tf;                
            }

            if (child_tf.name == "RH_HIP")
            {
                RH_HIP = child_tf;                
            }         
            if (child_tf.name == "RH_THIGH")
            {
                RH_THIGH = child_tf;                
            }   
            if (child_tf.name == "RH_SHANK")
            {
                RH_SHANK = child_tf;                
            }  
            if (child_tf.name == "RH_FOOT")
            {
                RH_FOOT = child_tf;                
            }  


            if (child_tf.name == "RF_HIP")
            {
                RF_HIP = child_tf;                
            }         
            if (child_tf.name == "RF_THIGH")
            {
                RF_THIGH = child_tf;                
            }   
            if (child_tf.name == "RF_SHANK")
            {
                RF_SHANK = child_tf;                
            }  
            if (child_tf.name == "RF_FOOT")
            {
                RF_FOOT = child_tf;                
            }   


            if (child_tf.name == "LH_HIP")
            {
                LH_HIP = child_tf;                
            }         
            if (child_tf.name == "LH_THIGH")
            {
                LH_THIGH = child_tf;                
            }   
            if (child_tf.name == "LH_SHANK")
            {
                LH_SHANK = child_tf;                
            }  
            if (child_tf.name == "LH_FOOT")
            {
                LH_FOOT = child_tf;                
            }  


            if (child_tf.name == "LF_HIP")
            {
                LF_HIP = child_tf;                
            }         
            if (child_tf.name == "LF_THIGH")
            {
                LF_THIGH = child_tf;                
            }   
            if (child_tf.name == "LF_SHANK")
            {
                LF_SHANK = child_tf;                
            }  
            if (child_tf.name == "LF_FOOT")
            {
                LF_FOOT = child_tf;                
            }  

            if (child_tf.name == "ContactSensor_RH_THIGH")
            {
                groundContact_RH_THIGH = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RH_SHANK1")
            {
                groundContact_RH_SHANK1 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RH_SHANK2")
            {
                groundContact_RH_SHANK2 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RH_FOOT")
            {
                groundContact_RH_FOOT = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }

            if (child_tf.name == "ContactSensor_RF_THIGH")
            {
                groundContact_RF_THIGH = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RF_SHANK1")
            {
                groundContact_RF_SHANK1 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RF_SHANK2")
            {
                groundContact_RF_SHANK2 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_RF_FOOT")
            {
                groundContact_RF_FOOT = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }  

            if (child_tf.name == "ContactSensor_LH_THIGH")
            {
                groundContact_LH_THIGH = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LH_SHANK1")
            {
                groundContact_LH_SHANK1 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LH_SHANK2")
            {
                groundContact_LH_SHANK2 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LH_FOOT")
            {
                groundContact_LH_FOOT = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }  

             if (child_tf.name == "ContactSensor_LF_THIGH")
            {
                groundContact_LF_THIGH = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LF_SHANK1")
            {
                groundContact_LF_SHANK1 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LF_SHANK2")
            {
                groundContact_LF_SHANK2 = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }
            if (child_tf.name == "ContactSensor_LF_FOOT")
            {
                groundContact_LF_FOOT = child_tf.gameObject.GetComponent<GroundContactCustom>();                
            }  
        }


        articulationBody_BaseInertia = base_inertia.GetComponent<ArticulationBody>();

        articulationBody_RH_HIP = RH_HIP.GetComponent<ArticulationBody>();
        articulationBody_RH_THIGH = RH_THIGH.GetComponent<ArticulationBody>();
        articulationBody_RH_SHANK = RH_SHANK.GetComponent<ArticulationBody>();

        articulationBody_LH_HIP = LH_HIP.GetComponent<ArticulationBody>();
        articulationBody_LH_THIGH = LH_THIGH.GetComponent<ArticulationBody>();
        articulationBody_LH_SHANK = LH_SHANK.GetComponent<ArticulationBody>();

        articulationBody_RF_HIP = RF_HIP.GetComponent<ArticulationBody>();
        articulationBody_RF_THIGH = RF_THIGH.GetComponent<ArticulationBody>();
        articulationBody_RF_SHANK = RF_SHANK.GetComponent<ArticulationBody>();    

        articulationBody_LF_HIP = LF_HIP.GetComponent<ArticulationBody>();
        articulationBody_LF_THIGH = LF_THIGH.GetComponent<ArticulationBody>();
        articulationBody_LF_SHANK = LF_SHANK.GetComponent<ArticulationBody>();
    }

}
