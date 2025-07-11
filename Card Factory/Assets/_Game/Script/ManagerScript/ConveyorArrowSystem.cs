using UnityEngine;
using System.Collections.Generic;
using Dreamteck.Splines;
using Sirenix.OdinInspector;

public class ConveyorArrowSystem : MonoBehaviour
{
    [Header("References")]
    public SplineComputer splineComputer;
    public GameObject prefab;

    [Header("Spawn Settings")]
    public float spacing = 2f;
    public bool alignRotation = true;
    public Vector3 rotationOffset;

#if UNITY_EDITOR
    [Button("Spawn Objects")]
#endif
    void SpawnObjects()
    {
        if (splineComputer == null || prefab == null)
        {
            Debug.LogWarning("Spline ho?c Prefab ch?a ???c gán.");
            return;
        }

        float splineLength = splineComputer.CalculateLength();
        int objectCount = Mathf.FloorToInt(splineLength / spacing);

        for (int i = 0; i <= objectCount; i++)
        {
            float distance = i * spacing;
            double percent = splineComputer.Travel(0.0, distance, Spline.Direction.Forward);

            SplineSample sample = new SplineSample();
            splineComputer.Evaluate(percent, ref sample);

            Vector3 position = sample.position;
            Quaternion rotation = Quaternion.identity;

            if (alignRotation)
            {
                rotation = sample.rotation * Quaternion.Euler(rotationOffset);
            }

            GameObject arrow = Instantiate(prefab, position, rotation, this.transform);

            // ? Gán spline và percent cho SplineFollower (n?u có)
            SplineFollower follower = arrow.GetComponent<SplineFollower>();
            if (follower != null)
            {
                follower.spline = splineComputer;
                follower.SetPercent(percent); // Gán v? trí b?t ??u
                follower.follow = true;       // B?t ??u di chuy?n n?u mu?n
            }
        }
    }
}