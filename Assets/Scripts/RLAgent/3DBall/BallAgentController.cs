using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallAgentController : AgentControllerBase
{
    public GameObject ball;
    private Rigidbody m_BallRb;
    public int action_dim = 2;
    public override void Reset()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        m_BallRb.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
            + gameObject.transform.position;
    }

    public override void ExecuteAction()
    {
        var actionZ = 2f * Mathf.Clamp(action_list[0], -1f, 1f);
        var actionX = 2f * Mathf.Clamp(action_list[1], -1f, 1f);

        if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
            (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        }

        if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
            (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_BallRb = ball.GetComponent<Rigidbody>();
        initializeAction(action_dim: action_dim);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnEpisodeBegin()
    {
    }

    public override void OnAgentStart()
    {
    }
}
