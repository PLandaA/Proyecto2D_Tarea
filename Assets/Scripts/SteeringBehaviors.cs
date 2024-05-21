
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviors
{
    int pathIndex = 0;
    bool reversePath = false;
    public void Seek(Vector2 targetPos, Transform agent, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 desiredVel = new Vector2(targetPos.x - agent.position.x, targetPos.y - agent.position.y);
        desiredVel = desiredVel.normalized * maxSpeed;

        Vector2 steering = new Vector2(desiredVel.x - rb.velocity.x, desiredVel.y - rb.velocity.y);
        steering = new Vector2(steering.x / rb.mass, steering.y / rb.mass);
        steering = Vector2.ClampMagnitude(steering, maxForce);

        rb.velocity += steering;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);

        //LookAt2D(targetPos, agent, rotateSpeed);
    }

    public void Flee(Vector2 target, Transform agent, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 desiredVel = new Vector2(agent.position.x - target.x ,  agent.position.y - target.y);
        desiredVel = desiredVel.normalized * maxSpeed;

        Vector2 steering = new Vector2(desiredVel.x - rb.velocity.x, desiredVel.y - rb.velocity.y);
        steering = new Vector2(steering.x / rb.mass, steering.y / rb.mass);
        steering = Vector2.ClampMagnitude(steering, maxForce);

        rb.velocity += steering;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);

        Vector3 targetPos = new Vector3(agent.position.x + desiredVel.x, agent.position.y + desiredVel.y, agent.position.z);
        //LookAt2D(targetPos, agent, rotateSpeed);
    }
    public void Arrival(Vector2 targetPos, Transform agent, float maxSpeed, float slowingRadius, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 desiredVel = new Vector2(targetPos.x - agent.position.x, targetPos.y - agent.position.y);
        desiredVel = desiredVel.normalized * maxSpeed;
        float distance = Vector2.Distance(targetPos, agent.transform.position);

        if(distance < slowingRadius)
        {
            desiredVel = desiredVel.normalized * maxSpeed * (distance / slowingRadius);
        }

        Vector2 steering = new Vector2(desiredVel.x - rb.velocity.x, desiredVel.y - rb.velocity.y);
        steering = new Vector2(steering.x / rb.mass, steering.y / rb.mass);
        steering = Vector2.ClampMagnitude(steering, maxForce);

        rb.velocity += steering;
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed);

        //LookAt2D(targetPos, agent, rotateSpeed);
    }
    public void Evade(GameObject target, Transform agent, float futureMag, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 targetCurrentVel = target.GetComponent<Rigidbody2D>().velocity;
        targetCurrentVel.Normalize();
        targetCurrentVel *= futureMag;
        targetCurrentVel.x += target.transform.position.x;
        targetCurrentVel.y += target.transform.position.y;
        Flee(targetCurrentVel, agent, maxSpeed, maxForce, rotateSpeed,rb);
    }
    public void Pursuit(GameObject target, Transform agent, float futureMag, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 targetCurrentVel = target.GetComponent<Rigidbody2D>().velocity;
        targetCurrentVel.Normalize();
        targetCurrentVel *= futureMag;
        targetCurrentVel.x += target.transform.position.x;
        targetCurrentVel.y += target.transform.position.y;
        Seek(targetCurrentVel, agent, maxSpeed, maxForce, rotateSpeed, rb);
    }
    public void Wander(GameObject agent, float displacement, float radio, float wanderRange, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 Wheel = new Vector2((agent.GetComponent<Rigidbody2D>().velocity.normalized.x * displacement) + agent.transform.position.x, (agent.GetComponent<Rigidbody2D>().velocity.normalized.y * displacement) + agent.transform.position.y);
        Vector2 rndDiR = new Vector2(Random.Range(-wanderRange, wanderRange), Random.Range(-wanderRange, wanderRange)).normalized * radio;
        rndDiR += Wheel;
       Seek(rndDiR, agent.transform, maxSpeed,maxForce, rotateSpeed,rb);
    }
    public void QueueSB(Vector2 path,GameObject agent,LayerMask layer,float distLeader, float maxSpeed, float slowingRadius, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        GameObject target = new GameObject();
        Vector2 posLeader = agent.transform.up * distLeader;
        posLeader = new Vector2(agent.transform.position.x + posLeader.x, agent.transform.position.y + posLeader.y);
        Collider2D[] haveLeader = Physics2D.OverlapCircleAll(posLeader, 1.0f, layer);
        List<Collider2D> list = new List<Collider2D>(haveLeader);
        for (int i = 0; i < list.Count; ++i)
        {
            GameObject vecino = list[i].gameObject;
            if (vecino != agent && Vector2.Distance(posLeader, vecino.transform.position) <= 1.0f)
            {
                target = vecino;
            }
            else
            {
                target = null;
            }
        }
        if (target != null){
           Arrival(new Vector2(target.transform.position.x - 1.0f, target.transform.position.y - 1.0f),agent.transform,maxSpeed,slowingRadius,maxForce, rotateSpeed, rb);
        }
        else{
            Arrival(path,agent.transform,maxSpeed, slowingRadius,maxForce, rotateSpeed, rb);
        }
    }
    public void PathFollowing(Vector2[] pathPoints, GameObject agent, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        
        
        Vector2 currentPointInPath = pathPoints[pathIndex];

        Seek(currentPointInPath, agent.transform, maxSpeed, maxForce, rotateSpeed, rb);

        if (Vector3.Distance(agent.transform.position, currentPointInPath) < 1)
        {
            if (reversePath)
            {
                if (pathIndex == 0)
                {
                    pathIndex = 0;
                    reversePath = false;
                }
                else
                {
                    pathIndex--;
                }
            }

            else
            {
                if (pathIndex == pathPoints.Length - 1)
                {
                    pathIndex = pathPoints.Length - 1;
                    reversePath = true;
                }
                else
                {
                    pathIndex++;
                }
            }
        }
    }

    //Vector2 targetPos, Transform agent, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb
    public void ObstacleAvoidance(float futureSight,GameObject agent, float maxSpeed, float maxForce, float rotateSpeed, float futureSightRadius,Rigidbody2D rb, LayerMask whatIsWall)
    {
        Vector2 aheadSight = (rb.velocity.normalized * futureSight) + (Vector2)agent.transform.position;
        Vector2 playerFutureSight = aheadSight - (Vector2)agent.transform.position;
        Vector2 firstNormal = Vector2.Perpendicular(playerFutureSight).normalized * 2;
        Vector2 secondNormal = Vector2.Perpendicular(-playerFutureSight).normalized * 2;
        firstNormal += aheadSight;
        secondNormal += aheadSight;
        Debug.DrawLine(aheadSight, firstNormal);
        Debug.DrawLine(aheadSight, secondNormal);

        bool Obstacle = Physics2D.OverlapCircle(aheadSight, futureSightRadius, whatIsWall);
        if (Obstacle)
        {
            GameObject wall = WallAhead(aheadSight, futureSightRadius, whatIsWall);
            if (wall != null)
            {
                Vector2 firstNormalDistance = (Vector2)wall.transform.position - firstNormal;
                Vector2 secondNormalDistance = (Vector2)wall.transform.position - secondNormal;
                Vector2 playerDistanceWithObject = (Vector2)wall.transform.position - aheadSight;
                Debug.DrawLine(firstNormal, wall.transform.position);
                Debug.DrawLine(secondNormal, wall.transform.position);
                Debug.DrawLine(aheadSight, wall.transform.position);


                if (firstNormalDistance.magnitude > secondNormalDistance.magnitude)
                {
                    if (playerDistanceWithObject.magnitude <= secondNormalDistance.magnitude)
                    {
                        Seek(secondNormal,agent.transform,maxSpeed,maxForce,rotateSpeed,rb);
                    }
                    else
                    {
                        Seek(firstNormal, agent.transform, maxSpeed, maxForce, rotateSpeed, rb);
                    }
                }
                else
                {
                    if (playerDistanceWithObject.magnitude <= firstNormalDistance.magnitude)
                    {
                        Seek(firstNormal,agent.transform, maxSpeed, maxForce, rotateSpeed, rb);
                    }
                    else
                    {
                        Seek(secondNormal, agent.transform, maxSpeed, maxForce, rotateSpeed, rb);
                    }

                }

            }
        }
    }
    private GameObject WallAhead(Vector2 aheadSight, float futureSightRadius, LayerMask whatIsWall)
    {
        try
        {
            return Physics2D.OverlapCircle(aheadSight, futureSightRadius, whatIsWall).gameObject;
        }
        catch
        {
            return null;
        }
    }

    public void LeaderFollowing(GameObject agent, GameObject leader, List<GameObject> followers, float distanceLeader, float maxSpeed, float maxForce, float rotateSpeed, Rigidbody2D rb)
    {
        Vector2 distanceFromLeader = (leader.GetComponent<Rigidbody2D>().velocity.normalized * -1) * distanceLeader;
        distanceFromLeader += (Vector2)leader.transform.position;

        Debug.DrawLine(leader.transform.position, distanceFromLeader);
        Vector2 toLeaderDistance = (Vector2)leader.transform.position - distanceFromLeader;
        Vector2 normalNeightbrs = Vector2.Perpendicular(toLeaderDistance).normalized;
        if (followers.Contains(agent))
        {
            ushort temp = 1;
            for (int i = 1; i < followers.Count + 1; i++)
            {
                if (followers[i - 1] == agent)
                {
                    normalNeightbrs *= (distanceLeader * temp);
                    Seek(normalNeightbrs + distanceFromLeader, agent.transform, maxSpeed, maxForce, rotateSpeed, rb);
                    Debug.DrawLine(distanceFromLeader, normalNeightbrs + distanceFromLeader);
                }

                normalNeightbrs *= -1;
                if (i % 2 == 0) 
                temp++;
                }
            }   
       else
       {
        followers.Add(agent);
       }

    }
    private void LookAt2D(Vector3 p_targetPositiom, Transform agent,float rotateSpeed)
    {
        agent.rotation = Quaternion.RotateTowards(agent.rotation, Quaternion.LookRotation(Vector3.forward, p_targetPositiom),rotateSpeed);
    }

}
