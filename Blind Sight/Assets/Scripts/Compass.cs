using UnityEngine;
using System.Collections;

public class Compass : MonoBehaviour {

    const float MAXcompass = 355;
    const float MINcompass = -355;
    //const float DEFAULTcompass = 0;

    float _direction;

    Player player;

    // north = 355
    // east = 185
    // south = 5
    // west = -180

    void Start () {
        player = FindObjectOfType<Player>();
	}
	
	void Update () {
        _direction = (-player.facingDir * 1.972f) + 355;
        SetDirection(_direction);
    }

    void SetDirection(float direction) {
        transform.localPosition = new Vector3(direction, transform.localPosition.y, transform.localPosition.z);
    }

}
