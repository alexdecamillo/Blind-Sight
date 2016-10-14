using System;
using UnityEngine;
using System.Collections;

public class Click : MonoBehaviour {

    public Transform muzzle;
    public Projectile projectile;
    float msBetweenShots = 2;

    float nextShotTime;

    public void Shoot() {

        if (Time.time > nextShotTime) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
        }
    }
}
