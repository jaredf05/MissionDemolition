using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof(Renderer))]
public class Goal : MonoBehaviour
{
    // A static field accesible by code anywhere
    static public bool goalMet = false;


    void OnTriggerEnter(Collider other)
    {
        // When the trigger is hit by something, check to see if it's a Projectile
        Projectile proj = other.GetComponent<Projectile>();
        if (proj != null)
        {
            // If so, set goalMet to true
            Goal.goalMet = true;
            // ALso set the alpha of the color to hgiher opacity
            Material mat = GetComponent<Renderer>().material;
            Color c = mat.color;
            c.a = 0.75f;
            mat.color = c;
        }
    }
}
