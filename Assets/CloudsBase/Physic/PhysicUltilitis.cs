using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicUltilitis 
{
    public static Vector3 ThroughForcebyAngel(Vector3 beginposition,Vector3 targetdestination,Rigidbody2D rb,float angel) {
        Vector3 force =  new Vector3();
        float Dx = (targetdestination.x - beginposition.x);
        float Dy = (targetdestination.y - beginposition.y);
        float m = rb.mass;
        float g = rb.gravityScale * Mathf.Abs(Physics.gravity.y);
        float time = Mathf.Sqrt(2*(Mathf.Tan(angel * Mathf.Deg2Rad) * Dx - Dy)/g);
        float fx = Dx * m/time;
        float fy = Mathf.Tan(angel* Mathf.Deg2Rad) * fx + g * m * Time.deltaTime; // Gravity Pressure is added with general forcemode. * Time.deltatime to calulate the pressure for impluse
        force.x = fx;
        force.y = fy;
        return force;
    }
    public static Vector3 ThroughForcebyTime(Vector3 beginposition,Vector3 targetdestination,Rigidbody2D rb,float time) {
        Vector3 force =  new Vector3();
        float g = rb.gravityScale * Mathf.Abs(Physics.gravity.y);
        float m = rb.mass;
        float Dx = (targetdestination.x - beginposition.x);
        float Dy = (targetdestination.y - beginposition.y);
        float vx = Dx/time;
        float vy = (Dy + g* Mathf.Pow(time,2)/2)/time;
        float fx = vx * m;
        float fy = vy * m + m*g * Time.deltaTime; //Gravity Pressure is added with general forcemode. * Time.deltatime to calulate the pressure for impluse
        force.x = fx;
        force.y = fy;
        return force;

    }
    public static Vector2 SpringJointForce(SpringJoint2D springJoint,Rigidbody2D connectedBody,Transform baseobj) {
        float frequency = springJoint.frequency;
        float dampingRatio = springJoint.dampingRatio;
        float mass = springJoint.connectedBody != null ? connectedBody.mass : 1f; // Assume mass is 1 if no connected body
        
        // Calculate the spring constant k
        float k = Mathf.Pow(2 * Mathf.PI * frequency, 2) * mass;

        // Calculate the damping coefficient b
        float b = 2 * mass * dampingRatio * Mathf.Sqrt(k);

        // Calculate the current displacement (difference from target distance)
        Vector2 displacement = (Vector2)baseobj.transform.position - springJoint.connectedAnchor;
        float displacementMagnitude = displacement.magnitude - springJoint.distance;

        // Calculate relative velocity
        Vector2 relativeVelocity = connectedBody.linearVelocity - baseobj.GetComponent<Rigidbody2D>().linearVelocity;

        // Calculate the spring force
        Vector2 springForce = -k * displacementMagnitude * displacement.normalized - b * relativeVelocity;
        return springForce;
    }
}
