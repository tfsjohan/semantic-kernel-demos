namespace SemanticKernelDemos.ExtractDetailsFromImageToJson;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPubAgreementOrganizationDetailsExtractor(this IServiceCollection services)
    {
        services.AddScoped<OrganizationDetailsExtractor>();

        return services;
    }
}