#version 330 core
precision mediump float;
out vec4 FragColor;

in vec3 ourColor;
in vec2 TexCoord;

uniform sampler2D y_tex;
uniform sampler2D uv_tex;

void main()
{
	vec3 yuv;		
    yuv.x = texture(y_tex, TexCoord).r;
	yuv.y = texture(uv_tex, TexCoord).r-0.5;
	yuv.z = texture(uv_tex, TexCoord).g-0.5;
	highp vec3 rgb = mat3(1,1,1,
						  0,-0.344,1.770,
						  1.403,-0.714,0)*yuv;
				
	vec4 currentColor=vec4(rgb,1.0);	
	
	//磨皮程度
	float intensity = 1.0; // 0.0 - 1.0f 再大会很模糊					  
	vec2 blurCoordinates[8];				  

	//步长
    vec2 singleStepOffset=vec2(1.0/float(1280),1.0/float(854));
    //计算均值，20个值  先进行高斯模糊，效果会更好些,20个方向的偏移坐标，覆盖不同半径（模拟高斯分布）
    blurCoordinates[0] = TexCoord.xy + singleStepOffset * vec2(5.0, -8.0);
    blurCoordinates[1] = TexCoord.xy + singleStepOffset * vec2(5.0, 8.0);
    blurCoordinates[2] = TexCoord.xy + singleStepOffset * vec2(-5.0, 8.0);
    blurCoordinates[3] = TexCoord.xy + singleStepOffset * vec2(-5.0, -8.0);
    blurCoordinates[4] = TexCoord.xy + singleStepOffset * vec2(8.0, -5.0);
    blurCoordinates[5] = TexCoord.xy + singleStepOffset * vec2(8.0, 5.0);
    blurCoordinates[6] = TexCoord.xy + singleStepOffset * vec2(-8.0, 5.0);
    blurCoordinates[7] = TexCoord.xy + singleStepOffset * vec2(-8.0, -5.0);

    for (int i = 0; i < 8; i++) {
		vec3 bc_yuv;		
		bc_yuv.x = texture(y_tex, blurCoordinates[i].xy).r;
		bc_yuv.y = texture(uv_tex, blurCoordinates[i].xy).r-0.5;
		bc_yuv.z = texture(uv_tex, blurCoordinates[i].xy).g-0.5;
		highp vec3 bc_rgb = mat3(1,1,1,
							  0,-0.344,1.770,
							  1.403,-0.714,0)*bc_yuv;
        rgb = rgb + bc_rgb;
    }
    //  取平均值,// 均值模糊
    vec4 blur = vec4(rgb*1.0/9.0,currentColor.a);

    //  一个完整的图片相减  差异部分显示出来  高反差 // 提取高频细节
    vec4 highPassColor=currentColor-blur;

    //高反差结果进一步调优  clamp内置函数使结果在0-1之间 2.0 * highPassColor.r * highPassColor.r * 24.0    // 非线性增强

	// 正确的pow函数用法
	const float intensityFactor = 2.0 * 24.0; // 预计算常量
	highPassColor.rgb = clamp(pow(highPassColor.rgb, vec3(2.0)) * intensityFactor, 0.0, 1.0);


    vec4 highPassBlur=vec4(highPassColor.rgb,1.0);

    float b =min(currentColor.b,blur.b);

    //b是上面蓝色通道取值 通过蓝色通道检测皮肤区域（皮肤区域常偏黄/红）
	//皮肤区域在模糊后蓝色通道值较低 通过b < 0.2时mask为0排除非皮肤区域 局限性：
	//对深色肤色敏感度不足
	//光照条件变化时可能失效
    float skinMask = clamp((b - 0.2) * 5.0, 0.0, 1.0);

    float maxChannelColor = max(max(highPassBlur.r, highPassBlur.g), highPassBlur.b);

    //细节的地方，不融合，痘印的地方，融合程度越深
    //细节的地方，值越小，黑色的地方，值越大，计算系数
    float currentIntensity = (1.0 - maxChannelColor / (maxChannelColor + 0.2)) * skinMask * intensity;
	
    //线性融合， 细节的地方不融合，其他地方融合
    //(x,y,a) x⋅(1−a)+y⋅a
    vec3 r =mix(currentColor.rgb,blur.rgb,currentIntensity);

    FragColor=vec4(r,1.0); 
}