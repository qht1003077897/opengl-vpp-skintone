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
				
    FragColor=vec4(rgb,1.0);	
}