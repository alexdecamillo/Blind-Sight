using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    public LayerMask collisionMask;
    public float speed;

    float lifetime = 3;
    float skinWidth = .1f;

    float timeHit, timeStart;

    public EchoSphere echo;

    void Start() {
        //echo = FindObjectOfType<EchoSphere>();
        // destroys objects after lifetime seconds incase nothing is hit
        Destroy(gameObject, lifetime);

        timeStart = Time.time;
    }
    
    // moves object and then checks for collisions
    void Update() {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit) {
        timeHit = Time.time - timeStart;
        if (hit.transform.tag == "Goal") {
            //FindObjectOfType<UI>().Win();
        }
        //FindObjectOfType<UI>().Hit(timeHit);
        EchoSphere newEcho = Instantiate(echo, transform.position, transform.rotation) as EchoSphere;
        newEcho.TriggerPulse();
        GameObject.DestroyObject(gameObject, float.Epsilon);
    }

}
