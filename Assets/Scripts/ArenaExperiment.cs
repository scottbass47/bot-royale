using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.DistanceMetrics;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using SharpNeat.Phenomes;
using SharpNeat.SpeciationStrategies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaExperiment : MonoBehaviour
{
    [SerializeField] private NeuralNetworkMesh nnMesh;

    private void Start()
    {
        int populationSize = 100;
        NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateAcyclicScheme();

        NeatGenomeParameters neatParams = new NeatGenomeParameters();
        neatParams.ActivationFn = TanH.__DefaultInstance;
        neatParams.FeedforwardOnly = activationScheme.AcyclicNetwork;

        IGenomeDecoder<NeatGenome, IBlackBox> neatDecoder = new NeatGenomeDecoder(activationScheme);

        IGenomeFactory<NeatGenome> neatFactory = new NeatGenomeFactory(3, 3, neatParams);
        List<NeatGenome> genomeList = neatFactory.CreateGenomeList(populationSize, 0);
        ArenaEvaluator evaluator = GetComponent<ArenaEvaluator>();
        evaluator.Initialize(neatDecoder);

        IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

        // Evolution parameters
        NeatEvolutionAlgorithmParameters neatEvolutionParams = new NeatEvolutionAlgorithmParameters();
        neatEvolutionParams.SpecieCount = 10;
        ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);
        IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, 10);

        NeatEvolutionAlgorithm<NeatGenome> ea = GetComponent<UnityEvolutionAlgorithm>();
        ea.Construct(neatEvolutionParams, speciationStrategy, complexityRegulationStrategy, new NullGenomeListEvaluator<NeatGenome, IBlackBox>());
        ea.Initialize(evaluator, neatFactory, genomeList);
        ea.UpdateScheme = new UpdateScheme(1); // This needs to be set AFTER Initialize is called

        ea.PausedEvent += (sender, e) =>
        {
            //ea.StartContinue();
        };
        ea.GenerationEvent += (sender, gen) =>
        {
            Debug.Log($"Generation {gen}");
            Debug.Log($"Highest fitness: {ea.CurrentChampGenome.EvaluationInfo.Fitness}");
            nnMesh.GenerateMesh(ea.CurrentChampGenome);
            ea.RequestPause();
            StartCoroutine(PauseRoutine(ea));
        };

        ea.StartContinue();
    }

    private IEnumerator PauseRoutine(IEvolutionAlgorithm<NeatGenome> ea)
    {
        yield return null;
        ea.StartContinue();
    }
}
