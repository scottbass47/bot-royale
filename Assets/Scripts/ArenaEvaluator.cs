using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArenaEvaluator : MonoBehaviour, IGenomeListEvaluator<NeatGenome>
{
    private IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder;

    // TODO: When the arena trial finishes: evalCount += populationSize
    private ulong evalCount;
    public ulong EvaluationCount => evalCount;
    public bool StopConditionSatisfied => false;

    [SerializeField] private GameObject botPrefab;
    [SerializeField] private Transform spawnPoint;

    private Dictionary<uint, GameObject> botObjects;
    private Dictionary<uint, NeatGenome> botGenomes;

    private void Awake()
    {
        botObjects = new Dictionary<uint, GameObject>();
        botGenomes = new Dictionary<uint, NeatGenome>();
    }

    public void Initialize(IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder)
    {
        this.genomeDecoder = genomeDecoder;
    }

    public void Reset()
    {
    }

    public IEnumerator Evaluate(IList<NeatGenome> genomeList)
    {
        foreach(NeatGenome genome in genomeList)
        {
            var botObject = Instantiate(botPrefab);
            botObject.transform.position = spawnPoint.transform.position;

            var botController = botObject.GetComponent<BotController>();
            var brain = genomeDecoder.Decode(genome);
            botController.SetBrain(brain);
            botController.SetStartPosition(botObject.transform.position);
            botController.OnCollide += SaveFitnessAndDestroyBot;
            botController.OnExceedMaxIdleTime += SaveFitnessAndDestroyBot;

            botController.BotId = genome.Id;
            botObjects.Add(genome.Id, botObject);
            botGenomes.Add(genome.Id, genome);
        }
        // Debug
        yield return null;
        PickRandomBot().GetComponent<BotController>().TurnOnDebugRendering();

        yield return StartCoroutine(RunArena());

        foreach(uint genomeId in botObjects.Keys)
        {
            botGenomes[genomeId].EvaluationInfo.SetFitness(0.0);

            Destroy(botObjects[genomeId]);
        }

        botObjects.Clear();
        botGenomes.Clear();
    }

    private GameObject PickRandomBot()
    {
        List<GameObject> bots = Enumerable.ToList(botObjects.Values);
        return bots[Random.Range(0, bots.Count)];
    }

    private void SaveFitnessAndDestroyBot(GameObject bot)
    {
        var botController = bot.GetComponent<BotController>();
        var botId = botController.BotId;

        // Already saved
        if (!botObjects.ContainsKey(botId)) return;

        botGenomes[botId].EvaluationInfo.SetFitness(botController.Fitness);

        botObjects.Remove(botId);
        botGenomes.Remove(botId);

        // Before we destroy the bot, transfer debug rendering to another bot
        if (botObjects.Count > 0 && botController.IsDebugRendering())
        {
            PickRandomBot().GetComponent<BotController>().TurnOnDebugRendering();
        }
        Destroy(bot);
    }

    private IEnumerator RunArena()
    {
        Debug.Log("Let the games begin!");
        float elapsed = 0.0f;
        for(;;)
        {
            elapsed += Time.deltaTime;
            if(elapsed >= 10f || botObjects.Count == 0)
            {
                Debug.Log("Match complete.");
                yield break;
            }
            yield return null;
        }
    }
}
