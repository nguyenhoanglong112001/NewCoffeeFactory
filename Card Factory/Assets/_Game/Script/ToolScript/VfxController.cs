using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxController : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        GameManager.Ins.poolManager.vfxPool.Despawn(gameObject);
    }
}
