using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeuralNetworkMesh : Graphic
{
    private void Start()
    {
        Mesh mesh = new Mesh();
        var canvasRenderer = GetComponent<CanvasRenderer>();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
