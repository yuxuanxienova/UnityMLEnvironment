using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class Agent : MonoBehaviour
{    
    private static int nextId = 0; // Static counter to track the next available ID

    [Header("Settings")][Space(10)]
    public AgentObserverBase agentObserver;
    public AgentControllerBase agentController;
    public AgentRewardCalculatorBase agentRewardCalculator;

    // public ServiceClientSampleAction serviceClientSampleAction;
    //public PublishEventSampleAction publishEventSampleAction;
    //public PublishTransition publishTransition;
    //public PublishEpisodicReward publishEpisodicReward;
    public Client client;
    private bool trancated_flag=false;

    private float timeElapsed_actionUpdate = 0;
    public float actionUpdateInterval = 0.2f;

    private float timeElapsed_transitionPublishUpdate = 0;
    public float transitionPublishUpdateInterval = 0.1f;

    private float[] state_stored;
    private float[] action_stored;

    private int episodeCount = 0;

    public int id = 0;

    private int num_update_action=0;

    void Start()
    {
        // Assign a unique ID to this agent
        id = nextId;
        nextId++; // Increment the static counter for the next agent

        // Add this agent to the AgentManager dictionary
        // AgentManager.Instance.AddAgentToDict(id, this);

        //Register the listener for the response message
        NetManager.AddMsgListener("SampleActionResponseMsg", OnSampleActionResponse);
        

        //Call Start for all components
        agentController.OnAgentStart();
        agentObserver.OnAgentStart();
        agentRewardCalculator.OnAgentStart();

        //Call Episode Begin
        OnEpisodeBegin();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAction();
        UpdateTransitionPublish();

    }
    public void Reset()
    {
        episodeCount += 1;
        client.CallPublishEpisodicReward(agentRewardCalculator.GetEpisodeReward());
        Debug.Log($"[INFO][Agent][agent_id={id}][episodeCount={episodeCount}][episodeReward={agentRewardCalculator.GetEpisodeReward()}])");
        Debug.Log($"[INFO][Agent][agent_id={id}][episodeCount={episodeCount}]Resetting agent");

        //Reset all components
        agentController.Reset();
        agentObserver.Reset();
        agentRewardCalculator.Reset();

        //Set the flag
        trancated_flag=true;

        //new episode
        OnEpisodeBegin();
    }
    public void OnEpisodeBegin()
    {
        Debug.Log($"[INFO][Agent][agent_id={id}]OnEpisodeBegin");
        //Initialize all components
        agentController.OnEpisodeBegin();
        agentObserver.OnEpisodeBegin();
        agentRewardCalculator.OnEpisodeBegin();

    }
    private void UpdateAction()
    {
        timeElapsed_actionUpdate += Time.deltaTime;
        if(timeElapsed_actionUpdate > actionUpdateInterval)
        {
            num_update_action+=1;
            


            float[] obs_arr = GetObservation();
            // UnityEngine.Debug.Log($"[INFO][Agent][agent_id={id}][num_update_action={num_update_action}]Observation:"+ExtensionMethods.FloatArrayToString(obs_arr));
            client.CallPublishEventSampleAction(obs_arr,id_agent:id);
            timeElapsed_actionUpdate=0;

        }
    }
    public void OnSampleActionResponse(MsgBase msgBase)
    {
        // Handle the response specific to this agent
        // Debug.Log($"[INFO][Agent][agent_id={id}][num_update_action={num_update_action}]Received action: " + string.Join(",", action));
        SampleActionResponseMsg msg = (SampleActionResponseMsg)msgBase;
        List<float> floatList = new List<float>(msg.action);
        SetExecuteAction(floatList);
    }

    private void UpdateTransitionPublish()
    {
        timeElapsed_transitionPublishUpdate += Time.deltaTime;
        if(timeElapsed_transitionPublishUpdate > transitionPublishUpdateInterval)
        {
            float[] state_tminus1 = state_stored;
            float[] action_tminus1 = action_stored;
            float[] reward_tminus1 = CalculateReward();
            float[] state_t = GetObservation();

            state_stored = GetObservation();
            action_stored = GetAction();

            client.CallPublishTransition(state_tminus1, action_tminus1, reward_tminus1, state_t, trancated_flag);
            trancated_flag = false;
            timeElapsed_transitionPublishUpdate=0;
        }
    }

    public void SetTrancatedFlag(bool flag)
    {
        trancated_flag=flag;
    }
    public bool GetTrancaredFlag()
    {
        return trancated_flag;
    }
    public void SetExecuteAction(List<float> action_list)
    {
        agentController.SetAction(action_list);
        agentController.ExecuteAction();
    }

    public float[] GetAction()
    {
        List<float> action_list = agentController.GetAction();
        if(action_list != null)
        {
            return action_list.ToArray();
        }
        else
        {
            return new float[0];
        }
    }

    public float[] GetObservation()
    {
        List<float> observation_list = agentObserver.GetObservations();
        agentObserver.CheckObservationListDim();
        if(observation_list != null)
        {
            return observation_list.ToArray();
        }
        else
        {
            return new float[0];
        }
    }
    public float[] CalculateReward()
    {
        List<float> reward_list = agentRewardCalculator.CalculateReward();
        if(reward_list != null)
        {
            return reward_list.ToArray();
        }
        else
        {
            return new float[0];
        }
    }
}
