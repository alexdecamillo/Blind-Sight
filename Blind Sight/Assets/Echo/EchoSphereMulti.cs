using UnityEngine;
using System;
using System.Collections;


public class EchoSphereMulti : MonoBehaviour {
	public enum ShaderPackingMode { Texture, Property };
	public ShaderPackingMode CurrentPackingMode = ShaderPackingMode.Texture;
	
	public Material echoMaterial = null;
	public Texture2D EchoTexture;
	public GameObject EchoObject;
	// Echo sphere Properties
	public float sphereMaxRadius = 10.0f;		//Final size of the echo sphere.
	private float sphereCurrentRadius = 0.0f;	//Current size of the echo sphere
	
	public float fadeDelay = 0.0f;			//Time to delay before triggering fade.
	public float fadeRate = 1.0f;			//Speed of the fade away
	public float echoSpeed = 1.0f;			//Speed of the sphere growth.
	public bool is_manual = false;			//Is pulse manual.  if true, pulse triggered by left-mouse click
	
	private bool is_animated = false;		//If true, pulse is currently running.
	public float fade = 0.0f;
	public float pulse_frequency = 5.0f;
	private float deltaTime = 0.0f;
	
	// Use this for initialization
	void Start () {
		if(!is_manual)is_animated = true;
		
		CreateEchoTexture();
	}
	
	/// <summary>
	/// Create an echo texture used to hold multiple echo sources and fades.
	/// </summary>
	void CreateEchoTexture(){
		EchoTexture = new Texture2D(128,128,TextureFormat.RGBA32,false);
		EchoTexture.filterMode = FilterMode.Point;
		EchoTexture.Apply();
		
		//EchoObject.renderer.sharedMaterial.SetTexture("_MainTex",EchoTexture);
		//echoMaterial.mainTexture = EchoTexture;
		echoMaterial.SetTexture("_EchoTex",EchoTexture);

	}
	// Update is called once per frame
	void Update () {
		if(echoMaterial == null)return;
		
		// If manual selection is disabled, automatically trigger a pulse at the given freq.
		if(!is_manual){
			deltaTime += Time.deltaTime;
			if(deltaTime >= pulse_frequency && !is_animated){
				TriggerPulse();
			}
		} else {
			deltaTime += Time.deltaTime;
		}
		
		UpdateRayCast();
		UpdateEcho();
		UpdateTexture();
	}
	
	// Called to trigger an echo pulse
	void TriggerPulse(){
		deltaTime = 0.0f;
		sphereCurrentRadius = 0.0f;
		fade = 0.0f;
		is_animated = true;
	}
	
	// Called to halt an echo pulse.
	void HaltPulse(){
		is_animated = false;	
	}
	
	void ClearPulse(){
		fade = 0.0f;
		sphereCurrentRadius = 0.0f;
		is_animated = false;
	}
		//The step function returns 0.0 for each component of value that is less than edge.
	static float step(float edge, float x) {
		return (x < edge)?0.0f:1.0f;
	}
	static float exp2(float x){
		return (float)Math.Pow(2,x);
	}
	static float abs(float x){
		return (float)Math.Abs(x);
	}
	static int floor(float x){
		return (int)Math.Floor(x);
	}
	static float mod(float x, float y){
		return x - y * floor(x/y);
	}
	static float log2(float x){
		return (float)Math.Log(x,2);
	}
	//unpack a 32bit float from 4 8bit, [0;1] clamped floats
	static float unpackFloat4( Vector4 _packed)
	{
		Vector4 rgba = 255.0f * _packed;
		float sign =  step(-128.0f, -rgba.y) * 2.0f - 1.0f;
		float exponent = rgba.x - 127.0f;    
		if (abs(exponent + 127.0f) < 0.001f)
			return 0.0f;           
		float mantissa =  mod(rgba.y, 128.0f) * 65536.0f + rgba.z * 256.0f + rgba.w + (0x800000);
		return sign *  exp2(exponent-23.0f) * mantissa ;     
	}

