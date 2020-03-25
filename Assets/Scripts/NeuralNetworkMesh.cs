using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkMesh : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 10.0f)] private float verticalSpacing = 1.0f;
    [SerializeField] [Range(0.0f, 10.0f)] private float horizontalSpacing = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] private float verticalPerturb = 0.25f;

    [SerializeField] private GameObject neuronObject;
    [SerializeField] private GameObject connectionObject;

    public void GenerateMesh(NeatGenome genome)
    {
        // Perform analysis
        AcyclicNetworkDepthAnalysis depthAnalysis = new AcyclicNetworkDepthAnalysis();
        NetworkDepthInfo depthInfo = depthAnalysis.CalculateNodeDepths(genome);
        int maxDepth = depthInfo._networkDepth;
        int[] nodesPerDepthLevel = new int[maxDepth];
        Dictionary<int, List<int>> nodesByDepth = new Dictionary<int, List<int>>(); 
        Dictionary<uint, int> nodeIdxById = new Dictionary<uint, int>(); 

        int mostNodesPerLayer = 0;
        for(int i = 0; i < genome.NodeList.Count; i++)
        {
            int depth = depthInfo._nodeDepthArr[i];
            List<int> nodes;
            if (!nodesByDepth.TryGetValue(depth, out nodes))
            {
                nodes = new List<int>();
                nodesByDepth.Add(depth, nodes);
            }
            nodes.Add(i);
            mostNodesPerLayer = Mathf.Max(++nodesPerDepthLevel[depth], mostNodesPerLayer);
        }

        List<Vector3> vertices;
        var neuronsMesh = GenerateNeuronsMesh(genome, nodeIdxById, maxDepth, mostNodesPerLayer, nodesByDepth, out vertices);
        neuronObject.GetComponent<MeshFilter>().mesh = neuronsMesh;

        var connectionMesh = GenerateConnectionsMesh(genome, nodeIdxById, vertices);
        connectionObject.GetComponent<MeshFilter>().mesh = connectionMesh;
    }

    private Mesh GenerateNeuronsMesh(
        NeatGenome genome, 
        Dictionary<uint, int> nodeIdxById, 
        int maxDepth, 
        int mostNodesPerLayer, 
        Dictionary<int, List<int>> nodesByDepth, 
        out List<Vector3> vertices
    )
    {
        Mesh mesh = new Mesh();

        vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> pointIndices = new List<int>();

        // Nodes
        for(int i = 0; i < genome.NodeList.Count; i++)
        {
            nodeIdxById.Add(genome.NodeList[i].Id, i);
            pointIndices.Add(i);
        }

        float width = maxDepth * horizontalSpacing;
        float height = mostNodesPerLayer * verticalSpacing;

        for(int depth = 0; depth < maxDepth; depth++)
        {
            var nodes = nodesByDepth[depth];
            float x = GetX(depth, maxDepth, width);
            for(int i = 0; i < nodes.Count; i++)
            {
                float y = GetY(i, nodes.Count);
                y += Random.Range(-verticalPerturb, verticalPerturb);
                vertices.Add(CreateVertex(x, y));
            }
        }

        // Finalize mesh
        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(pointIndices.ToArray(), MeshTopology.Points, 0);

        return mesh;
    }

    private Mesh GenerateConnectionsMesh(NeatGenome genome, Dictionary<uint, int> nodeIdxById, List<Vector3> nodeVertices)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> pointIndices = new List<int>();

        // Connections
        int idx = 0;
        foreach(var connection in genome.ConnectionList)
        {
            int sourceIdx = nodeIdxById[connection.SourceNodeId];
            int targetIdx = nodeIdxById[connection.TargetNodeId];

            var sourcePos = nodeVertices[sourceIdx];
            var targetPos = nodeVertices[targetIdx];

            vertices.Add(new Vector3((float)connection.Weight, 0, 0));
            colors.Add(new Color(sourcePos.x, sourcePos.y, targetPos.x, targetPos.y));
            pointIndices.Add(idx++);

            //Debug.Log($"[{genome.Id}] {connection.SourceNodeId} - {connection.TargetNodeId}: {connection.Weight}");
        }

        mesh.subMeshCount = 1;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(pointIndices.ToArray(), MeshTopology.Points, 0);
        return mesh;
    }

    private float GetX(int depth, int maxDepth, float width)
    {
        var depthPct = (float)(depth + 1) / (float)(maxDepth + 1);
        return width * (depthPct - 0.5f);
    }

    private float GetY(int idx, int layerSize)
    {
        var heightPct = (float)(idx + 1) / (float)(layerSize + 1);
        var layerHeight = layerSize * verticalSpacing;
        var heightInLayer = heightPct * layerHeight;
        return heightInLayer - layerHeight * 0.5f;
    }

    private Color GetGrey(float t)
    {
        return Color.Lerp(Color.white, Color.black, t);
    }

    private Vector3 CreateVertex(float x, float y)
    {
        return new Vector3(x, y, 0);
    }
}
