using System; 
using System.Collections.Generic; 
using UnityEngine; 
using System.Linq;
public static class ExtensionMethods 
{ 
    //ROS-Unity Conversion

    public static Vector3 VecRos2Unity(this Vector3 vector3_ros) 
    { 
        return new Vector3(-vector3_ros.y, vector3_ros.z, vector3_ros.x); 
    } 

    public static Vector3 VecUnity2Ros(this Vector3 vector3_unity) 
    { 
        return new Vector3(vector3_unity.z,-vector3_unity.x,vector3_unity.y); 
    } 

    public static Vector3 EulerRos2Unity(this Vector3 vector3_ros) 
    { 
        return new Vector3(vector3_ros.y, -vector3_ros.z, -vector3_ros.x); 
    } 

    public static Vector3 EulerUnity2Ros(this Vector3 vector3_unity) 
    { 
        return new Vector3(-vector3_unity.z,vector3_unity.x,-vector3_unity.y); 
    } 

    public static Quaternion QuaternionRos2Unity(this Quaternion qua_ros) 
    { 
        return new Quaternion(qua_ros.y, -qua_ros.z, -qua_ros.x, qua_ros.w); 
    } 

    public static Quaternion QuaternionUnity2Ros(this Quaternion qua_unity) 
    { 
        return new Quaternion(-qua_unity.z,qua_unity.x,-qua_unity.y,qua_unity.w); 
    } 

    //Type Conversion
    public static  string ListToString(List<float> list)
    {
        return string.Join(", ", list);
    }
    public static  string FloatArrayToString(float[] array)
    {
        // Using Select to format each float before joining
        return string.Join(", ", array.Select(f => f.ToString("F2")));
    }

} 
