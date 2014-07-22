uniform mat4 projection_matrix;
uniform mat4 model_matrix;
uniform mat4 view_matrix;
uniform vec3 color;
 
attribute vec3 in_position;
attribute vec3 in_normal;
 
varying vec3 vertex_light_position;
varying vec3 vertex_normal;
varying vec3 vertex_color;
 
void main(void)
{
  vertex_normal = normalize((model_matrix * vec4(in_normal, 0)).xyz);
  vertex_light_position = normalize(vec3(0.5, 0.3, 0.2));
  vertex_color = color;
 
  gl_Position = projection_matrix * view_matrix * model_matrix * vec4(in_position, 1);
}