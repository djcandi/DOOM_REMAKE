using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FormerHuman : MonoBehaviour
{
    public float viewRadius;
    public float viewAngle;
    public float range;

    public LayerMask targetPlayer;
    public LayerMask obstacleMask;

    public GameObject player;

    public AudioSource disparoSFX;

    UnityEngine.AI.NavMeshAgent pathfinder;

    Animator animator;

    public float timeBetweenAttacks;
    private float tiempoDisparo = 1f;
    bool alreadyAttacked = false; 
    public GameObject projectile;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        pathfinder = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (detectado() && !enRango())
            perseguir();
        else if (detectado() && enRango())
            if (!alreadyAttacked)
                atacar();
            else 
                Invoke(nameof(esquivar),tiempoDisparo);
    }

    private bool detectado()
    {
        Vector3 playerTarget = (player.transform.position - transform.position).normalized;
        bool detected = false;

        if (Vector3.Angle(transform.forward, playerTarget) < viewAngle / 2)
        {
            float distanceToTarget = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToTarget <= viewRadius)
            {
                if (Physics.Raycast(transform.position, playerTarget, distanceToTarget, obstacleMask) == false)
                {
                    //Debug.Log("Look at me Hector");
                    detected = true;
                }
            }
        }
        return detected;
    }

    private void perseguir() 
    {
        pathfinder.SetDestination(player.transform.position);
        animator.SetBool("Run",true);        
    }

    private bool enRango()
    {
        float distance = Vector3.Distance(player.transform.position,this.transform.position);
        Vector3 playerTarget = (player.transform.position - transform.position).normalized;

        return distance < range /*&& Physics.Raycast(transform.position, playerTarget, distance, obstacleMask) == false*/ ;
    }

    private void atacar()
    {
        //Make sure enemy doesn't move
        pathfinder.SetDestination(transform.position);
        animator.SetBool("Run",false);

        transform.LookAt(player.transform.position);

        if (!alreadyAttacked)
        {
            animator.SetBool("Disparo", true);
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            disparoSFX.Play();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        Debug.Log("juuju");
        alreadyAttacked = false;
    }

    private void esquivar()
    {
        Vector3 point;

        if (RandomPoint(player.transform.position, range, out point)) //pass in our centre point and radius of area
        {
            animator.SetBool("Run", true);
            animator.SetBool("Disparo",false);
            //transform.LookAt(player.transform.position);
            pathfinder.SetDestination(point);
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * (range - 3f); //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) 
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
