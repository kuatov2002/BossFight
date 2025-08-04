using System;
using UnityEngine;

public class FireFloor : MonoBehaviour
{
    public float damage;
    public PlayerHealth health;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            UIManager.Instance.RestartLevel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance.RestartLevel();
        }
    }
}
