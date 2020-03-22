using SharpNeat.Core;
using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XOREvaluator : IPhenomeEvaluator<IBlackBox>
{
    private const double StopFitness = 10.0;
    private ulong evalCount;
    private bool stopConditionSatisfied;

    public ulong EvaluationCount => evalCount;
    public bool StopConditionSatisfied => stopConditionSatisfied;

    public FitnessInfo Evaluate(IBlackBox phenome)
    {
        double fitness = 0;
        double output;
        double pass = 1.0;
        ISignalArray inputArr = phenome.InputSignalArray;
        ISignalArray outputArr = phenome.OutputSignalArray;

        evalCount++;            

        //----- Test 0,0
        phenome.ResetState();

        // Set the input values
        inputArr[0] = 0.0;
        inputArr[1] = 0.0;

        // Activate the black box.
        phenome.Activate();
        if(!phenome.IsStateValid) 
        {   // Any black box that gets itself into an invalid state is unlikely to be
            // any good, so lets just bail out here.
            return FitnessInfo.Zero;
        }

        // Read output signal.
        output = outputArr[0];
        Debug.Assert(output >= 0.0, "Unexpected negative output.");

        // Calculate this test case's contribution to the overall fitness score.
        //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
        fitness += 1.0-(output*output);
        if(output > 0.5) {
            pass = 0.0;
        }

    //----- Test 1,1
        // Reset any black box state from the previous test case.
        phenome.ResetState();

        // Set the input values
        inputArr[0] = 1.0;
        inputArr[1] = 1.0;

        // Activate the black box.
        phenome.Activate();
        if(!phenome.IsStateValid) 
        {   // Any black box that gets itself into an invalid state is unlikely to be
            // any good, so lets just bail out here.
            return FitnessInfo.Zero;
        }

        // Read output signal.
        output = outputArr[0];
        Debug.Assert(output >= 0.0, "Unexpected negative output.");

        // Calculate this test case's contribution to the overall fitness score.
        //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
        fitness += 1.0-(output*output);
        if(output > 0.5) {
            pass = 0.0;
        }

    //----- Test 0,1
        // Reset any black box state from the previous test case.
        phenome.ResetState();

        // Set the input values
        inputArr[0] = 0.0;
        inputArr[1] = 1.0;

        // Activate the black box.
        phenome.Activate();
        if(!phenome.IsStateValid) 
        {   // Any black box that gets itself into an invalid state is unlikely to be
            // any good, so lets just bail out here.
            return FitnessInfo.Zero;
        }

        // Read output signal.
        output = outputArr[0];
        Debug.Assert(output >= 0.0, "Unexpected negative output.");

        // Calculate this test case's contribution to the overall fitness score.
        // fitness += output; // Use this line to punish absolute error instead of squared error.
        fitness += 1.0-((1.0-output)*(1.0-output));
        if(output <= 0.5) {
            pass = 0.0;
        }

    //----- Test 1,0
        // Reset any black box state from the previous test case.
        phenome.ResetState();

        // Set the input values
        inputArr[0] = 1.0;
        inputArr[1] = 0.0;

        // Activate the black box.
        phenome.Activate();
        if(!phenome.IsStateValid) 
        {   // Any black box that gets itself into an invalid state is unlikely to be
            // any good, so lets just bail out here.
            return FitnessInfo.Zero;
        }

        // Read output signal.
        output = outputArr[0];
        Debug.Assert(output >= 0.0, "Unexpected negative output.");

        // Calculate this test case's contribution to the overall fitness score.
        // fitness += output; // Use this line to punish absolute error instead of squared error.
        fitness += 1.0-((1.0-output)*(1.0-output));
        if(output <= 0.5) {
            pass = 0.0;
        }

        // If all four outputs were correct, that is, all four were on the correct side of the
        // threshold level - then we add 10 to the fitness.
        fitness += pass * 10.0;

        if(fitness >= StopFitness) {
            stopConditionSatisfied = true;
        }

        return new FitnessInfo(fitness, fitness);
    }

    public void Reset()
    {
    }
}
