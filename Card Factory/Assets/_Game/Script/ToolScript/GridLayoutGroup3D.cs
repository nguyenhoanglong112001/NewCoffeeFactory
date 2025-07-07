using UnityEngine;

[System.Serializable]
public enum GridAxis
{
    Horizontal,
    Vertical
}

[System.Serializable]
public enum GridConstraint
{
    Flexible,
    FixedColumnCount,
    FixedRowCount
}

public class GridLayoutGroup3D : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 cellSize = Vector3.one;
    public Vector3 spacing = Vector3.zero;

    [Header("Grid Constraint")]
    public GridConstraint constraint = GridConstraint.Flexible;
    public int constraintCount = 1;

    [Header("Start Axis")]
    public GridAxis startAxis = GridAxis.Horizontal;

    [Header("Alignment")]
    public TextAnchor childAlignment = TextAnchor.UpperLeft;

    [Header("Auto Update")]
    public bool autoUpdate = true;

    private Vector3 m_LastPosition;
    private int m_LastChildCount;

    void Start()
    {
        ArrangeChildren();
        m_LastPosition = transform.position;
        m_LastChildCount = transform.childCount;
    }

    void Update()
    {
        if (autoUpdate)
        {
            if (transform.position != m_LastPosition || transform.childCount != m_LastChildCount)
            {
                ArrangeChildren();
                m_LastPosition = transform.position;
                m_LastChildCount = transform.childCount;
            }
        }
    }

    [ContextMenu("Arrange Children")]
    public void ArrangeChildren()
    {
        if (transform.childCount == 0) return;

        int childCount = transform.childCount;
        int columnCount, rowCount;

        // Calculate grid dimensions
        CalculateGridSize(childCount, out columnCount, out rowCount);

        // Calculate starting position based on alignment
        Vector3 startPos = CalculateStartPosition(columnCount, rowCount);

        // Arrange children
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;

            Vector3 position = CalculateChildPosition(i, columnCount, rowCount, startPos);
            child.localPosition = position;
        }
    }

    private void CalculateGridSize(int childCount, out int columnCount, out int rowCount)
    {
        switch (constraint)
        {
            case GridConstraint.FixedColumnCount:
                columnCount = constraintCount;
                rowCount = Mathf.CeilToInt((float)childCount / columnCount);
                break;
            case GridConstraint.FixedRowCount:
                rowCount = constraintCount;
                columnCount = Mathf.CeilToInt((float)childCount / rowCount);
                break;
            default: // Flexible
                columnCount = Mathf.CeilToInt(Mathf.Sqrt(childCount));
                rowCount = Mathf.CeilToInt((float)childCount / columnCount);
                break;
        }
    }

    private Vector3 CalculateStartPosition(int columnCount, int rowCount)
    {
        Vector3 totalSize = new Vector3(
            (columnCount - 1) * (cellSize.x + spacing.x),
            (rowCount - 1) * (cellSize.y + spacing.y),
            0
        );

        Vector3 startPos = Vector3.zero;

        // Calculate alignment offset
        switch (childAlignment)
        {
            case TextAnchor.UpperLeft:
                startPos = Vector3.zero;
                break;
            case TextAnchor.UpperCenter:
                startPos = new Vector3(-totalSize.x * 0.5f, 0, 0);
                break;
            case TextAnchor.UpperRight:
                startPos = new Vector3(-totalSize.x, 0, 0);
                break;
            case TextAnchor.MiddleLeft:
                startPos = new Vector3(0, -totalSize.y * 0.5f, 0);
                break;
            case TextAnchor.MiddleCenter:
                startPos = new Vector3(-totalSize.x * 0.5f, -totalSize.y * 0.5f, 0);
                break;
            case TextAnchor.MiddleRight:
                startPos = new Vector3(-totalSize.x, -totalSize.y * 0.5f, 0);
                break;
            case TextAnchor.LowerLeft:
                startPos = new Vector3(0, -totalSize.y, 0);
                break;
            case TextAnchor.LowerCenter:
                startPos = new Vector3(-totalSize.x * 0.5f, -totalSize.y, 0);
                break;
            case TextAnchor.LowerRight:
                startPos = new Vector3(-totalSize.x, -totalSize.y, 0);
                break;
        }

        return startPos;
    }

    private Vector3 CalculateChildPosition(int index, int columnCount, int rowCount, Vector3 startPos)
    {
        int x, y;

        if (startAxis == GridAxis.Horizontal)
        {
            x = index % columnCount;
            y = index / columnCount;
        }
        else
        {
            x = index / rowCount;
            y = index % rowCount;
        }

        return startPos + new Vector3(
            x * (cellSize.x + spacing.x),
            -y * (cellSize.y + spacing.y), // Negative Y for top-to-bottom
            0
        );
    }

    // Gizmos để visualize grid trong Scene view
    void OnDrawGizmosSelected()
    {
        if (transform.childCount == 0) return;

        int childCount = transform.childCount;
        int columnCount, rowCount;
        CalculateGridSize(childCount, out columnCount, out rowCount);
        Vector3 startPos = CalculateStartPosition(columnCount, rowCount);

        Gizmos.color = Color.yellow;

        for (int i = 0; i < childCount; i++)
        {
            Vector3 position = transform.position + CalculateChildPosition(i, columnCount, rowCount, startPos);
            Gizmos.DrawWireCube(position, cellSize);
        }
    }
}