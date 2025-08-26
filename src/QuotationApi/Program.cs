using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Adiciona logging para vermos os retries em ação
builder.Services.AddLogging(configure => configure.AddConsole());

// Configura o HttpClient com políticas de resiliência
builder.Services.AddHttpClient("AwesomeApiClient", client =>
{
    client.BaseAddress = new Uri("https://economia.awesomeapi.com.br/");
})
.AddPolicyHandler(GetRetryPolicy(builder.Logging.Services.BuildServiceProvider().GetService<ILogger<Program>>()!))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10)); // Timeout de 10s

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "QuotationApi", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Política de Retry
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Trata erros de rede, 5xx e 408
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                logger.LogWarning("Falha na requisição. Tentando novamente em {timespan}. Tentativa {retryAttempt}", timespan, retryAttempt);
            });
}