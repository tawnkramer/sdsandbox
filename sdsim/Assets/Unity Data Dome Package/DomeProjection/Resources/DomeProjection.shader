// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ZubrVR/DomeProjection"
{
	Properties
	{
		[HideInInspector]
		_MainTex ("Texture", CUBE) = "white" {}

		[HideInInspector]
		_Rotation ("Rotation", Vector) = (0.0, 0.0, 0.0, 1.0)

		[HideInInspector]
		_HalfFOV ("HalfFOV", Float) = 1.5707963267949

		[HideInInspector]
		_FadeParams ("FadeParams", Float) = (0.5, 0.5, 0.5, 0.5)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			#define M_PI 3.1415926535897932384626433832795

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			samplerCUBE _MainTex;

			float4 _Rotation;
			float _HalfFOV;
			float4 _FadeParams;

			float3 RotateVector(float4 quat, float3 vec)
			{
				// Rotate a vector using a quaternion.
				return vec + 2.0 * cross(cross(vec, quat.xyz) + quat.w * vec, quat.xyz);
			}

			bool RayCircleIntersect(float2 circleCenter, 
									float  circleRadius,
									float2 rayOrigin,
									float2 rayDirection, 
									out float2 result)
			{
				// 2D Ray-circle intersection. Reference: http://mathworld.wolfram.com/Circle-LineIntersection.html
				float2 diff = rayOrigin - circleCenter;
				float a = dot(rayDirection, rayDirection);
				float b = 2 * dot(rayDirection, diff);
				float c = dot(diff, diff) - circleRadius * circleRadius;
				float disc = b * b - 4 * a * c;
				if (disc >= 0)
				{
					float t = (-b + sqrt(disc)) / (2 * a);
					result = rayOrigin + rayDirection * t;
					return true;
				}

				return false;
			}

			float4 ApplyBackFade(float4 col, float2 uv)
			{
				// Apply a linear top-to-bottom fade.
				const float backFadeIntensity = _FadeParams.x;
				float fade = 1 - (saturate(uv.y) * backFadeIntensity);
				col.rgb *= fade;
				return col;
			}

			float4 ApplyCrescentFade(float4 col, float2 pos)
			{
				const float crescentFadeIntensity = _FadeParams.y;
				const float crescentFadeRadius    = _FadeParams.z;
				const float crescentFadeOffset    = _FadeParams.w;

				float fade = 1;

				// Outer boundary (edge of the fisheye)
				float2 c1 = float2(0, 0);
				float2 r1 = 1;

				// Inner boundary (inner circle of the crescent)
				float2 c2 = float2(0, crescentFadeOffset);
				float  r2 = crescentFadeRadius;

				// Direction to current pixel.
				float2 d  = pos - c2;

				// Check if pos is outside of the inner circle...
				float r = length(pos - c2);
				if (r >= r2)
				{
					// ...if so, calculate the fade amount based on the intersections of the line from the center
					// of the inner circle with both the inner circle (where the crescent is fully transparent)
					// and the outer circle (where the crescent reaches maximum intensity).
					float2 p1, p2;
					RayCircleIntersect(c1, r1, c2, d, p1);
					RayCircleIntersect(c2, r2, c2, d, p2);

					float a = length(p1 - p2);
					float b = length(pos - p2);
					fade = saturate(b / a);
					fade = 1 - (fade * fade * crescentFadeIntensity);	// Use exponential fade for a smooth effect (linear looks ugly).
				}

				return col * fade;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// Center and radius of a perfect circle centered in the middle of the screen.
				float2 c  = _ScreenParams.xy * 0.5f;
				float  r  = min(c.x, c.y);

				// Position of the current pixel relative to the circle.
				float2 p  = i.uv * _ScreenParams.xy - c;

				if (p.x * p.x + p.y * p.y <= r * r)
				{
					// Current pixel lies within the circle; render fisheye view of the cubemap here.

					// Derive spherical coordinates from the position of the pixel within the sphere
					// (we're effectively doing a reverse fisheye projection here)
					p /= r;
					float phi = atan2(p.y, p.x);
					float theta = length(p) * _HalfFOV;

					// Convert phi/theta to a cartesian direction.
					float3 t;
					t.x = sin(theta) * cos(phi);
					t.y = sin(theta) * sin(phi);
					t.z = cos(theta);

					// Offset t by the camera orientation and then lookup resulting direction in the cubemap.
					// (camera orientation is applied here, because Unity only renders axis-aligned cubemaps)
					float4 col = texCUBE(_MainTex, RotateVector(_Rotation, t));

					// Apply fades.
					col = ApplyBackFade(col, i.uv);
					col = ApplyCrescentFade(col, p);

					return col;
				}
				else
				{
					// Current pixel lies outside the circle; render black.
					return fixed4(0, 0, 0, 1);
				}
			}
			ENDCG
		}
	}
}
