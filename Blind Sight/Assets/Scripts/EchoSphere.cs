using UnityEngine;
using System;
using System.Collections;


public class EchoSphere : MonoBehaviour {
	// Echo sphere Properties
	public Material EchoMaterialSource;
    public Texture EchoTexture = null;
    public Shader shader;
    Material EchoMaterial;
	
	public float MaxRadius;		//Final size of the echo sphere.
	public float CurrentRadius = 0.0f;  //Current size of the echo sphere

    public float FadeDelay;           //Time to delay before triggering fade.
    public float FadeRate;          //Speed of the fade away
    public float EchoSpeed;			//Speed of the sphere growth.
	
	private bool isAnimated = false;	
	private float deltaTime = 0.0f;

    public float fade;
	public bool isTexturedScene = true;
	
	// Use this for initialization
	void Start () {
        GameObject.Destroy(gameObject, (FadeRate + FadeDelay));
        EchoMaterial = new Material(EchoMaterialSource);
        EchoMaterial.CopyPropertiesFromMaterial(EchoMaterialSource);
        EchoMaterial.shader = shader;
        Debug.Log(EchoMaterial.name + " set shader as " + EchoMaterial.shader.name);
		SetupSimpleScene2();
    }
	
	/// <summary>
	/// Scenario1: Monocolor echo. 
	/// </summary>
	void SetupSimpleScene1(){
		MaxRadius = 3.0f;
		CurrentRadius = 0.0f;
		FadeDelay = 0.1f;
		FadeRate = 1.0f;
		EchoSpeed = 5.0f;
		EchoMaterial.mainTexture = null;
		
		EchoMaterial.SetFloat("_DistanceFade",1.0f);
		isTexturedScene = true;
	}
	
	/// <summary>
	/// Scenario2: Diffuse texture echo
	/// </summary>
	void SetupSimpleScene2(){
        MaxRadius = 3.0f;
        CurrentRadius = 0.0f;
        FadeDelay = 0.1f;
        FadeRate = 1.0f;
        EchoSpeed = 5.0f;
        EchoMaterial.mainTexture = EchoTexture;
		
		EchoMaterial.SetFloat("_DistanceFade",1.0f);
		isTexturedScene = true;
	}
	/*
	void OnGUI () {
		// Make a background box
		GUI.Box(new Rect(10,10,100,90), "Scenarios");
		
		GUI.enabled = isTexturedScene;
		
		// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
		if(GUI.Button(new Rect(20,40,80,20), "No Texture")) {
			SetupSimpleScene1();
		}
		GUI.enabled = true;
		
		GUI.enabled = !isTexturedScene;
		// Make the second button.
		if(GUI.Button(new Rect(20,70,80,20), "Textured")) {
			SetupSimpleScene2();
		}
		GUI.enabled = true;
	}
    */
	// Update is called once per frame
	void Update () {
		deltaTime += Time.deltaTime;
		
		//UpdateRayCast();
		UpdateEcho();
		UpdateShader();
	}
	
	// Called to trigger an echo pulse
	public void TriggerPulse(){
		deltaTime = 0.0f;
		CurrentRadius = 0.0f;
		fade = 0.0f;
		isAnimated = true;
        Debug.Log("");
	}
	
	// Called to halt an echo pulse.
	void HaltPulse(){
		isAnimated = false;
        
	}
	
	void ClearPulse(){
		fade = 0.0f;
		CurrentRadius = 0.0f;
        GameObject.Destroy(gameObject, float.Epsilon);
        //isAnimated = false;
    }
	/*
	// Called to manually place echo pulse
	void UpdateRayCast() {
		if (Input.GetButtonDown("Fire1")){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		  	RaycastHit hit;
			if (Physics.Raycast(ray,out hit, Mathf.Infinity)) {
				transform.position = hit.point;
				TriggerPulse();
			}
		}  
	}
    */
	// Called to update the echo front edge
	void UpdateEcho(){
		//if(!isAnimated)return;
		
		if(CurrentRadius >= MaxRadius){
			HaltPulse();
		} else {
			CurrentRadius += Time.deltaTime * EchoSpeed;  
		}
	}
	
	// Called to update the actual shader values (some of which only change once but are included here
	// for illustrative purposes)
	void UpdateShader(){
		float radius = CurrentRadius;
		float maxRadius = MaxRadius;
		float maxFade = MaxRadius / EchoSpeed;
		
		if(deltaTime > FadeDelay)
			fade += Time.deltaTime * FadeRate;
		
		// Update our shader properties (requires Echo.shader)
		EchoMaterial.SetVector("_Position",transform.position);
		EchoMaterial.SetFloat("_Radius",radius);
		EchoMaterial.SetFloat("_MaxRadius",maxRadius);
		EchoMaterial.SetFloat("_Fade",fade);
		EchoMaterial.SetFloat("_MaxFade",maxFade);
	}
}
