using SharpNeat.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleGenomeListEvaluator<TGenome, TPhenome> : IGenomeListEvaluator<TGenome>
    where TGenome : class, IGenome<TGenome>
    where TPhenome : class
{
    private IGenomeDecoder<TGenome, TPhenome> genomeDecoder;
    private IPhenomeEvaluator<TPhenome> phenomeEvaluator;

    public ulong EvaluationCount => phenomeEvaluator.EvaluationCount;
    public bool StopConditionSatisfied => phenomeEvaluator.StopConditionSatisfied;

    public SimpleGenomeListEvaluator(IGenomeDecoder<TGenome, TPhenome> genomeDecoder, IPhenomeEvaluator<TPhenome> phenomeEvaluator)
    {
        this.genomeDecoder = genomeDecoder;
        this.phenomeEvaluator = phenomeEvaluator;
    }

    public void Reset() => phenomeEvaluator.Reset();

    public void Evaluate(IList<TGenome> genomeList)
    {
        foreach(TGenome genome in genomeList)
        {
            var phenome = genomeDecoder.Decode(genome);
            if(phenome == null)
            {
                genome.EvaluationInfo.SetFitness(0.0);
                genome.EvaluationInfo.AuxFitnessArr = null;
            }
            else
            {
                FitnessInfo fitnessInfo = phenomeEvaluator.Evaluate(phenome);
                genome.EvaluationInfo.SetFitness(fitnessInfo._fitness);
                genome.EvaluationInfo.AuxFitnessArr = fitnessInfo._auxFitnessArr;
            }
        }
    }
}
