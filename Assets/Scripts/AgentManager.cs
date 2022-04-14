using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private CameraController cam;
    [SerializeField] private float mutationThreshold = 0.5f;
    private int startMutatingIndex;
    
    [Range(1, 100)]
    [SerializeField] private float timeScale = 1;
    
    public int populationSize = 200;
    public int generationNb;
    
    [Range(1, 120)]
    public float trainingDuration = 30;

    [Range(0, 100)] 
    public float mutationRate = 5;

    [Space]
    private List<Agent> agents = new List<Agent>();

    private List<Agent> agentsInLead = new List<Agent>();
    
    private Agent agent;

    private int diff;
    private int x;
    
    public Agent agentPrefab;
    public Transform agentGroup;

    public TMP_Text chronoText;
    public TMP_Text generationText;
    private float startingTime;

    void Start()
    {
        StartCoroutine(Loop());
        agents.Sort();
        NeuralNetworkViewer.instance.RefreshAgent(agents[0]);

        startingTime = Time.time;
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        RefreshTimer();
        Refocus();
    }

    private IEnumerator Loop()
    {
        StartNewGeneration();
        ResetTimer();
        yield return new WaitForSeconds(trainingDuration);
        StartCoroutine(Loop());
    }

    private void StartNewGeneration()
    {
        generationNb += 1;
        generationText.text = generationNb.ToString();
        agents.Sort();
        AddOrRemoveAgents();
        Mutate();
        ResetAgents();
        SetMaterials();
        cam.target = agents[0].transform;
    }

    private void AddOrRemoveAgents()
    {
        if (agents.Count != populationSize)
        {
            diff = populationSize - agents.Count;

            if (diff > 0)
            {
                for (x = 0; x < diff; x++)
                {
                    AddAgents();
                }
            }
            else
            {
                for (x = 0; x < -diff; x++)
                {
                    RemoveAgent();
                }
            }
        }
    }

    private void AddAgents()
    {
        agent = Instantiate(agentPrefab, Vector3.zero, Quaternion.identity, agentGroup);
        agent.net = new NeuralNetwork(agentPrefab.net.layers);
        agents.Add(agent);
        agentsInLead.Add(agent);
    }
    
    private void RemoveAgent()
    {
        Destroy(agents[agents.Count-1].gameObject);
        agents.RemoveAt(agents.Count-1);
        agentsInLead.RemoveAt(agentsInLead.Count-1);
    }
    
    private void Mutate()
    {
        startMutatingIndex = agents.Count / 2;
        float bestFitness = agents[0].fitness;
        bool lessThanFifty = false;

        for (int i = 1; i < agents.Count / 2; i++)
        {
            if (agents[i].fitness < bestFitness * mutationThreshold)
            {
                startMutatingIndex = i;
                lessThanFifty = true;
                break;
            }
        }

        if (!lessThanFifty)
        {
            mutationThreshold = Mathf.Lerp(mutationThreshold, 1, 0.1f);
        }

        for (x = startMutatingIndex; x < agents.Count; x++)
        {
            agents[x].net.CopyNet(agents[(x - startMutatingIndex) % startMutatingIndex].net);
            agents[x].net.Mutate(mutationRate);
            agents[x].SetMutantMaterial();
        }
    }
    
    private void ResetAgents()
    {
        for (x = 0; x < agents.Count; x++)
        {
            agents[x].ResetAgent();
        }
    }
    
    private void SetMaterials()
    {
        for (x = 1; x < startMutatingIndex; x++)
        {
            agents[x].SetDefaultMaterial();
        }
        
        agents[0].SetFirstMaterial();
    }

    public void Save()
    {
        List<NeuralNetwork> nets = new List<NeuralNetwork>();

        for (x = 0; x < agents.Count; x++)
        {
            nets.Add(agents[x].net);
        }
        
        DataManager.instance.Save(nets);
    }

    public void Load()
    {
        //Doit avoir la même taille que la sauvegarde au load
        
        Data data = DataManager.instance.Load();

        if (data != null)
        {
            for (x = 0; x < agents.Count; x++)
            {
                agents[x].net = data.nets[x];
            }
        }
        End();
    }

    public void End()
    { 
        StopAllCoroutines();
        StartCoroutine(Loop());
    }

    void RefreshTimer()
    {
        chronoText.text = (trainingDuration - (Time.time - startingTime)).ToString("f0");
    }

    void ResetTimer()
    {
        startingTime = Time.time;
    }

    public void Refocus()
    {
        agentsInLead = agentsInLead.OrderBy(a=>a.distanceTravelled*-1).ToList();
        NeuralNetworkViewer.instance.RefreshAgent(agentsInLead[0]);
        cam.target = agentsInLead[0].transform;
    }
}
