Shader "Custom/Echo/Sphere" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color",Color) = (1.0,1.0,1.0,1.0)	// Color of echo
		_Position ("Position",Vector) = (0.0,0.0,0.0)		// 
		_Radius ("Radius",float) = 5.0
		_MaxRadius ("Max Radius",float) = 5.0
		_EchoFade("Echo Fade",float) = 0.0
		_MaxFade("Max Fade",float) = 0.0  
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
		 
		// Custom light model that ignores actual lighting. 
		half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten) {
			half NdotL = dot (s.Normal, lightDir);
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
			c.a = s.Alpha;
			return c;
		}
		float3 ApplyFade(Input IN){
			float dist = distance(IN.worldPos, _Position);	// Distance from current pixel (from its world coord) to center of echo sphere
			float fade = (dist - _Radius) * -1 + _Fade;	//Simple fade hack.  Distance from the front edge of the echo.
			float c2 = fade / _MaxRadius;	//Divide by max radius to normalize to [0,1]
			float c1 = 1.0 - c2;		
			
			if(dist < _Radius && fade > 0.0 ){
				return _MainColor.rgb * c1 + tex2D (_MainTex, IN.uv_MainTex).rgb * c2;
			}
			else {
				return tex2D (_MainTex, IN.uv_MainTex).rgb;
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
