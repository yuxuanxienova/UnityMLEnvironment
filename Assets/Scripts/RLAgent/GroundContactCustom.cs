using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundContactCustom : MonoBehaviour
{

        public bool touchingGround;
        const string k_Ground = "ground"; // Tag of ground object.

        /// <summary>
        /// Check for collision with ground, and optionally penalize agent.
        /// </summary>
        void OnCollisionEnter(Collision col)
        {
            
            if (col.transform.CompareTag(k_Ground))
            {
                touchingGround = true;
                // Debug.Log("[INFO][GroundContact]OnCollisionEnter touchingGround:"+ touchingGround);
            }
        }

        /// <summary>
        /// Check for end of ground collision and reset flag appropriately.
        /// </summary>
        void OnCollisionExit(Collision other)
        {
            if (other.transform.CompareTag(k_Ground))
            {
                touchingGround = false;
            }
        }
}
