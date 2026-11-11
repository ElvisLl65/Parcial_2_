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
		Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay" "IgnoreProjector" = "True" }
		Cull Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade noambient nolightmap novertexlights noshadow
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _LensRadius;
		uniform float _LensSoftness;
		uniform float _EyeSeparation;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = float3( 0, 0, 0 );
			o.Emission = float3( 0, 0, 0 );
			o.Metallic = 0;
			o.Smoothness = 0;

			float aspect = _ScreenParams.x / max( _ScreenParams.y, 1.0 );
			float2 centeredUV = float2( ( i.uv_texcoord.x - 0.5 ) * aspect, i.uv_texcoord.y - 0.5 );

			float dLeft = distance( centeredUV, float2( -_EyeSeparation, 0.0 ) );
			float dRight = distance( centeredUV, float2( _EyeSeparation, 0.0 ) );
			float dBinoc = min( dLeft, dRight );

			float inner = max( 0.001, _LensRadius - _LensSoftness );
			float smoothstepResult11 = smoothstep( inner, _LensRadius, dBinoc );
			o.Alpha = smoothstepResult11;
		}

		ENDCG
	}
	Fallback Off
}
