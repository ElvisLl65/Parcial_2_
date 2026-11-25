// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HealPostProcess"
{
	Properties
	{
		_MainTex ( "Screen", 2D ) = "black" {}
		_Maintex("_Maintex", 2D) = "white" {}
		_Vector1("Vector 0", Vector) = (0.5,0.5,0,0)
		_HealEdge("HealEdge", Float) = 0.75
		_HealRadius("HealRadius", Float) = 0.35
		_HealIntensity("HealIntensity", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}

	SubShader
	{
		LOD 0

		
		
		ZTest Always
		Cull Off
		ZWrite Off

		
		Pass
		{ 
			CGPROGRAM 

			

			#pragma vertex vert_img_custom 
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			

			struct appdata_img_custom
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				
			};

			struct v2f_img_custom
			{
				float4 pos : SV_POSITION;
				half2 uv   : TEXCOORD0;
				half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
				half4 uv2 : TEXCOORD1;
				half4 stereoUV2 : TEXCOORD3;
		#endif
				
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;
			
			uniform sampler2D _Maintex;
			uniform float4 _Maintex_ST;
			uniform float _HealRadius;
			uniform float _HealEdge;
			uniform float2 _Vector1;
			uniform float _HealIntensity;


			v2f_img_custom vert_img_custom ( appdata_img_custom v  )
			{
				v2f_img_custom o;
				
				o.pos = UnityObjectToClipPos( v.vertex );
				o.uv = float4( v.texcoord.xy, 1, 1 );

				#if UNITY_UV_STARTS_AT_TOP
					o.uv2 = float4( v.texcoord.xy, 1, 1 );
					o.stereoUV2 = UnityStereoScreenSpaceUVAdjust ( o.uv2, _MainTex_ST );

					if ( _MainTex_TexelSize.y < 0.0 )
						o.uv.y = 1.0 - o.uv.y;
				#endif
				o.stereoUV = UnityStereoScreenSpaceUVAdjust ( o.uv, _MainTex_ST );
				return o;
			}

			half4 frag ( v2f_img_custom i ) : SV_Target
			{
				#ifdef UNITY_UV_STARTS_AT_TOP
					half2 uv = i.uv2;
					half2 stereoUV = i.stereoUV2;
				#else
					half2 uv = i.uv;
					half2 stereoUV = i.stereoUV;
				#endif	
				
				half4 finalColor;

				// ase common template code
				float2 uv_Maintex = i.uv.xy * _Maintex_ST.xy + _Maintex_ST.zw;
				float4 color10 = IsGammaSpace() ? float4(1,0.9607843,0.4705882,0) : float4(1,0.9130987,0.1878208,0);
				float2 texCoord2 = i.uv.xy * float2( 1,1 ) + float2( 0,0 );
				float smoothstepResult6 = smoothstep( _HealRadius , _HealEdge , distance( texCoord2 , _Vector1 ));
				float4 lerpResult11 = lerp( tex2D( _Maintex, uv_Maintex ) , color10 , ( smoothstepResult6 * _HealIntensity ));
				

				finalColor = lerpResult11;

				return finalColor;
			} 
			ENDCG 
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
194.6667;72.66667;1120.667;638.3333;884.6548;567.5659;1.993434;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-387.6667,110.5;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;3;-223.6667,274.5;Inherit;False;Property;_Vector1;Vector 0;1;0;Create;True;0;0;0;False;0;False;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DistanceOpNode;4;-32.66666,121.5;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;105.3333,272.5;Inherit;False;Property;_HealEdge;HealEdge;2;0;Create;True;0;0;0;False;0;False;0.75;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;43.33329,44.7999;Inherit;False;Property;_HealRadius;HealRadius;3;0;Create;True;0;0;0;False;0;False;0.35;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;6;284.4335,171.2999;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;160.1566,417.8548;Inherit;False;Property;_HealIntensity;HealIntensity;4;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;467.1457,270.3406;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;520.4048,-37.87426;Inherit;True;Property;_Maintex;_Maintex;0;0;Create;True;0;0;0;True;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;10;578.7779,445.7628;Inherit;False;Constant;_HealColor;HealColor;5;0;Create;True;0;0;0;False;0;False;1,0.9607843,0.4705882,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;11;778.1214,250.4062;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1100.2,221.9484;Float;False;True;-1;2;ASEMaterialInspector;0;4;HealPostProcess;c71b220b631b6344493ea3cf87110c93;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;7;False;-1;False;True;0;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;4;0;2;0
WireConnection;4;1;3;0
WireConnection;6;0;4;0
WireConnection;6;1;7;0
WireConnection;6;2;5;0
WireConnection;9;0;6;0
WireConnection;9;1;8;0
WireConnection;11;0;1;0
WireConnection;11;1;10;0
WireConnection;11;2;9;0
WireConnection;0;0;11;0
ASEEND*/
//CHKSM=B8C214DD62ABF04797DB84B132D87A872A19BC84