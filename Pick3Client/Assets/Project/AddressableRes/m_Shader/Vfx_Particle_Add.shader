Shader "Unlit/Vfx_Particle_Add"
{
    Properties
    {
        [Enum(CullMode)] _CULLENUM("剔除模式", int) = 0
        //[Enum(BlendMode)]_src_blend_factory("源混合因子",int)=5
        //[Enum(BlendMode)]_dst_blend_factory("目标混合因子",int)=1
        //[Enum(Off, 0, On, 1)]_ztest_on("z测试",int)=1
        //[Enum(UnityEngine.Rendering.CompareFunction)]_ztest_on("遮挡显示",int)=4
        [Enum(show_all,0,hide_cut,4,show_cut,7)]_ztest_on("遮挡显示",int)=4
        [Enum(BlendMode)]_bloom_dist_factor("bloom模式",int)=0

        [HideInInspector]_CustomTime("_CustomTime", Float) = 0

        [Space(8)]
        [Header(texture _____________________________________________________________)]
        [Space(2)]

        [Toggle(_USE_TEX)]_use_tex("使用贴图", int) = 0
        [HDR]_tex_color("贴图颜色", Color) = (1, 1, 1, 1)
        _tex("贴图", 2D) = "white" {}
        [Toggle(_UV_SPEED_CUSTOM)]_UV_SPEED_CUSTOM("使用粒子custom1.xy(uv0.zw)控制uv偏移",int) = 0
        _tex_uv_speed("贴图UV速度向量", Vector) = (0,0,0,0)
        _tex_rotate("贴图旋转【0到360°】", Range(0, 360)) = 0
        _tex_mask("贴图遮罩", 2D) = "white" {}
        [Toggle(_MASK_UV_SPEED_CUSTOM)]_MASK_UV_SPEED_CUSTOM("粒子custom1.xy(uv1.xy)控制mask uv偏移",int) = 0
        _tex_mask_uv_speed("贴图遮罩UV速度向量", Vector) = (0,0,0,0)
        _tex_mask_rotate("贴图遮罩旋转【0到360°】", Range(0, 360)) = 0
        _tex_bloom("贴图泛光", Range(0, 2)) = 0

        [Space(8)]
        [Header(noise _____________________________________________________________)]
        [Space(2)]

        [Toggle(_USE_UV_NOSIZE)]_use_uv_nosize("使用UV扰动", int) = 0
        _uv_nosize_tex("UV扰动贴图", 2D) = "white" {}
        _uv_nosize_strength("UV扰动横向宽度", Range(0,1)) = 0.01
        _uv_nosize_speed("UV扰动贴图速度向量", Vector) = (0,0,0,0)

        [Space(8)]
        [Header(fresnel _____________________________________________________________)]
        [Space(2)]

        [Toggle(_USE_FNL)]_use_fnl("用菲涅尔", int) = 0
        [HDR]_fnl_color("菲涅尔颜色", Color) = (1, 1, 1, 1)
        _fnl_size("菲涅尔范围", Range(0.0001, 8)) = 1
	    _fnl_intensity("菲涅尔强度", Range(0, 1)) = 1
        _fnl_bloom("菲涅尔泛光", Range(0, 8)) = 0

        [Space(8)]
        [Header(dissove _____________________________________________________________)]
        [Space(2)]

        [Toggle(_USE_DISSOLVE)]_use_dissolve("使用溶解", int) = 0
        _diss_tex("溶解贴图", 2D) = "white"{}
        _diss_tex_rotate("溶解贴图旋转【0到360°】", Range(0, 360)) = 0
	    [ScaleOffset] _diss_tex_offset("溶解贴图偏移", Vector) = (0, 0, 0, 0)
        [HDR]_diss_edge_color("溶解边颜色", Color) = (1, 1, 1, 1)
	    _diss_edge_width("溶解边宽度", Range(0,1)) = 0.01
	    _diss_edge_smoothness("溶解边外侧平滑", Range(0, 3)) = 1.8
	    _diss_smoothness("溶解边内侧平滑", Range(0, 1)) = 0.01
        [Toggle(_DISSOLVE_ALPHA_CLIP)]_diss_alpha_clip("溶解使用alpha做阀值", int) = 0
        [Toggle(_DISSOLVE_CUSTOM)]_DISSOLVE_CUSTOM("custom1.x(uv1.z)做阀值", int) = 0
	    _diss_clip("溶解阀值", Range(-0.2,1.2)) = 0.2
        _diss_bloom("溶解泛光", Range(0, 2)) = 0

        //支持Mask 裁剪的部分
        //Start
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("Color Mask", Float) = 15
        //End

        }
     	CGINCLUDE
		#include "UnityCG.cginc"
        struct a2v{
        	float4 vertex : POSITION;
			float4 normal : NORMAL;
            float4 color :COLOR;

            #if _UV_SPEED_CUSTOM || _MASK_UV_SPEED_CUSTOM
            float4 uv : TEXCOORD0;
            #else
            float2 uv : TEXCOORD0;
            #endif
            #if _MASK_UV_SPEED_CUSTOM || _DISSOLVE_CUSTOM
            float4 uv1 : TEXCOORD1;
            #endif
            #if _DISSOLVE_CUSTOM
            float4 uv2 : TEXCOORD2;
            #endif
        };
        struct v2f{
            float4 pos : SV_POSITION;
            #if _USE_FNL
			float3 vertex : TEXCOORD0;
			float3 world_normal : TEXCOORD1;
			float3 world_pos : TEXCOORD2;
            #endif
            //
            #if _USE_TEX
            fixed4 color :COLOR;
            fixed2 uv_tex:TEXCOORD3;
            fixed2 uv_tex_mask :TEXCOORD4;
            #endif
            //
             #if _USE_UV_NOSIZE
            fixed2 uv_uv_nosize : TEXCOORD5;
            #endif
            //
            #if _USE_DISSOLVE
            fixed2 uv_dissiove : TEXCOORD6;
            #endif

            #if _UV_SPEED_CUSTOM || _MASK_UV_SPEED_CUSTOM
            fixed4 uv_custom : TEXCOORD7;
            #endif

            #if _DISSOLVE_CUSTOM
            fixed4 uv_custom1 : TEXCOORD8;
            #endif
        };

        float _CustomTime;
        //tex
        #if _USE_TEX
        fixed4 _tex_color;
        sampler2D _tex;
        float4 _tex_ST;
        float4 _tex_uv_speed;
        float _tex_rotate;
        sampler _tex_mask;
        float4 _tex_mask_ST;
        float4 _tex_mask_uv_speed;
        float _tex_mask_rotate;
        fixed _tex_bloom;
        #endif

        #if _USE_UV_NOSIZE
        //noise
        sampler2D _uv_nosize_tex;
        fixed4 _uv_nosize_tex_ST;
        fixed _uv_nosize_strength;
        fixed4 _uv_nosize_speed;
        #endif

        #if _USE_FNL
        //fnl
        fixed4 _fnl_color;
		float _fnl_size;
		float _fnl_intensity;
        float _fnl_model_radius;
		float _fnl_vertical_size;
		float _fnl_vertical_intensity;
		float _fnl_rotate_speed;
        fixed _fnl_bloom;
        #endif

        #if _USE_DISSOLVE
        //dissolve
        sampler2D _diss_tex;
        float4 _diss_tex_ST;
         float _diss_tex_rotate;
        half _diss_clip;
		half _diss_smoothness;
		float4 _diss_edge_color;
		float _diss_edge_width;
        float _diss_edge_smoothness;
		float2 _diss_tex_offset;
        fixed _diss_bloom;
        #endif

        v2f vert(a2v v){
          v2f o;
          // common.
          o.pos = UnityObjectToClipPos(v.vertex);

          #if _UV_SPEED_CUSTOM
          o.uv_custom.xy = v.uv.zw;
          #endif
          #if _MASK_UV_SPEED_CUSTOM
          o.uv_custom.zw = v.uv1.xy;
          #endif
          #if _DISSOLVE_CUSTOM
          o.uv_custom1.x = v.uv1.z;
          #endif

          #if _USE_FNL
		  o.vertex = v.vertex;
		  o.world_normal = UnityObjectToWorldNormal(v.normal);
		  o.world_pos = mul(unity_ObjectToWorld, v.vertex).xyz;
          #endif
         
          // tex
          #if _USE_TEX
          o.color = v.color;
          o.uv_tex = TRANSFORM_TEX(v.uv,_tex);
          o.uv_tex_mask = TRANSFORM_TEX(v.uv,_tex_mask);
          #endif
          // noise
          #if _USE_UV_NOSIZE
          o.uv_uv_nosize = TRANSFORM_TEX(v.uv,_uv_nosize_tex);
          #endif
          // dissove
          #if _USE_DISSOLVE
          float time = fmod(_Time.y, 2e5);
		  o.uv_dissiove = TRANSFORM_TEX(v.uv, _diss_tex) + frac(_diss_tex_offset.xy * (time-_CustomTime));
          #endif
          return o;
        }

        fixed4 frag(v2f i):SV_Target
        {
            const fixed3 tbl = float3(0.299,0.587,0.114);
            fixed4 col = fixed4(0,0,0,1);//i.color;//fixed4(1,1,1,1);
            float time = fmod(_Time.y, 2e5);
            // uv noise.
            #if _USE_UV_NOSIZE
            fixed4 uvnosize = fixed4(0,0,0,0);
            uvnosize = tex2D(_uv_nosize_tex, i.uv_uv_nosize + (time-_CustomTime) * _uv_nosize_speed.xy);
            #endif

            // main texture.
            #if _USE_TEX
            {
                fixed2 uv = i.uv_tex;
                if(_tex_rotate>0.1){
                    float angle = 3.1415926/180*_tex_rotate;
                    float2 temp_uv = (uv-_tex_ST.zw)/_tex_ST.xy - float2(0.5,0.5);// - float2(_tex_ST.x, _tex_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                    uv = (uv+float2(0.5,0.5))*_tex_ST.xy + _tex_ST.zw;//uv += float2(_tex_ST.x, _tex_ST.y) * 0.5;
                }
                #if _USE_UV_NOSIZE
                uv += uvnosize.xy*_uv_nosize_strength;
                #endif
                #if _UV_SPEED_CUSTOM
                col = tex2D(_tex,uv+(time-_CustomTime)*_tex_uv_speed.xy+i.uv_custom.xy);
                #else
                col = tex2D(_tex,uv+(time-_CustomTime)*_tex_uv_speed.xy);
                #endif
                col *= _tex_color*i.color;
                //++
                uv = i.uv_tex_mask;
                if(_tex_mask_rotate>0.1){
                    float angle = 3.1415926/180*_tex_mask_rotate;
                    float2 temp_uv = (uv-_tex_mask_ST.zw)/_tex_mask_ST.xy - float2(0.5,0.5);// - float2(_tex_mask_ST.x, _tex_mask_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                   uv = (uv+float2(0.5,0.5))*_tex_mask_ST.xy + _tex_mask_ST.zw;//uv += float2(_tex_mask_ST.x, _tex_mask_ST.y) * 0.5;
                }
                #if _USE_UV_NOSIZE
                uv += uvnosize.xy*_uv_nosize_strength;
                #endif
                #if _MASK_UV_SPEED_CUSTOM
                fixed4 mask = tex2D(_tex_mask,uv+(time-_CustomTime)*_tex_mask_uv_speed.xy + i.uv_custom.zw);
                #else
                fixed4 mask = tex2D(_tex_mask,uv+(time-_CustomTime)*_tex_mask_uv_speed.xy);
                #endif
                fixed mask_key = dot(mask.rgb,tbl)*mask.a;
                col.a *= mask_key;
            }
            #endif
            

            #if _USE_FNL
            {
                float3 world_normal = normalize(i.world_normal);
			    float3 world_view = normalize(UnityWorldSpaceViewDir(i.world_pos));
			    float fresnel = pow(1 - saturate(dot(world_normal, world_view)), _fnl_size) * _fnl_intensity;
			   // float vertical = pow(saturate(_fnl_model_radius - abs(i.vertex.y)), _fnl_vertical_size) * _fnl_vertical_intensity;

			    fixed4 fnl_col = _fnl_color * fresnel;// * vertical;

                col.rgb += fnl_col.rgb;
            }
            #endif

            #if _USE_DISSOLVE
            {
                fixed2 uv = i.uv_dissiove;
                                if(_diss_tex_rotate>0.1){
                    float angle = 3.1415926/180*_diss_tex_rotate;
                    float2 temp_uv = (uv-_diss_tex_ST.zw)/_diss_tex_ST.xy - float2(0.5,0.5);// - float2(_diss_tex_ST.x, _diss_tex_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                    uv = (uv+float2(0.5,0.5))*_diss_tex_ST.xy + _diss_tex_ST.zw;//uv += float2(_diss_tex_ST.x, _diss_tex_ST.y) * 0.5;
                }
                #if _USE_UV_NOSIZE
                uv+=uvnosize.xy*_uv_nosize_strength;
                #endif
                fixed4 dissove_clr = tex2D(_diss_tex, uv);
				fixed dissove = dot(dissove_clr.rgb,tbl);//*dissove_clr.a;
                #if _DISSOLVE_CUSTOM
                float dc = i.uv_custom1.x;
                #elif _DISSOLVE_ALPHA_CLIP
                float dc = ((1.0-i.color.a)*step(0.001,i.color.a));
                #else
                float dc = _diss_clip;
                #endif
                float dissove_offset = dissove - dc;
                clip(dissove_offset);
				float edge_area = saturate(1 - saturate((dissove_offset - _diss_edge_width) / _diss_smoothness)) * step(0.0001,dc);
				//edge_area *= _diss_edge_color.a;
                float clr_edge_area = _diss_edge_color.a*edge_area;
				col.rgb = col.rgb * (1 - clr_edge_area)+ _diss_edge_color.rgb * clr_edge_area;
                //col.a = _Bloom*(col.a*(1-edge_area))+edge_area;
                col.a = col.a*((1.0-edge_area) + _diss_edge_color.a*edge_area*saturate(pow(dissove_offset /_diss_edge_width,_diss_edge_smoothness)));
            }
            #endif

            return col;
        }
                
        fixed4 frag_bloom(v2f i):SV_Target
        {
            const fixed3 tbl = float3(0.299,0.587,0.114);
            float time = fmod(_Time.y, 2e5);
            fixed4 col = fixed4(0,0,0,0);//i.color;//fixed4(1,1,1,1);
            fixed mask_a=1.0;
            #if _USE_UV_NOSIZE
            fixed4 uvnosize = tex2D(_uv_nosize_tex, i.uv_uv_nosize + (time-_CustomTime) * _uv_nosize_speed.xy);
            #endif

            #if _USE_TEX
            {
                fixed2 uv = i.uv_tex;
                if(_tex_rotate>0.1){
                    float angle = 3.1415926/180*_tex_rotate;
                    float2 temp_uv = (uv-_tex_ST.zw)/_tex_ST.xy - float2(0.5,0.5);// - float2(_tex_ST.x, _tex_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                    uv = (uv+float2(0.5,0.5))*_tex_ST.xy + _tex_ST.zw;//uv += float2(_tex_ST.x, _tex_ST.y) * 0.5;
                }
                
                #if _USE_UV_NOSIZE
                uv += uvnosize.xy*_uv_nosize_strength;
                #endif

                #if _UV_SPEED_CUSTOM
                col = tex2D(_tex,uv+(time-_CustomTime)*_tex_uv_speed.xy+i.uv_custom.xy);
                #else
                col = tex2D(_tex,uv+(time-_CustomTime)*_tex_uv_speed.xy);
                #endif
                col *= _tex_color*i.color;
                //++
                uv = i.uv_tex_mask;
                if(_tex_mask_rotate>0.1){
                    float angle = 3.1415926/180*_tex_mask_rotate;
                    float2 temp_uv = (uv-_tex_mask_ST.zw)/_tex_mask_ST.xy - float2(0.5,0.5);// - float2(_tex_mask_ST.x, _tex_mask_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                   uv = (uv+float2(0.5,0.5))*_tex_mask_ST.xy + _tex_mask_ST.zw;//uv += float2(_tex_mask_ST.x, _tex_mask_ST.y) * 0.5;
                }
                #if _USE_UV_NOSIZE
                uv += uvnosize.xy*_uv_nosize_strength;
                #endif
                #if _MASK_UV_SPEED_CUSTOM
                fixed4 mask = tex2D(_tex_mask,uv+(time-_CustomTime)*_tex_mask_uv_speed.xy + i.uv_custom.zw);
                #else
                fixed4 mask = tex2D(_tex_mask,uv+(time-_CustomTime)*_tex_mask_uv_speed.xy);
                #endif
                fixed mask_key = dot(mask.rgb,tbl)*mask.a;
                //
                mask_a = col.a*mask_key;
                col.a=mask_a* _tex_bloom;
            }
            #endif

            #if _USE_FNL
            {
                float3 world_normal = normalize(i.world_normal);
			    float3 world_view = normalize(UnityWorldSpaceViewDir(i.world_pos));
			    float fresnel = pow(1 - saturate(dot(world_normal, world_view)), _fnl_size) * _fnl_intensity;
			    //float vertical = pow(saturate(_fnl_model_radius - abs(i.vertex.y)), _fnl_vertical_size) * _fnl_vertical_intensity;

			    fixed4 fnl_col = _fnl_color * fresnel;// * vertical;

                //col.rgb += fnl_col.rgb;
                col.a += fnl_col.a * _fnl_bloom;
            }
            #endif

            #if _USE_DISSOLVE
            {
                fixed2 uv = i.uv_dissiove;
                                if(_diss_tex_rotate>0.1){
                    float angle = 3.1415926/180*_diss_tex_rotate;
                    float2 temp_uv = (uv-_diss_tex_ST.zw)/_diss_tex_ST.xy - float2(0.5,0.5);// - float2(_diss_tex_ST.x, _diss_tex_ST.y) * 0.5;
                    uv.x = temp_uv.x * cos(angle) + temp_uv.y * sin(angle);
                    uv.y = -temp_uv.x * sin(angle) + temp_uv.y  * cos(angle);
                    uv = (uv+float2(0.5,0.5))*_diss_tex_ST.xy + _diss_tex_ST.zw;//uv += float2(_diss_tex_ST.x, _diss_tex_ST.y) * 0.5;
                }
                #if _USE_UV_NOSIZE
                uv+=uvnosize.xy*_uv_nosize_strength;
                #endif
                fixed4 dissove_clr = tex2D(_diss_tex, uv);
				fixed dissove = dot(dissove_clr.rgb,tbl);//*dissove_clr.a;
                #if _DISSOLVE_CUSTOM
                float dc = i.uv_custom1.x;
                #elif _DISSOLVE_ALPHA_CLIP
                float dc = ((1.0-i.color.a)*step(0.001,i.color.a));
                #else
                float dc = _diss_clip;
                #endif
                float dissove_offset = dissove - dc;
                clip(dissove_offset);
				float edge_area = saturate(1 - saturate((dissove_offset - _diss_edge_width) / _diss_smoothness)) * step(0.0001,dc);
				//edge_area *= _diss_edge_color.a;
                float clr_edge_area = _diss_edge_color.a*edge_area;
				//col.rgb = col.rgb * (1 - clr_edge_area)+ _diss_edge_color.rgb * clr_edge_area;
                //col.a = _Bloom*(col.a*(1-edge_area))+edge_area;
                col.a = col.a*(1.0-edge_area) + mask_a*_diss_edge_color.a*edge_area*saturate(pow(dissove_offset /_diss_edge_width,_diss_edge_smoothness))*_diss_bloom;
            }
            #endif
            return col;
        }

        ENDCG

        SubShader
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"} 
            ZTest [_ztest_on]
            ZWrite Off
            
            CULL [_CULLENUM]

            //支持Mask 裁剪的部分
            //Start
             Stencil
             {
                  Ref[_Stencil]
                  Comp[_StencilComp]
                  Pass[_StencilOp]
                  ReadMask[_StencilReadMask]
                  WriteMask[_StencilWriteMask]
             }
             ColorMask[_ColorMask]
            //End
            		
                    
            Pass
		    {
			    COLORMASK RGB
                Blend SrcAlpha One  //[_src_blend_factory] [_dst_blend_factory]

			    CGPROGRAM
			    #pragma vertex vert
			    #pragma fragment frag

                #pragma shader_feature_local _USE_TEX
                #pragma shader_feature_local _USE_UV_NOSIZE
                #pragma shader_feature_local _USE_FNL
                #pragma shader_feature_local _USE_DISSOLVE
                #pragma shader_feature_local _DISSOLVE_ALPHA_CLIP
                          #pragma shader_feature_local _UV_SPEED_CUSTOM
                #pragma shader_feature_local _MASK_UV_SPEED_CUSTOM
                #pragma shader_feature_local _DISSOLVE_CUSTOM
			    ENDCG
		    }

            Pass
		    {
			    COLORMASK A

			    //Blend One OneMinusSrcAlpha
			    Blend One [_bloom_dist_factor]

			    CGPROGRAM
			    #pragma vertex vert
			    #pragma fragment frag_bloom

                #pragma shader_feature_local _USE_TEX
                #pragma shader_feature_local _USE_UV_NOSIZE
                #pragma shader_feature_local _USE_FNL
                #pragma shader_feature_local _USE_DISSOLVE
                #pragma shader_feature_local _DISSOLVE_ALPHA_CLIP
                #pragma shader_feature_local _UV_SPEED_CUSTOM
                #pragma shader_feature_local _MASK_UV_SPEED_CUSTOM
                #pragma shader_feature_local _DISSOLVE_CUSTOM
			    ENDCG
		    }
           
        }
}