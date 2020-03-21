using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SharpNeat.Neat.Genome;
using SharpNeat.Neat.Genome.Double;
using SharpNeat.Neat.Genome.IO;
using SharpNeat.NeuralNet;
using SharpNeat.Tasks.BinaryThreeMultiplexer;

namespace SharpNeat.Tasks.Tests
{
    [TestClass]
    public class BinaryThreeMultiplexerTests
    {
        [TestMethod]
        [TestCategory("BinaryThreeMultiplexer")]
        public void TestCorrectResponse()
        {
            TestCorrectResponseInner(false);
        }

        [TestMethod]
        [TestCategory("BinaryThreeMultiplexer")]
        public void TestCorrectResponse_EnableHardwareAcceleration()
        {
            TestCorrectResponseInner(true);
        }

        private void TestCorrectResponseInner(bool enableHardwareAcceleration)
        {
            var activationFnFactory = new DefaultActivationFunctionFactory<double>(enableHardwareAcceleration);
            var metaNeatGenome = new MetaNeatGenome<double>(4, 1, true, activationFnFactory.GetActivationFunction("LeakyReLU"));

            // Load test genome.
            NeatGenomeLoader<double> loader = NeatGenomeLoaderFactory.CreateLoaderDouble(metaNeatGenome);
            NeatGenome<double> genome = loader.Load("TestData/binary-three-multiplexer.genome");

            // Decode genome to a neural net.
            var genomeDecoder = NeatGenomeDecoderFactory.CreateGenomeDecoderAcyclic();
            IBlackBox<double> blackBox = genomeDecoder.Decode(genome);

            // Evaluate the neural net.
            var evaluator = new BinaryThreeMultiplexerEvaluator();

            // Confirm the expected fitness (to a limited amount of precision to allow for small variations of floating point
            // results that can occur as a result of platform/environmental variations).
            FitnessInfo fitnessInfo = evaluator.Evaluate(blackBox);
            Assert.AreEqual(107.50554956432657, fitnessInfo.PrimaryFitness, 0.000001);
        }
    }
}
