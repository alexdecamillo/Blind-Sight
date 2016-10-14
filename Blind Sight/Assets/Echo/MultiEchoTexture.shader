Shader "Custom/Echo/MultipleUsingTexture" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_EchoTex ("Echo (RGBA)", 2D) = "white" {}
		_MainColor ("Main Color",Color) = (1.0,1.0,1.0,1.0)	
		_DistanceFade("Distance Fade",float) = 1.0	
		_MaxRadius("MaxRadius",float) = 1.0
		_MaxFade("MaxFade",float) = 1.0
		
	} 
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		
		// Target Shader Model 3.0 or you won't have enough arithmetic instructions for float unpack
		#pragma target 3.0
		#pragma surface surf NoLighting
		#include "UnityCG.cginc"
		
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;	
		};
		
		sampler2D _MainTex;
		sampler2D _EchoTex;
		
		float4 _MainColor;
		float _DistanceFade;
		float _MaxRadius;
		float _MaxFade;
		
		// Custom light model that ignores actual lighting. 
		half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}
		
		//unpack float from pixel
		float unpackFloat4( float4 _packed)
		{
		    float4 rgba = 255.0 * _packed;
		    float sign =  step(-128.0, -rgba[1]) * 2.0 - 1.0;
		    float exponent = rgba[0] - 127.0;    
		    if (abs(exponent + 127.0) < 0.001){
		        return 0.0;      
		    } else {      
			    float mantissa =  fmod(rgba[1], 128.0) * 65536.0 + rgba[2] * 256.0 + rgba[3] + (0x800000);
			    return sign *  exp2(exponent-23.0) * mantissa ;     
			}
		}
		float ApplyFade(Input IN,float textureOffset){
			float3 position;
			float size = 128.0; 		//hard coded texture width/height
			
			//NOTE:  UV's are [0,1] so divide by the width(or height, they should be equal) of the image.
			//In this example the texture is 128x128.
			position[0] = unpackFloat4(tex2D(_EchoTex,float2(textureOffset/size,0.0)));
			position[1] = unpackFloat4(tex2D(_EchoTex,float2(textureOffset/size,1.0/size)));
			position[2] = unpackFloat4(tex2D(_EchoTex,float2(textureOffset/size,2.0/size)));
			
			float radius = unpackFloat4(tex2D(_EchoTex,float2(textureOffset/size,3.0/size)));
			float infade = unpackFloat4(tex2D(_EchoTex,float2(textureOffset/size,4.0/size)));
			
			float dist = distance(IN.worldPos, position);	// Distance from current pixel (from its world coord) to center of echo sphere
			
			if(radius >= 3*_MaxRadius || dist >= radius){
				return 0.0;
			} else {
				// If _DistanceFade = true, fading is related to vertex distance from echo origin.
				// If false, fading is even across entire echo.
				float c1 = (_DistanceFade>=1.0)?dist/radius:1.0;
				 
				// Apply fading effect.
				c1 *= (infade<=_MaxFade)?1.0-infade/_MaxFade:0.0;	//adjust by fade distance.
				
				// Ignore Fade values <= 0 (meaning no fade.)
				c1 = (infade<=0)?1.0:c1;
				
				return c1;
			}
		}
		// Custom surfacer that mimics an echo effect
		void surf (Input IN, inout SurfaceOutput o) {
			float c1;
			//Unity was not pleased with my use of a for loop.  Somewhere
			//in the loop unroll Unity blew up and crashed.  So echo count
			//is hardcoded to be 3 here.
			c1 += ApplyFade(IN,0.0);
			c1 += ApplyFade(IN,1.0);
			c1 += ApplyFade(IN,2.0);
			c1 /= 3.0;  
			
			float c2 = 1.0 - c1;
			
			o.Albedo = _MainColor.rgb * c2 + tex2D (_MainTex, IN.uv_MainTex).rgb * c1 ;		
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
