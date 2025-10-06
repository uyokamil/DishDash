// ================================== //
// COPYRIGHT (c) 2024 Kamil Czarnecki //
// ================================== //

using System.Collections;
using UnityEngine;

public class ParticleSelfDestroy : MonoBehaviour
{
    /// <summary>
    /// Particles will destroy themselves after a set duration.
    /// </summary>

    [SerializeField] private float particleDuration = 1f;

    void Start()
    {
        StartCoroutine(DestroySelf());
    }

    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(particleDuration);
        Destroy(gameObject);
    }
}
