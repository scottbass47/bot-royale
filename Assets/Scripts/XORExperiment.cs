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
    private void Start()
    {
        int populationSize = 50;
        NetworkActivationScheme activationScheme = NetworkActivationScheme.CreateAcyclicScheme();

        NeatGenomeParameters neatParams = new NeatGenomeParameters();
        neatParams.ActivationFn = ReLU.__DefaultInstance;
        neatParams.FeedforwardOnly = activationScheme.AcyclicNetwork;

        IGenomeDecoder<NeatGenome, IBlackBox> neatDecoder = new NeatGenomeDecoder(activationScheme);

        IGenomeFactory<NeatGenome> neatFactory = new NeatGenomeFactory(2, 1, neatParams);
        List<NeatGenome> genomeList = neatFactory.CreateGenomeList(populationSize, 0);
        XOREvaluator evaluator = new XOREvaluator();

        ParallelOptions parallelOptions = new ParallelOptions();
        IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);

        // Evolution parameters
        NeatEvolutionAlgorithmParameters neatEvolutionParams = new NeatEvolutionAlgorithmParameters();
        neatEvolutionParams.SpecieCount = 10;
        ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, parallelOptions);
        IComplexityRegulationStrategy complexityRegulationStrategy = new DefaultComplexityRegulationStrategy(ComplexityCeilingType.Absolute, 50);

        NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(neatEvolutionParams, speciationStrategy, complexityRegulationStrategy);
        ea.UpdateScheme = new UpdateScheme(1);

        IGenomeListEvaluator<NeatGenome> innerEvaluator = new ParallelGenomeListEvaluator<NeatGenome, IBlackBox>(neatDecoder, evaluator, parallelOptions);
        IGenomeListEvaluator<NeatGenome> selectiveEvaluator = new SelectiveGenomeListEvaluator<NeatGenome>
            (innerEvaluator, SelectiveGenomeListEvaluator<NeatGenome>.CreatePredicate_OnceOnly());

        ea.Initialize(selectiveEvaluator, neatFactory, genomeList);
        ea.UpdateEvent += (sender, e) =>
        {
            //Debug.Log("UPDATE");
            //Debug.Log(ea.CurrentGeneration);
        };
        ea.PausedEvent += (sender, e) =>
        {
            //Debug.Log("PAUSED");
            Debug.Log($"Finished on generation {ea.CurrentGeneration}");
        };
        ea.StartContinue();
    }
}
