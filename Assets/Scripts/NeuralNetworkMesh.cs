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

    public void GenerateMesh(NeatGenome genome)
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.Clear();

        var bottomLeft = Vector2.zero;

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> lineIndices = new List<int>();
        List<int> pointIndices = new List<int>();

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

        float width = maxDepth * horizontalSpacing;
        float height = mostNodesPerLayer * verticalSpacing;

        for(int depth = 0; depth < maxDepth; depth++)
        {
            var nodes = nodesByDepth[depth];
            float x = GetX(depth, maxDepth, width);
            for(int i = 0; i < nodes.Count; i++)
            {
                float y = GetY(i, nodes.Count);
                vertices.Add(CreateVertex(x, y));
            }
        }

        // Nodes
        for(int i = 0; i < genome.NodeList.Count; i++)
        {
            nodeIdxById.Add(genome.NodeList[i].Id, i);
            pointIndices.Add(i);
        }

        // Connections
        foreach(var connection in genome.ConnectionList)
        {
            lineIndices.Add(nodeIdxById[connection.SourceNodeId]);
            lineIndices.Add(nodeIdxById[connection.TargetNodeId]);
        }

        // Finalize mesh
        mesh.subMeshCount = 2;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(lineIndices.ToArray(), MeshTopology.Lines, 0);
        mesh.SetIndices(pointIndices.ToArray(), MeshTopology.Points, 1);
        meshFilter.mesh = mesh;
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
