varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;

uniform vec2 resolution;
 
void main(void)
{
	float diffuse_value = max(dot(vertex_normal, vertex_light_position), 0.0);
  	vec2 position = (gl_FragCoord.xy / resolution.xy);
	
	vec4 top = vec4(0.0, 0.4, 1.0, 1.0);
	vec4 bottom = vec4(0.0, 1.0, 1.0,1.0);
	
	vec4 color = vec4(mix(bottom, top, position.y)) * max(0.7, diffuse_value);

	gl_FragColor = color;
}