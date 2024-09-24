using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK_RF : MonoBehaviour
{
    // public GameObject baseInertiaObj;
    // public GameObject endEffectorObj;
    // public ArticulationBody articulationBody_RF_HIP;
    // public ArticulationBody articulationBody_RF_THIGH;
    // public ArticulationBody articulationBody_RF_SHANK;

    public float L_h0 = 0.1f;
    public float L_h1 = 0.06f;

    public float L_h2 = 0.1f;
    public float L_v2 = 0.285f;
    public float L_v3 = 0.338f;

    public float L_h3 = 0.01305f;

    public float L_PE = 0.088f;

    public float L_IA1_z = 0.3f;
    public float L_A1A2_z = 0.0838f; 
    // Start is called before the first frame update


    
    void Start()
    {
        // articulationBody_RF_HIP.SetDriveForceLimit(ArticulationDriveAxis.X,80f);
        // articulationBody_RF_THIGH.SetDriveForceLimit(ArticulationDriveAxis.X,80f);
        // articulationBody_RF_SHANK.SetDriveForceLimit(ArticulationDriveAxis.X,80f);
        
    }

    // Update is called once per frame
    void Update()
    {
        // articulationBody_RF_HIP.SetDriveForceLimit(ArticulationDriveAxis.X,80f);
        // articulationBody_RF_THIGH.SetDriveForceLimit(ArticulationDriveAxis.X,80f);
        // articulationBody_RF_SHANK.SetDriveForceLimit(ArticulationDriveAxis.X,80f);


        // // float alpha_rad = CalculateAlphaRad(endEffectorObj.transform.position.y - baseInertiaObj.transform.position.y, endEffectorObj.transform.position.z - baseInertiaObj.transform.position.z);
        // // float alpha_deg = alpha_rad * Mathf.Rad2Deg;
        // // alpha_deg = MapAngleDeg(alpha_deg);

        // // float beta_rad = CalculateBetaRad(endEffectorObj.transform.position.y - baseInertiaObj.transform.position.y, endEffectorObj.transform.position.z - baseInertiaObj.transform.position.z);
        // // float beta_deg = beta_rad * Mathf.Rad2Deg;
        // // beta_deg = MapAngleDeg(beta_deg);

        // // float gamma_rad = CalculateGammaRad(endEffectorObj.transform.position.x - baseInertiaObj.transform.position.x, endEffectorObj.transform.position.y - baseInertiaObj.transform.position.y,alpha_deg,beta_deg);
        // // float gamma_deg = gamma_rad * Mathf.Rad2Deg;

        // // Debug.Log("[INFO]gamma_deg=" + gamma_deg);
        // // Debug.Log("[INFO]alpha_deg=" + alpha_deg);
        // // Debug.Log("[INFO]beta_deg=" + beta_deg);

        // List<float> joint_list = CalculateIK(endEffectorObj.transform.position.x,endEffectorObj.transform.position.y,endEffectorObj.transform.position.z);
        // float gamma_deg = joint_list[0];
        // float alpha_deg = joint_list[1];
        // float beta_deg = joint_list[2];

        // articulationBody_RF_HIP.SetDriveTarget(ArticulationDriveAxis.X,gamma_deg);
        // articulationBody_RF_THIGH.SetDriveTarget(ArticulationDriveAxis.X,alpha_deg);
        // articulationBody_RF_SHANK.SetDriveTarget(ArticulationDriveAxis.X,beta_deg);

    }
    // public List<float> CalculateIK(float I_E_x, float I_E_y, float I_E_z)
    // {
    //     float alpha_rad = CalculateAlphaRad(I_E_y - baseInertiaObj.transform.position.y, I_E_z - baseInertiaObj.transform.position.z);
    //     float alpha_deg = alpha_rad * Mathf.Rad2Deg;
    //     alpha_deg = MapAngleDeg(alpha_deg);

    //     float beta_rad = CalculateBetaRad(I_E_y - baseInertiaObj.transform.position.y, I_E_z - baseInertiaObj.transform.position.z);
    //     float beta_deg = beta_rad * Mathf.Rad2Deg;
    //     beta_deg = MapAngleDeg(beta_deg);

    //     float gamma_rad = CalculateGammaRad(I_E_x - baseInertiaObj.transform.position.x, I_E_y - baseInertiaObj.transform.position.y,alpha_deg,beta_deg);
    //     float gamma_deg = gamma_rad * Mathf.Rad2Deg;

    //     return new List<float>{gamma_deg,alpha_deg,beta_deg};

    // }

    public List<float> CalculateIK_rela(float B_E_x, float B_E_y, float B_E_z)
    {
        float alpha_rad = CalculateAlphaRad(B_E_y, B_E_z);
        float alpha_deg = alpha_rad * Mathf.Rad2Deg;
        alpha_deg = MapAngleDeg(alpha_deg);

        float beta_rad = CalculateBetaRad(B_E_y , B_E_z );
        float beta_deg = beta_rad * Mathf.Rad2Deg;
        beta_deg = MapAngleDeg(beta_deg);

        float gamma_rad = CalculateGammaRad(B_E_x, B_E_y ,alpha_deg,beta_deg);
        float gamma_deg = gamma_rad * Mathf.Rad2Deg;

        return new List<float>{gamma_deg,alpha_deg,beta_deg};
    }

    public float CalculateGammaRad(float B_E_x, float B_E_y, float alpha_deg, float beta_deg)
    {
        float alpha_rad  = alpha_deg * Mathf.Deg2Rad;
        float beta_rad = beta_deg * Mathf.Deg2Rad;

        float theta_2_rad = Mathf.Atan2(B_E_x-L_h0, -B_E_y);
        float theta_1_rad = Mathf.Atan2(L_h1+L_h2+L_h3 , L_v2*Mathf.Cos(alpha_rad) + L_v3*Mathf.Cos(alpha_rad+beta_rad));

        float gamma_rad = -theta_2_rad + theta_1_rad;

        return gamma_rad;
    }

    public float CalculateAlphaRad(float B_E_y, float B_E_z)
    {
        float L_A2E = Mathf.Sqrt(B_E_y*B_E_y + B_E_z*B_E_z);
        if(L_A2E > L_v2 + L_v3)
        {
            L_A2E = L_v2 + L_v3; 
        }

        float n = (L_A2E*L_A2E - L_v3*L_v3  - L_PE*L_PE - L_v2*L_v2)/(2*L_v2);
        float m = Mathf.Sqrt(L_v3*L_v3 + L_PE*L_PE - n*n);

        float theta_4_rad = Mathf.PI + Mathf.Atan2(B_E_z - L_IA1_z - L_A1A2_z,B_E_y); 
        float phi_rad = Mathf.Atan2(L_PE,L_v3);
        float theta_3_rad = Mathf.Acos((L_v2+n)/L_A2E);
        float alpha_rad = -(theta_3_rad - theta_4_rad);
        return alpha_rad;
    }

    public float CalculateBetaRad(float B_E_y, float B_E_z)
    {
        float L_A2E = Mathf.Sqrt(B_E_y*B_E_y + B_E_z*B_E_z);

        if(L_A2E > L_v2 + L_v3)
        {
            L_A2E = L_v2 + L_v3; 
        }
        float n = (L_A2E*L_A2E - L_v3*L_v3  - L_PE*L_PE - L_v2*L_v2)/(2*L_v2);
        float m = Mathf.Sqrt(L_v3*L_v3 + L_PE*L_PE - n*n);
        float phi_rad = Mathf.Atan2(L_PE,L_v3);
        float theta_5_rad = Mathf.Atan2(m,n) ;
        float beta_rad = theta_5_rad + phi_rad;
        return beta_rad;
    }

    public float MapAngleDeg(float angle)
    {
        if(angle > 180)
        {
            angle = angle - 360;
        }
        
        if(angle < -180)
        {
            angle = angle + 360;
        }

        return angle;
    }
}