	//pack a 32bit float into 4 8bit, [0;1] clamped floats
	static Vector4 packFloat(float f) 
	{
		float F = abs(f); 
		if(F == 0.0)
		{
			return new Vector4(0,0,0,0);
		}
		float Sign =  step(0.0f, -f);
		float Exponent = floor( log2(F)); 

		float Mantissa = F/ exp2(Exponent); 
		//std::cout << "  sign: " << Sign << ", exponent: " << Exponent << ", mantissa: " << Mantissa << std::endl;
		//denormalized values if all exponent bits are zero
		if(Mantissa < 1.0f)
			Exponent -= 1;      

		Exponent +=  127;

		Vector4 rgba = new Vector4(0,0,0,0);
		rgba.x = Exponent;
		rgba.y = 128.0f * Sign +  mod(floor(Mantissa * 128.0f),128.0f);
		rgba.z = floor( mod(floor(Mantissa* exp2(23.0f - 8.0f)), exp2(8.0f)));
		rgba.w = floor( exp2(23.0f)* mod(Mantissa, exp2(-15.0f)));
		
		return (1 / 255.0f) * rgba;
	}
	static Color packFloatColor(float f){
		Vector4 tmp = packFloat(f);
		return new Color(tmp.x,tmp.y,tmp.z,tmp.w);
	}
	// Called to manually place echo pulse
	void UpdateRayCast() {
		if(!is_manual)return;
		if (Input.GetButtonDown("Fire1")){
			Debug.Log("Mouse Click");
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		  	RaycastHit hit;
	        	if (Physics.Raycast(ray,out hit, 10000)) {
	            		Debug.Log("Hit Something");
				TriggerPulse();
				transform.position = hit.point;
			}
		}
	        
	
	}
	
	float frac(float v){
		return v - (float)Math.Floor(v);
	}
	Vector4 frac(Vector4 values) { 
		return new Vector4(frac(values.x),frac(values.y),frac(values.z),frac(values.w));
	}
	Color PackToColor(float v)
	{
		Vector4 bitSh = new Vector4(256.0f * 256.0f * 256.0f, 256.0f * 256.0f, 256.0f, 1.0f);
		Vector4 bitMsk = new Vector4(0.0f, 1.0f / 256.0f, 1.0f / 256.0f, 1.0f / 256.0f);
		Vector4 res = frac(v * bitSh);
		Vector4 xxyz = new Vector4(res.x,res.x,res.y,res.z);
		Vector4 masked = new Vector4(xxyz.x * bitMsk.x,xxyz.y * bitMsk.y,xxyz.z * bitMsk.z,xxyz.w * bitMsk.w);
		res -= masked;
		
		Color ret = new Color(res.x,res.y,res.z,res.w);
		return ret;
	}
	
	Color PackToColor(float v, float min, float max)
	{
		return PackToColor((v - min) / (max - min));
	}
	
	Color PackToColor1000x1000(float v){
		return PackToColor(v,-1000.0f,1000.0f);
	}
	public float encodeValue;
	bool updateOnce = false;
	
	void UpdateTexture(){
		//if(updateOnce)return;
		
		float radius = sphereCurrentRadius;
		float max_radius = sphereMaxRadius;
		float max_fade = sphereMaxRadius / echoSpeed;
		
		if(deltaTime > fadeDelay)
			fade += Time.deltaTime * fadeRate;
		
	
		EchoTexture.SetPixel(0,0,packFloatColor(transform.position.x));
		EchoTexture.SetPixel(0,1,packFloatColor(transform.position.y));
		EchoTexture.SetPixel(0,2,packFloatColor(transform.position.z));
		EchoTexture.SetPixel(0,3,packFloatColor(radius));
		EchoTexture.SetPixel(0,4,packFloatColor(max_radius));
		EchoTexture.SetPixel(0,5,packFloatColor(fade));
		EchoTexture.SetPixel(0,6,packFloatColor(max_fade));
		
		EchoTexture.SetPixel(1,0,packFloatColor(transform.position.x-10.0f));
		EchoTexture.SetPixel(1,1,packFloatColor(transform.position.y));
		EchoTexture.SetPixel(1,2,packFloatColor(transform.position.z));
		EchoTexture.SetPixel(1,3,packFloatColor(radius));
		EchoTexture.SetPixel(1,4,packFloatColor(max_radius));
		EchoTexture.SetPixel(1,5,packFloatColor(fade));
		EchoTexture.SetPixel(1,6,packFloatColor(max_fade));
		EchoTexture.Apply();	
	}
	// Called to update the echo front edge
	void UpdateEcho(){
		if(!is_animated)return;
		
		if(sphereCurrentRadius >= sphereMaxRadius){
			HaltPulse();
		} else {
			sphereCurrentRadius += Time.deltaTime * echoSpeed;  
		}
	}
}
