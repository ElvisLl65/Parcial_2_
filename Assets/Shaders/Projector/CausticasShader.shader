// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CausticasShader"
{
	Properties
	{
		_CausticaA("Caustica A", 2D) = "white" {}
		_CausticaB("Caustica B", 2D) = "white" {}
		_ColorCausticas("Color Causticas", Color) = (0.1764706,0.9529412,0.8117647,1)
		_Intensidad("Intensidad", Range( 0 , 5)) = 1.5
		_Escala("Escala", Float) = 1
		_Velocidad("Velocidad", Float) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend One One
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset -1 , -1
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _CausticaA;
			uniform float _Velocidad;
			float4x4 unity_Projector;
			uniform float _Escala;
			uniform sampler2D _CausticaB;
			uniform float4 _ColorCausticas;
			uniform float _Intensidad;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1 = v.vertex;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float mulTime23 = _Time.y * _Velocidad;
				float4 break4 = mul( unity_Projector, i.ase_texcoord1 );
				float4 appendResult5 = (float4(break4.x , break4.y , 0.0 , 0.0));
				float4 temp_output_13_0 = ( ( appendResult5 / break4.w ) * _Escala );
				float2 panner10 = ( mulTime23 * float2( 0.05,0.03 ) + temp_output_13_0.xy);
				float2 panner18 = ( mulTime23 * float2( -0.04,0.025 ) + ( temp_output_13_0 * float4( 0.8,0.8,0.8,0.8 ) ).xy);
				
				
				finalColor = ( ( ( tex2D( _CausticaA, panner10 ) * tex2D( _CausticaB, panner18 ) ) * _ColorCausticas ) * _Intensidad );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
484;73;1469;749;792.4926;6673.688;1.6;True;False
Node;AmplifyShaderEditor.UnityProjectorMatrixNode;1;-911.6165,-6335.389;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.PosVertexDataNode;2;-939.6937,-6185.591;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-666.6937,-6265.591;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;4;-496.6942,-6279.591;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;5;-237.7108,-6412.741;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-140,-6060;Inherit;False;Property;_Escala;Escala;4;0;Create;True;0;0;0;False;0;False;1;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;6;-73.2674,-6245.391;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;60,-6200;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-140,-5900;Inherit;False;Property;_Velocidad;Velocidad;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;265,-6086;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0.8,0.8,0.8,0.8;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;23;60,-5900;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;18;400,-6000;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;-0.04,0.025;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;10;300,-6300;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.05,0.03;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;19;640,-6080;Inherit;True;Property;_CausticaB;Caustica B;1;0;Create;True;0;0;0;False;0;False;-1;9fbef4b79ca3b784ba023cb1331520d5;9fbef4b79ca3b784ba023cb1331520d5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;561.3,-6344.106;Inherit;True;Property;_CausticaA;Caustica A;0;0;Create;True;0;0;0;False;0;False;-1;9fbef4b79ca3b784ba023cb1331520d5;9fbef4b79ca3b784ba023cb1331520d5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;960,-6260;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;8;997,-6156;Inherit;False;Property;_ColorCausticas;Color Causticas;2;0;Create;True;0;0;0;False;0;False;0.1764706,0.9529412,0.8117647,1;0.1752847,0.9528301,0.810582,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;1205,-6337;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;1273,-6234;Inherit;False;Property;_Intensidad;Intensidad;3;0;Create;True;0;0;0;False;0;False;1.5;1.5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;1572,-6298;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1820.7,-6325.493;Float;False;True;-1;2;ASEMaterialInspector;100;1;CausticasShader;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;4;1;False;-1;1;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;2;False;-1;True;3;False;-1;True;True;-1;False;-1;-1;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;4;0;3;0
WireConnection;5;0;4;0
WireConnection;5;1;4;1
WireConnection;6;0;5;0
WireConnection;6;1;4;3
WireConnection;13;0;6;0
WireConnection;13;1;12;0
WireConnection;17;0;13;0
WireConnection;23;0;14;0
WireConnection;18;0;17;0
WireConnection;18;1;23;0
WireConnection;10;0;13;0
WireConnection;10;1;23;0
WireConnection;19;1;18;0
WireConnection;7;1;10;0
WireConnection;20;0;7;0
WireConnection;20;1;19;0
WireConnection;9;0;20;0
WireConnection;9;1;8;0
WireConnection;22;0;9;0
WireConnection;22;1;21;0
WireConnection;0;0;22;0
ASEEND*/
//CHKSM=54C9684CBCB336A9D89AEFF2B8DF2815048012D8