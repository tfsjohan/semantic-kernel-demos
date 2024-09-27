using Microsoft.AspNetCore.Mvc;
using SemanticKernelDemos.ExtractDetailsFromImageToJson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPubAgreementOrganizationDetailsExtractor();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapPost("/", [IgnoreAntiforgeryToken] async Task<IResult> (
    [FromForm] IFormFile file,
    [FromServices] OrganizationDetailsExtractor extractor,
    CancellationToken cancellationToken = default) =>
{
    using var memoryStream = new MemoryStream();

    await file.CopyToAsync(memoryStream, cancellationToken);

    var data = new ReadOnlyMemory<byte>(memoryStream.ToArray());
    var mimeType = file.ContentType;

    var details = await extractor.ExtractDetailsFromImage(data, mimeType, cancellationToken);

    return Results.Ok(details);
}).DisableAntiforgery();

app.Run();