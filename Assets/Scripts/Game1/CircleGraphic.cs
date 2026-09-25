using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CircleGraphic : MaskableGraphic
{
    [SerializeField] private int segments = 64;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        float radiusX = width * 0.5f;
        float radiusY = height * 0.5f;

        UIVertex centerVertex = UIVertex.simpleVert;
        centerVertex.color = color;
        centerVertex.position = Vector2.zero;
        vh.AddVert(centerVertex);

        for (int i = 0; i <= segments; i++)
        {
            float angle = (Mathf.PI * 2f * i) / segments;

            float x = Mathf.Cos(angle) * radiusX;
            float y = Mathf.Sin(angle) * radiusY;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.position = new Vector2(x, y);

            vh.AddVert(vertex);
        }

        for (int i = 0; i < segments; i++)
        {
            vh.AddTriangle(0, i + 1, i + 2);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
    }
}