varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
 
void main(void)
{
  float diffuse = max(dot(vertex_normal, vertex_light_position), 0);
  float ambient = 0.7;
  float lighting = max(ambient,diffuse);
  vec4 color = lighting * vec4(vertex_color, 1);
  
  gl_FragColor = color;
}