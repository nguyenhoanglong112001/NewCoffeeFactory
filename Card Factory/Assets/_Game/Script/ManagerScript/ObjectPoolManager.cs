using Lean.Pool;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("===Pool===")]
    public LeanGameObjectPool poolPopup;
    public LeanGameObjectPool objectPool;
    public LeanGameObjectPool vfxPool;
    public LeanGameObjectPool cardPool;
    public LeanGameObjectPool cardGroupPool;
    public LeanGameObjectPool packPool;
}
