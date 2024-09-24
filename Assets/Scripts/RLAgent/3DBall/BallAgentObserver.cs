using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallAgentObserver : AgentObserverBase
{
    public GameObject ball;
    private Rigidbody m_BallRb;

    public override List<float> GetObservations()
    {
        ClearObservationList();
        AddToObservationList(gameObject.transform.rotation.z,name:"[gameObject.transform.rotation.z]"); 
        AddToObservationList(gameObject.transform.rotation.x,name:"[gameObject.transform.rotation.x]");
        AddToObservationList(ball.transform.position - gameObject.transform.position,name:"[ball.transform.position - gameObject.transform.position]");
        AddToObservationList(m_BallRb.velocity,name:"[m_BallRb.velocity]");

        return observation_list;
    }

    public override void OnAgentStart()
    {
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void Reset()
    {
        ClearObservationList();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
