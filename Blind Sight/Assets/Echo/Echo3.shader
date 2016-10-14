Shader "Custom/Echo/Single" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color",Color) = (1.0,1.0,1.0,1.0)	
		_Position ("Position",Vector) = (0.0,0.0,0.0)
		_Radius ("Radius",float) = 5.0
		_MaxRadius ("Max Radius",float) = 5.0
		_EchoFade("Echo Fade",float) = 0.0
		_MaxFade("Max Fade",float) = 0.0  
		_DistanceFade("Distance Fade",float) = 1.0	
	} 
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf NoLighting
		#include "UnityCG.cginc"
		
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;	
		};
		
		sampler2D _MainTex;
		sampler2D _EchoTex;
		
		float4 _MainColor;
		float3 _Position;
		float  _Radius;
		float  _MaxRadius;
		float  _Fade;
		float  _MaxFade;
		float _DistanceFade;
		
		// Custom light model that ignores actual lighting. 
		half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}
		
		float3 ApplyFade(Input IN){
			float dist = distance(IN.worldPos, _Position);	// Distance from current pixel (from its world coord) to center of echo sphere
			
			if(_Radius >= 3*_MaxRadius || dist >= _Radius){
				return _MainColor.rgb;//tex2D (_MainTex, IN.uv_MainTex).rgb;
			} else {
				// If _DistanceFade = true, fading is related to vertex distance from echo origin.
				// If false, fading is even across entire echo.
				float c1 = (_DistanceFade>=1.0)?dist/_Radius:1.0;
				
				// Apply fading effect.
				c1 *= (_Fade<=_MaxFade)?1.0-_Fade/_MaxFade:0.0;	//adjust by fade distance.
				
				// Ignore Fade values <= 0 (meaning no fade.)
				c1 = (_Fade<=0)?1.0:c1;
				
				// Amount of blur from base color
				float c2 = 1 - c1;
				
				// Return c2% of main color and c1% of actual texture color.
				return  _MainColor.rgb * c2 + tex2D (_MainTex, IN.uv_MainTex).rgb * c1 ;	
			} 
		}
		
		// Custom surfacer that mimics an echo effect
		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = ApplyFade(IN);			
		}
		ENDCG
		
	} 
	FallBack "Diffuse"
}
