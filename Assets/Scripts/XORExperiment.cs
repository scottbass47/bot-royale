using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharpNeat.Core;
using SharpNeat.Network;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Decoders.Neat;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using SharpNeat.Phenomes;

public class XORExperiment : MonoBehaviour
{
    [SerializeField] private NeuralNetworkMesh nnMesh;

    private void Start()
    {
        int populationSize = 100;
        NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateAcyclicScheme();

        NeatGenomeParameters neatParams = new NeatGenomeParameters();
        neatParams.ActivationFn = ReLU.__DefaultInstance;
        neatParams.FeedforwardOnly = activationScheme.AcyclicNetwork;

        IGenomeDecoder<NeatGenome, IBlackBox> neatDecoder = new NeatGenomeDecoder(activationScheme);

        IGenomeFactory<NeatGenome> neatFactory = new NeatGenomeFactory(2, 1, neatParams);
        List<NeatGenome> genomeList = neatFactory.CreateGenomeList(populationSize, 0);
        XOREvaluator evaluator = new XOREvaluator();

        IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

        // Evolution parameters
        NeatEvolutionAlgorithmParameters neatEvolutionParams = new NeatEvolutionAlgorithmParameters();
        neatEvolutionParams.SpecieCount = 10;
        ISpeciationStrategy<NeatGenome> speciationStrategy = new KMeansClusteringStrategy<NeatGenome>(distanceMetric);
        IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, 10);

        IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new SimpleGenomeListEvaluator<NeatGenome, IBlackBox>(neatDecoder, evaluator);

        NeatEvolutionAlgorithm<NeatGenome> ea = GetComponent<UnityEvolutionAlgorithm>();
        ea.Construct(neatEvolutionParams, speciationStrategy, complexityRegulationStrategy);
        ea.Initialize(genomeListEvaluator, neatFactory, genomeList);
        ea.UpdateScheme = new UpdateScheme(1); // This needs to be set AFTER Initialize is called

        ea.PausedEvent += (sender, e) =>
        {
            //ea.StartContinue();
        };
        ea.GenerationEvent += (sender, gen) =>
        {
            Debug.Log($"Generation {gen}");
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
