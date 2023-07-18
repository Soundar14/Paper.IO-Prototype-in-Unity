using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArea : MonoBehaviour
{
    public Player player;
    public MeshCollider playerMeshCollider;

    private void Awake()
    {
        playerMeshCollider = gameObject.AddComponent<MeshCollider>();
    }
}
