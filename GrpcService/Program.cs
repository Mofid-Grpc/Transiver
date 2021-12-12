using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

var MAX_BUFFER_SIZE = int.MaxValue;
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = MAX_BUFFER_SIZE;
    options.MaxSendMessageSize = MAX_BUFFER_SIZE;
});

var app = builder.Build();

app.MapGrpcService<ReplicatorService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
