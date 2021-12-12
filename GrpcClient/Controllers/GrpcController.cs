using BenchmarkDotNet.Attributes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Replication;
using System.Diagnostics;

namespace GrpcClient.Controllers;

[ApiController]
[Route("[controller]")]
public class GrpcController : ControllerBase
{
    private const int MAX_BUFFER_SIZE = int.MaxValue;

    [HttpGet("group/{size}")]
    [Benchmark]
    public async Task<IActionResult> DispatchGroup([FromRoute] int size)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5165", new GrpcChannelOptions
        {
            MaxReceiveMessageSize = MAX_BUFFER_SIZE,
            MaxSendMessageSize = MAX_BUFFER_SIZE,
        });

        var client = new Replicator.ReplicatorClient(channel);

        var request = new ReplicateRequest();
        request.ReplicaItems.Clear();

        for (int i = 0; i < size; i++)
        {
            request.ReplicaItems.Add(new ReplicaItem
            {
                Title = $"item {i}",
                State = "Ready",
            });
        }

        var timer = new Stopwatch();
        timer.Start();

        var response = await client.ReplicateGroupAsync(request);

        timer.Stop();
        timer.Reset();

        return Ok(timer.ElapsedMilliseconds);
    }

    [HttpGet("single/{size}")]
    [Benchmark]
    public async Task<IActionResult> DispatchSingle([FromRoute] int size)
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5165");
        var client = new Replicator.ReplicatorClient(channel);

        var tasks = new List<AsyncUnaryCall<ReplicaItem>>();

        for (int i = 0; i < size; i++)
        {
            var item = new ReplicaItem
            {
                Title = $"item {i}",
                State = "Ready",
            };

            var task = client.ReplicateSingleAsync(item);
            tasks.Add(task);
        }

        var timer = new Stopwatch();
        timer.Start();

        _ = await Task.WhenAll(tasks.Select(t => t.ResponseAsync));

        timer.Stop();
        timer.Reset();

        return Ok(timer.ElapsedMilliseconds);
    }
}