using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedAgentController : AgentControllerBase
{
    public GameObject basePrefab;
    public QuadrupedAgentObserver agentObserver;
    private Vector3 agent_init_position;
    private Transform base_inertia;
    private Transform root;
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

    private ArticulationBody articulationBody_Root;
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


    //CPG
    public float delta_phi_0_rad = Mathf.PI/10;
    [HideInInspector]
    public float phi_RH_rad = 0;
    [HideInInspector]
    public float phi_LH_rad = 0;
    [HideInInspector]
    public float phi_RF_rad = 0;
    [HideInInspector]
    public float phi_LF_rad = 0;

    [HideInInspector]
    public float delta_phi_RH_rad = 0;
    [HideInInspector]
    public float delta_phi_LH_rad = 0;
    [HideInInspector]
    public float delta_phi_RF_rad = 0;
    [HideInInspector]
    public float delta_phi_LF_rad = 0;

    public IK_RH iK_RH;
    public IK_RF iK_RF;
    public IK_LH iK_LH;
    public IK_LF iK_LF;

    private Vector3 pos_B_E_RH_0 ;
    private Vector3 pos_B_E_RF_0 ;
    private Vector3 pos_B_E_LH_0 ;
    private Vector3 pos_B_E_LF_0 ;
    public override void ExecuteAction()
    {
        //1. Phase offset per leg [Dim=4]
        delta_phi_RH_rad = Mathf.PI * Mathf.Clamp(action_list[0], -1f, 1f);
        delta_phi_LH_rad = Mathf.PI * Mathf.Clamp(action_list[1], -1f, 1f);
        delta_phi_RF_rad = Mathf.PI * Mathf.Clamp(action_list[2], -1f, 1f);
        delta_phi_LF_rad = Mathf.PI * Mathf.Clamp(action_list[3], -1f, 1f);

        //2. Residual Joint Position Target(in radius) [Dim=12]
        float delat_q_RH_HIP_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_RH_THIGH_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_RH_SHANK_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);

        float delat_q_LH_HIP_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_LH_THIGH_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_LH_SHANK_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);

        float delat_q_RF_HIP_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_RF_THIGH_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_RF_SHANK_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);

        float delat_q_LF_HIP_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_LF_THIGH_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);
        float delat_q_LF_SHANK_rad = 1 * Mathf.Clamp(action_list[0], -1f, 1f);

        //------------------------------CPG---------------------------------------

        //1. Update phi
        phi_RH_rad = ClampAngleRad0To2pi(phi_RH_rad + delta_phi_RH_rad + delta_phi_0_rad); 
        phi_RF_rad = ClampAngleRad0To2pi(phi_RF_rad + delta_phi_RF_rad + delta_phi_0_rad); 
        phi_LH_rad = ClampAngleRad0To2pi(phi_LH_rad + delta_phi_LH_rad + delta_phi_0_rad); 
        phi_LF_rad = ClampAngleRad0To2pi(phi_LF_rad + delta_phi_LF_rad + delta_phi_0_rad); 

        //2. Endeffector position at this time step
        Vector3 p_RH = CPG_p_phi(phi_RH_rad,"RH");
        Vector3 p_RF = CPG_p_phi(phi_RF_rad,"RF");
        Vector3 p_LH = CPG_p_phi(phi_LH_rad,"LH");
        Vector3 p_LF = CPG_p_phi(phi_LF_rad,"LF");

        //3. Calculate target joint angle from IKd
        List<float> q_RH_deg = iK_RH.CalculateIK_rela(p_RH.x,p_RH.y,p_RH.z);//in degree
        List<float> q_RF_deg = iK_RF.CalculateIK_rela(p_RF.x,p_RF.y,p_RF.z);//in degree
        List<float> q_LH_deg = iK_LH.CalculateIK_rela(p_LH.x,p_LH.y,p_LH.z);//in degree 
        List<float> q_LF_deg = iK_LF.CalculateIK_rela(p_LF.x,p_LF.y,p_LF.z);//in degree

        float gamma_RH_deg = q_RH_deg[0] + delat_q_RH_HIP_rad * Mathf.Rad2Deg ;
        float alpha_RH_deg = q_RH_deg[1] + delat_q_RH_THIGH_rad * Mathf.Rad2Deg;
        float beta_RH_deg  = q_RH_deg[2] + delat_q_RH_SHANK_rad * Mathf.Rad2Deg;

        float gamma_RF_deg = q_RF_deg[0] + delat_q_RF_HIP_rad * Mathf.Rad2Deg ;
        float alpha_RF_deg = q_RF_deg[1] + delat_q_RF_THIGH_rad * Mathf.Rad2Deg;
        float beta_RF_deg  = q_RF_deg[2] + delat_q_RF_SHANK_rad * Mathf.Rad2Deg;

        float gamma_LH_deg = q_LH_deg[0] + delat_q_LH_HIP_rad * Mathf.Rad2Deg ;
        float alpha_LH_deg = q_LH_deg[1] + delat_q_LH_THIGH_rad * Mathf.Rad2Deg;
        float beta_LH_deg  = q_LH_deg[2] + delat_q_LH_SHANK_rad * Mathf.Rad2Deg;

        float gamma_LF_deg = q_LF_deg[0] + delat_q_LF_HIP_rad * Mathf.Rad2Deg ;
        float alpha_LF_deg = q_LF_deg[1] + delat_q_LF_THIGH_rad * Mathf.Rad2Deg;
        float beta_LF_deg  = q_LF_deg[2] + delat_q_LF_SHANK_rad * Mathf.Rad2Deg;


        // ------------------Apply the action-----------------------
        articulationBody_RH_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_RH_deg);
        articulationBody_RH_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_RH_deg);
        articulationBody_RH_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_RH_deg);

        articulationBody_RF_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_RF_deg);
        articulationBody_RF_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_RF_deg);
        articulationBody_RF_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_RF_deg);

        articulationBody_LH_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_LH_deg);
        articulationBody_LH_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_LH_deg);
        articulationBody_LH_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_LH_deg);

        articulationBody_LF_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_LF_deg);
        articulationBody_LF_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_LF_deg);
        articulationBody_LF_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_LF_deg);
    }

    public float ClampAngleRad0To2pi(float angle)
    {
        if(angle > 2 * Mathf.PI)
        {
            angle = angle - 2 * Mathf.PI;
        }

        if(angle < 0)
        {
            angle = angle + 2 * Mathf.PI;
        }

        return angle;
    }
    private Vector3 CPG_p_phi(float _phi_rad, string leg_id)
    {
        Vector3 pos_B_E_n = new Vector3(0,0,0);//norminal end effector position
        if(leg_id == "RH"){pos_B_E_n = pos_B_E_RH_0;}
        else if (leg_id == "RF"){pos_B_E_n = pos_B_E_RF_0;}
        else if (leg_id == "LH"){pos_B_E_n = pos_B_E_LH_0;}
        else if (leg_id == "LF"){pos_B_E_n = pos_B_E_LF_0;}
        else{Debug.LogError("[ERROR][CPG_p_phi]unknown leg_id:" + leg_id);}

        Vector3 pos_out = new Vector3(0,0,0);

        if( 0<=_phi_rad &&_phi_rad <= Mathf.PI/2 )
        {
            float t = (2/Mathf.PI)*_phi_rad;
            float f_t = (-2f * Mathf.Pow(t,3) + 3f*Mathf.Pow(t,2) );
            float x = pos_B_E_n.x;
            float y = pos_B_E_n.y + 0.2f * (-2f * Mathf.Pow(t,3) + 3f*Mathf.Pow(t,2) );
            float z = pos_B_E_n.z;
            pos_out = new Vector3(x,y,z);
            // Debug.Log($"[INFO][f(t)={f_t}][t={t}][_phi_rad={_phi_rad}]");

        }
        else if (Mathf.PI/2<_phi_rad && _phi_rad<=Mathf.PI )
        {
            float t = (2/Mathf.PI)*_phi_rad - 1;
            float f_t = (2f * Mathf.Pow(t,3) - 3f*Mathf.Pow(t,2) + 1);
            float x = pos_B_E_n.x;
            float y = pos_B_E_n.y + 0.2f * (2f * Mathf.Pow(t,3) - 3f*Mathf.Pow(t,2) + 1);
            float z = pos_B_E_n.z;
            pos_out = new Vector3(x,y,z);
            // Debug.Log($"[INFO][f(t)={f_t}][t={t}][_phi_rad={_phi_rad}]");
        }
        else
        {
            float x = pos_B_E_n.x;
            float y = pos_B_E_n.y;
            float z = pos_B_E_n.z;
            pos_out = new Vector3(x,y,z);
            // Debug.Log($"[INFO][f(t)={0}][t={0}][_phi_rad={_phi_rad}]");
        }

        return pos_out;
    }
    public override void Reset()
    {
        Destroy(root.gameObject);
        CreateNewBase();
    }
    public override void OnAgentStart()
    {
        agent_init_position = transform.position;
        CreateNewBase();
    }
    public override void OnEpisodeBegin()
    {
        Debug.Log("[INFO][QuadrupedAGentController]OnEpisodeBegin");
        InitializeControllerFields();
    }

    public bool BodyTouchingGround()
    { 
        bool touchingGround = groundContact_LF_SHANK1.touchingGround || groundContact_LF_SHANK2.touchingGround || groundContact_LF_THIGH.touchingGround ||
        groundContact_LH_SHANK1.touchingGround || groundContact_LH_SHANK2.touchingGround || groundContact_LH_THIGH.touchingGround ||
        groundContact_RF_SHANK1.touchingGround || groundContact_RF_SHANK2.touchingGround || groundContact_RF_THIGH.touchingGround ||
        groundContact_RH_SHANK1.touchingGround || groundContact_RH_SHANK2.touchingGround || groundContact_RH_THIGH.touchingGround;  
        return touchingGround;
    }
    public void CreateNewBase()
    {
        GameObject newBase =  Instantiate(basePrefab, agent_init_position, Quaternion.identity);
        newBase.transform.SetParent(transform, false);
        root = newBase.transform;

    }
    public void InitializeControllerFields()
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

        articulationBody_Root = root.GetComponent<ArticulationBody>();
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

        //CPG
        pos_B_E_RH_0 = RH_FOOT.position - base_inertia.position;
        pos_B_E_RF_0 = RF_FOOT.position - base_inertia.position;
        pos_B_E_LH_0 = LH_FOOT.position - base_inertia.position;
        pos_B_E_LF_0 = LF_FOOT.position - base_inertia.position;
    }

    //-----------------------------------------Testing Function-----------------------------------------------
    // private int num_update=0;
    // public GameObject ef_position_indicator_RH;
    // void Update()
    // {
    //     num_update++;
    //     TestIK();
    // }

    // public void TestCPGUpdate()
    // {
    //     //1. Phase offset per leg [Dim=4]
    //     delta_phi_RH_rad = 0;
    //     delta_phi_LH_rad = 0;
    //     delta_phi_RF_rad = 0;
    //     delta_phi_LF_rad = 0;

    //     //2. Residual Joint Position Target(in radius) [Dim=12]
    //     float delat_q_RH_HIP_rad = 0;
    //     float delat_q_RH_THIGH_rad = 0;
    //     float delat_q_RH_SHANK_rad = 0;

    //     float delat_q_LH_HIP_rad = 0;
    //     float delat_q_LH_THIGH_rad = 0;
    //     float delat_q_LH_SHANK_rad = 0;

    //     float delat_q_RF_HIP_rad = 0;
    //     float delat_q_RF_THIGH_rad = 0;
    //     float delat_q_RF_SHANK_rad = 0;

    //     float delat_q_LF_HIP_rad = 0;
    //     float delat_q_LF_THIGH_rad = 0;
    //     float delat_q_LF_SHANK_rad = 0;

    //     //1. Update phi
    //     phi_RH_rad = ClampAngleRad0To2pi(phi_RH_rad + delta_phi_RH_rad + delta_phi_0_rad); 
    //     phi_RF_rad = ClampAngleRad0To2pi(phi_RF_rad + delta_phi_RF_rad + delta_phi_0_rad); 
    //     phi_LH_rad = ClampAngleRad0To2pi(phi_LH_rad + delta_phi_LH_rad + delta_phi_0_rad); 
    //     phi_LF_rad = ClampAngleRad0To2pi(phi_LF_rad + delta_phi_LF_rad + delta_phi_0_rad); 
    //     Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]phi_RH_rad={phi_RH_rad}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]phi_RH_rad={phi_RF_rad}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]phi_RH_rad={phi_LH_rad}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]phi_RH_rad={phi_LF_rad}");

    //     //2. Endeffector position at this time step
    //     Vector3 p_RH = CPG_p_phi(phi_RH_rad,"RH");
    //     // Vector3 p_RF = CPG_p_phi(phi_RF_rad,"RF");
    //     // Vector3 p_LH = CPG_p_phi(phi_LH_rad,"LH");
    //     // Vector3 p_LF = CPG_p_phi(phi_LF_rad,"LF");
    //     Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]p_RH={p_RH}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]p_RF={p_RF}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]p_LH={p_LH}");
    //     // Debug.Log($"[INFO][QuadrupedAgentController][TestCPGUpdate][num_update={num_update}]p_LF={p_LF}");
    //     ef_position_indicator_RH.transform.position = p_RH + base_inertia.position;

    //     //3. Calculate target joint angle from IKd
    //     List<float> q_RH_deg = iK_RH.CalculateIK_rela(p_RH.x,p_RH.y,p_RH.z);//in degree
    //     // List<float> q_RF_deg = iK_RF.CalculateIK_rela(p_RF.x,p_RF.y,p_RF.z);//in degree
    //     // List<float> q_LH_deg = iK_LH.CalculateIK_rela(p_LH.x,p_LH.y,p_LH.z);//in degree 
    //     // List<float> q_LF_deg = iK_LF.CalculateIK_rela(p_LF.x,p_LF.y,p_LF.z);//in degree

    //     float gamma_RH_deg = q_RH_deg[0] + delat_q_RH_HIP_rad * Mathf.Rad2Deg ;
    //     float alpha_RH_deg = q_RH_deg[1] + delat_q_RH_THIGH_rad * Mathf.Rad2Deg;
    //     float beta_RH_deg  = q_RH_deg[2] + delat_q_RH_SHANK_rad * Mathf.Rad2Deg;

    //     // float gamma_RF_deg = q_RF_deg[0] + delat_q_RF_HIP_rad * Mathf.Rad2Deg ;
    //     // float alpha_RF_deg = q_RF_deg[1] + delat_q_RF_THIGH_rad * Mathf.Rad2Deg;
    //     // float beta_RF_deg  = q_RF_deg[2] + delat_q_RF_SHANK_rad * Mathf.Rad2Deg;

    //     // float gamma_LH_deg = q_LH_deg[0] + delat_q_LH_HIP_rad * Mathf.Rad2Deg ;
    //     // float alpha_LH_deg = q_LH_deg[1] + delat_q_LH_THIGH_rad * Mathf.Rad2Deg;
    //     // float beta_LH_deg  = q_LH_deg[2] + delat_q_LH_SHANK_rad * Mathf.Rad2Deg;

    //     // float gamma_LF_deg = q_LF_deg[0] + delat_q_LF_HIP_rad * Mathf.Rad2Deg ;
    //     // float alpha_LF_deg = q_LF_deg[1] + delat_q_LF_THIGH_rad * Mathf.Rad2Deg;
    //     // float beta_LF_deg  = q_LF_deg[2] + delat_q_LF_SHANK_rad * Mathf.Rad2Deg;


    //     // ------------------Apply the action-----------------------
    //     articulationBody_RH_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_RH_deg);
    //     articulationBody_RH_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_RH_deg);
    //     articulationBody_RH_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_RH_deg);

    //     // articulationBody_RF_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_RF_deg);
    //     // articulationBody_RF_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_RF_deg);
    //     // articulationBody_RF_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_RF_deg);

    //     // articulationBody_LH_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_LH_deg);
    //     // articulationBody_LH_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_LH_deg);
    //     // articulationBody_LH_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_LH_deg);

    //     // articulationBody_LF_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_LF_deg);
    //     // articulationBody_LF_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_LF_deg);
    //     // articulationBody_LF_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_LF_deg);
    // }
    // public void TestIK()
    // {
    //     List<float> q_RH_deg = iK_RH.CalculateIK_rela(ef_position_indicator_RH.transform.position.x-base_inertia.position.x,ef_position_indicator_RH.transform.position.y-base_inertia.position.y,ef_position_indicator_RH.transform.position.z-base_inertia.position.z);//in degree
    //     float gamma_RH_deg = q_RH_deg[0] ;
    //     float alpha_RH_deg = q_RH_deg[1] ;
    //     float beta_RH_deg  = q_RH_deg[2] ;
    //     // ------------------Apply the action-----------------------
    //     articulationBody_RH_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_RH_deg);
    //     articulationBody_RH_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_RH_deg);
    //     articulationBody_RH_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_RH_deg);
    // }


}
