var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.ExchangerPool>("exchangerpool").WithExternalHttpEndpoints();
builder.Build().Run();
