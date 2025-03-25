using System;
using System.Diagnostics;
using Godot;

public partial class Renderer : Node
{
    private RenderingDevice rd;

    public override void _Ready()
    {
        // Create a local rendering device.
        rd = RenderingServer.CreateLocalRenderingDevice();

        // Load GLSL shader
        var shaderFile = GD.Load<RDShaderFile>("res://Shaders/compute_test.glsl");
        var shaderBytecode = shaderFile.GetSpirV();
        var shader = rd.ShaderCreateFromSpirV(shaderBytecode);

        // Prepare our data.
        float[] input = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var inputBytes = new byte[input.Length * sizeof(float)];
        Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

        // Create a storage buffer.
        var buffer = rd.StorageBufferCreate((uint)inputBytes.Length, inputBytes);

        // Create a uniform to assign the buffer to the rendering device.
        var uniform = new RDUniform
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 0
        };
        uniform.AddId(buffer);
        var uniformSet = rd.UniformSetCreate([uniform], shader, 0);

        // Create a compute pipeline
        var pipeline = rd.ComputePipelineCreate(shader);
        var computeList = rd.ComputeListBegin();

        // Start the stopwatch before dispatching the compute shader
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Dispatch compute workload
        rd.ComputeListBindComputePipeline(computeList, pipeline);
        rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        rd.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
        rd.ComputeListEnd();

        // Submit to GPU and wait for sync
        rd.Submit();
        rd.Sync();

        // Stop the stopwatch after synchronization
        stopwatch.Stop();

        // Read back the data from the buffer
        var outputBytes = rd.BufferGetData(buffer);
        var output = new float[input.Length];
        Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);

        // Print results
        GD.Print("Input: ", string.Join(", ", input));
        GD.Print("Output: ", string.Join(", ", output));
        GD.Print($"Elapsed Time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
