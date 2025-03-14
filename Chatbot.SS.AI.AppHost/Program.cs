var builder = DistributedApplication.CreateBuilder(args);


var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.Chatbot_SS_AI_ApiService>("apiservice");

builder.AddProject<Projects.Chatbot_SS_AI_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WithReference(apiService);

builder.AddProject<Projects.Chatbos_SS_LoginService>("chatbos-ss-loginservice");

builder.Build().Run();
