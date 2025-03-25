#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 256, local_size_y = 1, local_size_z = 1) in;

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) restrict buffer AgentBuffer
{
    vec4 position[];
    vec4 velocity[];
    vec4 direction[];
};

// The code we want to execute in each invocation
void main() {
    uint id = gl_GlobalInvocationID.x;

    // Get the agent data from the buffer
    vec2 pos = position[id].xy;
    vec2 vel = velocity[id].xy;
    vec2 dir = direction[id].xy;

    vel = dir * 100.0;

    // Update agent position and state in the buffer
    position[id] = vec4(pos + vel, 0.0, 0.0); // Update position
    direction[id] = dir; // Update direction
    velocity[id] = vel;  // Update velocity
}