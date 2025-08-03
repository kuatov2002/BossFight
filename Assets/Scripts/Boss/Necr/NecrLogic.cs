using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class NecromancerBoss : MonoBehaviour
{
    [Header("Цели и параметры")]
    public Transform player;
    public float meleeRange = 2f;
    public float detectionRange = 15f;
    public float actionCooldown = 3f;
    public Animator animator;
    
    [Header("Зоны обзора")]
    [Range(0, 360)]
    public float frontAngle = 90f;

    [Header("Призыв")] 
    public List<Transform> pointsForSummon;
    public GameObject zombiePrefab;
    
    private bool canAct = true;
    private Color gizmoColor = Color.white;
    private float lastActionTime = 0f;

    private void Start()
    {
        BossActions.onBossDied += Die;
    }

    private void Die()
    {
        BossActions.onBossDied -= Die;
    }

    void Update()
    {
        if (player == null) return;

        // Обновляем цвет гизмо в каждом кадре на основе текущего состояния
        UpdateGizmoColor();

        if (canAct && Time.time - lastActionTime >= actionCooldown)
        {
            StartCoroutine(PerformAction());
        }
    }

    void UpdateGizmoColor()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isPlayerInFront = IsPlayerInFront();

        // Меняем цвет в зависимости от текущего состояния (не по таймеру)
        if (distanceToPlayer <= meleeRange && !isPlayerInFront)
        {
            gizmoColor = Color.magenta; // Игрок близко, но не виден
        }
        else if (isPlayerInFront && distanceToPlayer <= meleeRange)
        {
            gizmoColor = Color.red; // Атака в ближнем бою
        }
        else if (distanceToPlayer > meleeRange && distanceToPlayer <= detectionRange)
        {
            gizmoColor = Color.yellow; // В зоне обнаружения
        }
        else
        {
            gizmoColor = Color.white; // По умолчанию
        }
    }

    IEnumerator PerformAction()
    {
        canAct = false;
        lastActionTime = Time.time;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isPlayerInFront = IsPlayerInFront();

        // Новое условие: игрок близко, но не в поле зрения
        if (distanceToPlayer <= meleeRange && !isPlayerInFront)
        {
            RetreatFromPlayer();
        }
        else
        {
            int action = Random.Range(0, 3);

            switch (action)
            {
                case 0:
                    if (isPlayerInFront && distanceToPlayer <= meleeRange)
                    {
                        MeleeAttack();
                    }
                    else
                    {
                        if (distanceToPlayer > meleeRange && distanceToPlayer <= detectionRange)
                            SummonCreatures();
                        else
                            CastFireball();
                    }
                    break;

                case 1:
                    if (distanceToPlayer > meleeRange && distanceToPlayer <= detectionRange)
                    {
                        SummonCreatures();
                    }
                    else
                    {
                        if (isPlayerInFront && distanceToPlayer <= meleeRange)
                            MeleeAttack();
                        else
                            CastFireball();
                    }
                    break;

                case 2:
                    CastFireball();
                    break;
            }
        }

        yield return new WaitForSeconds(actionCooldown);
        canAct = true;
    }

    bool IsPlayerInFront()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < frontAngle * 0.5f;
    }

    void RetreatFromPlayer()
    {
        Debug.Log("Некромант уходит от игрока! (игрок близко, но не в поле зрения)");
    }

    void MeleeAttack()
    {
        Debug.Log("Некромант ударил в ближнем бою!");
    }

    void CastFireball()
    {
        Debug.Log("Некромант кастует фаербол!");
    }

    void SummonCreatures()
    {
        animator.SetTrigger("Summon");
        foreach (var point in pointsForSummon)
        {
            var zombie = Instantiate(zombiePrefab, point.position, Quaternion.identity);
            zombie.GetComponent<Zombie>().player = player;
        }
        Debug.Log("Некромант призывает существ!");
    }

    // ГИЗМОСЫ
    void OnDrawGizmos()
    {
        // Используем цвет, установленный в Update
        Gizmos.color = gizmoColor;

        // Отрисовка зоны обнаружения
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Отрисовка зоны ближнего боя
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        // Отрисовка конуса обзора
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        if (frontAngle <= 0) return;

        Vector3 forward = transform.forward;
        Vector3 forwardLeft = Quaternion.Euler(0, -frontAngle * 0.5f, 0) * forward;
        Vector3 forwardRight = Quaternion.Euler(0, frontAngle * 0.5f, 0) * forward;

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f); // Полупрозрачный текущий цвет

        Gizmos.DrawLine(transform.position, transform.position + forwardLeft * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + forwardRight * detectionRange);

        Vector3 previousPoint = transform.position + forwardLeft * detectionRange;
        int steps = 20;
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            float angle = Mathf.Lerp(-frontAngle * 0.5f, frontAngle * 0.5f, t);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = transform.position + direction * detectionRange;
            
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(previousPoint, transform.position + forwardLeft * detectionRange);
    }

    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, player.position);
            
            bool inFront = IsPlayerInFront();
            float distance = Vector3.Distance(transform.position, player.position);
            
            if (inFront && distance <= meleeRange)
                Gizmos.color = Color.red;
            else if (distance <= detectionRange)
                Gizmos.color = Color.yellow;
            else if (distance <= meleeRange && !inFront)
                Gizmos.color = new Color(1, 0.5f, 0);
            else
                Gizmos.color = Color.gray;
                
            Gizmos.DrawSphere(player.position, 0.5f);
        }
    }
}