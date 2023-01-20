Shader "Custom/FiatShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_GlowColor("Glow Color", Color) = (0,0,0,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_ReplacementColor1("Replacment Color Main", Color) = (0,0,0,1)
		_ReplacementColor2("Replacment Color Secondary", Color) = (0,0,0,1)

		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[MaterialToggle] Outline("Outline", Float) = 0
		[MaterialToggle] Glow("Glow", Float) = 0
		[MaterialToggle] Dye("Dyes", Float) = 0
		[MaterialToggle] Replacecolor("ReplaceColor", Float) = 0

		_FirstDye("Dye Number 1", 2D) = "white" {}
		_MaskColor1("Mask Color 1 Light", Color) = (0,0,0,1)
		_MaskColor2("Mask Color 1 Dark", Color) = (0,0,0,1)

		_SecondDye("Dye Number 2", 2D) = "white" {}
		_MaskColor3("Mask Color 1 Light", Color) = (0,0,0,1)
		_MaskColor4("Mask Color 2 Dark", Color) = (0,0,0,1)

		_ShadeFactor("Shade Factor 0 - 1", Float) = 0
		
		_Glow("Glow", Float) = 10
		_GlowTexel("Glow Texel", Float) = 10
		_GlowWidth("Glow Width", Float) = 1
		_GlowBrightness("Glow Brightness", Float) = 10
		_Cur("Cur", Float) = 0
		_TextureScale("Texture Replace Scale", Float) = 1

	}	

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite On
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert

			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ OUTLINE_ON
			#pragma multi_compile _ GLOW_ON
			#pragma multi_compile _ DYE_ON
			#pragma multi_compile _ REPLACECOLOR_ON


			#include "UnityCG.cginc"


			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				//float2 depth : TEXCOORD1;
			};

			struct v2g
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				//float2 depth : TEXCOORD1;
			};

			struct g2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float2 scale : TEXCOORD1;
				float2 texscale : TEXCOORD2;
				//float2 depth : TEXCOORD3;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			fixed4 _Color;
			fixed4 _GlowColor;
			fixed4 _OutlineColor;
			float _Glow;
			float _GlowWidth;
			float _GlowBrightness;
			float _Cur;
			sampler2D _MainTex;
			sampler2D _FirstDye;
			sampler2D _SecondDye;
			float4 _MainTex_TexelSize;
			fixed4 _MaskColor1;
			fixed4 _MaskColor2;
			fixed4 _MaskColor3;
            fixed4 _MaskColor4;
			fixed4 _ReplacementColor1;
			fixed4 _ReplacementColor2;

			float _GlowTexel;
			float _ShadeFactor;
			float _TextureScale;


			v2g vert(appdata_t IN)
			{
				v2g OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;

				//OUT.depth = IN.vertex.z;

				return OUT;
			}

			/*
			[maxvertexcount(3)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> outStream) {

				v2g v1 = input[0];
				v2g v2 = input[1];
				v2g v3 = input[2];

				g2f f1;
				g2f f2;
				g2f f3;

				float2 scale;
				scale.x = max(abs(v1.vertex.x - v2.vertex.x), abs(v1.vertex.x - v3.vertex.x));
				scale.y = max(abs(v1.vertex.y - v2.vertex.y), abs(v1.vertex.y - v3.vertex.y));

				float2 texscale;
				texscale.x = max(abs(v1.texcoord.x - v2.texcoord.x), abs(v1.texcoord.x - v3.texcoord.x));
				texscale.y = max(abs(v1.texcoord.y - v2.texcoord.y), abs(v1.texcoord.y - v3.texcoord.y));

				f1.vertex = v1.vertex;
				f1.color = v1.color;
				f1.texcoord = v1.texcoord;
				f1.scale = scale;
				f1.texscale = texscale;
				//f1.depth = v1.depth;

				f2.vertex = v2.vertex;
				f2.color = v2.color;
				f2.texcoord = v2.texcoord;
				f2.scale = scale;
				f2.texscale = texscale;
				//f2.depth = v2.depth;

				f3.vertex = v3.vertex;
				f3.color = v3.color;
				f3.texcoord = v3.texcoord;
				f3.scale = scale;
				f3.texscale = texscale;
				//f3.depth = v3.depth;

				outStream.Append(f1);
				outStream.Append(f2);
				outStream.Append(f3);
			}
			*/
			
			fixed4 OutlineColor(float2 uv, float2 texel)
			{
				bool upA = tex2D(_MainTex, uv + float2(0, texel.y)).a > 0;
				bool downA = tex2D(_MainTex, uv - float2(0, texel.y)).a > 0;
				bool rightA = tex2D(_MainTex, uv + float2(texel.x, 0)).a > 0;
				bool leftA = tex2D(_MainTex, uv - float2(texel.x, 0)).a > 0;

				bool topLeft = tex2D(_MainTex, uv + float2(-texel.x, texel.y)).a > 0;
				bool topRight = tex2D(_MainTex, uv + float2(texel.x, texel.y)).a > 0;
				bool botLeft = tex2D(_MainTex, uv + float2(-texel.x, -texel.y)).a > 0;
				bool botRight = tex2D(_MainTex, uv + float2(texel.x, -texel.y)).a > 0;

				bool outlined = upA | downA | rightA | leftA | topLeft | topRight | botLeft | botRight;
				return outlined ? _OutlineColor : fixed4(0, 0, 0, 0);
				//return fixed4(0, 0, 0, outlined ? 1 : 0);
			}

			fixed GetGlowColor(float2 uv, float2 texel)
			{
				float size = 10;
				
				float maxSize = (size) * (size) * (_Glow)  ;
				float cur = _Cur;

				for (float y = -size; y <= size; y++) {
					for (float x = -size; x <= size; x++) {
						bool a = tex2D(_MainTex, uv + float2(texel.x * x, texel.y * y)).a > 0;
						if (a) {
							cur++;
						}
					}
				}

				float p = min(maxSize, cur);
				float f = p / maxSize;
				return min(f * f * (_GlowBrightness + sin(_Time.y) *4) , 2.5);
			}

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);

				return color;
			}

			fixed4 SampleMaskTexture(float2 uv, sampler2D t)
			{
				fixed4 color = tex2D(t, uv);
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				//float depthValue = 0.5 + IN.depth * -2;
				//float depthColor = max(0, min(1, depthValue));

				/*
				float2 screenSize = (IN.scale * _ScreenParams.xy) / 2;
				float2 texSize = _MainTex_TexelSize.zw * IN.texscale;
				float2 scale = screenSize / texSize;
				float2 texel = _MainTex_TexelSize.xy / scale;
					//
				//return fixed4 (0,1,0,1);


				*/
				//fixed4 d = SampleSpriteTexture(IN.texcoord);				
				

				float2 texel = _MainTex_TexelSize.xy / 4;
				float2 glowtexel = _MainTex_TexelSize.xy /(_GlowTexel + sin(_Time.y) *3);

				IN.color.rgb /= fixed3(0.5, 0.5, 0.5);

				fixed4 c = SampleSpriteTexture(IN.texcoord);

				#ifdef DYE_ON
				if (c.r == _MaskColor1.r && c.g == _MaskColor1.g && c.b == _MaskColor1.b && c.a == _MaskColor1.a){
					return SampleMaskTexture(IN.texcoord * _TextureScale, _FirstDye);
				}
				if (c.r == _MaskColor2.r && c.g == _MaskColor2.g && c.b == _MaskColor2.b && c.a == _MaskColor2.a){
					fixed4 shaded = SampleMaskTexture(IN.texcoord * _TextureScale, _FirstDye)*(1-_ShadeFactor);
					shaded.a = 1;
					return shaded;
				}
				if (c.r == _MaskColor3.r && c.g == _MaskColor3.g && c.b == _MaskColor3.b && c.a == _MaskColor3.a){
					return SampleMaskTexture(IN.texcoord * _TextureScale, _SecondDye);
				}
				if (c.r == _MaskColor4.r && c.g == _MaskColor4.g && c.b == _MaskColor4.b && c.a == _MaskColor4.a){
					fixed4 shaded = SampleMaskTexture(IN.texcoord * _TextureScale, _SecondDye)*(1-_ShadeFactor);
					shaded.a = 1;
					return shaded;
				}
				#endif

				#ifdef REPLACECOLOR_ON
				if (c.r == _MaskColor1.r && c.g == _MaskColor1.g && c.b == _MaskColor1.b && c.a == _MaskColor1.a){
					return _ReplacementColor1;
				}
				if (c.r == _MaskColor2.r && c.g == _MaskColor2.g && c.b == _MaskColor2.b && c.a == _MaskColor2.a){
					fixed4 replacement = _ReplacementColor1 * (1-_ShadeFactor);
					replacement.a = 1;
					return replacement;
				}
				if (c.r == _MaskColor3.r && c.g == _MaskColor3.g && c.b == _MaskColor3.b && c.a == _MaskColor3.a){
					return _ReplacementColor2;
				}
				if (c.r == _MaskColor4.r && c.g == _MaskColor4.g && c.b == _MaskColor4.b && c.a == _MaskColor4.a){
					fixed4 replacement = _ReplacementColor2 * (1-_ShadeFactor);
					replacement.a = 1;
					return replacement;
				}
				#endif

				//c.rbg *= depthColor;
				if (c.a == 0) {

					#ifdef OUTLINE_ON
					c = OutlineColor(IN.texcoord, texel);
					#endif
					#ifdef GLOW_ON
					if (c.a == 0) 
					{
						c = _GlowColor;
						c.a = GetGlowColor(IN.texcoord, glowtexel );
					}
					#endif
					c.a *= IN.color.a;
				}
				else {
					c *= IN.color;
					//c.a *= IN.color.a;
				}
				if (c.a <= 0) {
					discard;
				}
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}