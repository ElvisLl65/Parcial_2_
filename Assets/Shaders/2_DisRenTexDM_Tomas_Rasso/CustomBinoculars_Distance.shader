// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Custom/Binoculars_Distance"
{
	Properties
	{
		_LensRadius("LensRadius", Float) = 0.31
		_LensSoftness("LensSoftness", Float) = 0.04
		_EyeSeparation("EyeSeparation", Float) = 0.19
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noambient nolightmap novertexlights noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _LensRadius;
		uniform float _LensSoftness;
		uniform float _EyeSeparation;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 color24 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			o.Emission = color24.rgb;
			float2 appendResult10 = (float2(( ( i.uv_texcoord.x - 0.5 ) * ( _ScreenParams.x / max( _ScreenParams.y , 1.0 ) ) ) , ( i.uv_texcoord.y - 0.5 )));
			float2 appendResult13 = (float2(-_EyeSeparation , 0.0));
			float2 appendResult14 = (float2(_EyeSeparation , 0.0));
			float smoothstepResult23 = smoothstep( max( 0.001 , ( _LensRadius - _LensSoftness ) ) , _LensRadius , min( distance( appendResult10 , appendResult13 ) , distance( appendResult10 , appendResult14 ) ));
			o.Alpha = smoothstepResult23;
		}

		ENDCG
	}
	Fallback Off
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
284;72.66667;1280;720;1050;220;1.35;True;False
Node;AmplifyShaderEditor.RangedFloatNode;4;-1250,40;Inherit;False;Constant;_One;One;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;2;-1450,-100;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;7;-1180,-40;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-1450,-350;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-1250,-220;Inherit;False;Constant;_Half;Half;3;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1050,140;Inherit;False;Property;_EyeSeparation;EyeSeparation;2;0;Create;True;0;0;0;False;0;False;0.19;0.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;8;-980,-100;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;5;-1050,-380;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;12;-800,80;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-560,400;Inherit;False;Property;_LensRadius;LensRadius;0;0;Create;True;0;0;0;False;0;False;0.31;0.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-560,520;Inherit;False;Property;_LensSoftness;LensSoftness;1;0;Create;True;0;0;0;False;0;False;0.04;0.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;6;-1050,-240;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-780,-340;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;-580,220;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;13;-580,60;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-560,-280;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;20;-300,450;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-300,340;Inherit;False;Constant;_MinInner;MinInner;5;0;Create;True;0;0;0;False;0;False;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;15;-320,-140;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;16;-320,40;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;17;-100,-40;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;22;-80,390;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;23;200,140;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;24;200,-80;Inherit;False;Constant;_MaskColor;MaskColor;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;620,60;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Custom/Binoculars_Distance;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;2;False;-1;7;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;2;2
WireConnection;7;1;4;0
WireConnection;8;0;2;1
WireConnection;8;1;7;0
WireConnection;5;0;1;1
WireConnection;5;1;3;0
WireConnection;12;0;11;0
WireConnection;6;0;1;2
WireConnection;6;1;3;0
WireConnection;9;0;5;0
WireConnection;9;1;8;0
WireConnection;14;0;11;0
WireConnection;13;0;12;0
WireConnection;10;0;9;0
WireConnection;10;1;6;0
WireConnection;20;0;18;0
WireConnection;20;1;19;0
WireConnection;15;0;10;0
WireConnection;15;1;13;0
WireConnection;16;0;10;0
WireConnection;16;1;14;0
WireConnection;17;0;15;0
WireConnection;17;1;16;0
WireConnection;22;0;21;0
WireConnection;22;1;20;0
WireConnection;23;0;17;0
WireConnection;23;1;22;0
WireConnection;23;2;18;0
WireConnection;0;2;24;0
WireConnection;0;9;23;0
ASEEND*/
//CHKSM=55B25364DB65A2EDAD3B0D44D9DF122DB1139B49