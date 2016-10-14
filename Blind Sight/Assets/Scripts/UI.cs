using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI : MonoBehaviour {

    float distance;
    Text displayText;

    public Image arrow;
    public Image cover;

    // Use this for initialization
    void Start () {
        displayText = GetComponentInChildren<Text>();
    }
	
	// Update is called once per frame
	void Update () {
    }

    public void Hit(float time) {
        //Debug.Log(time);
        distance = GetDistance(time);
        SetDisplay();
    }

    public void HitDistance(float d) {
        //Debug.Log(time);
        distance = d / .4f;
        //SetDisplay();
    }

    void SetDisplay() {
        displayText.text = "" + distance;
    }

    public void Direction(float dir) {
        arrow.transform.localEulerAngles = new Vector3(0,0,-dir);
    }

    int GetDistance(float t) {
        if (t < .075) return 1;
        else if (t >= .075 && t < .15) return 2;
        else if (t >= .15 && t < .225) return 3;
        else if (t >= .225 && t < .3) return 4;
        else if (t >= .3 && t < .375) return 5;
        else if (t >= .375 && t < .45) return 6;
        else if (t >= .45 && t < .525) return 7;
        else return 8;
    }

    public void Win() {
        if (cover.color != Color.clear) {
            cover.color = Color.clear;
        }
        else {
            cover.color = Color.black;
        }
    }
}
