using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

// ReSharper disable ClassNeverInstantiated.Global

namespace SemanticKernelDemos.FunctionCalling;

public class OpenWeatherPlugin(HttpClient httpClient, IConfiguration config)
{
    [KernelFunction]
    [Description("Get the latitude and longitude of a location, like a city.")]
    public async Task<LocationData[]> GetLatitudeAndLongitude([Description("The name of the location")] string location)
    {
        var apiKey = config["OpenWeather:ApiKey"];
        var url = $"https://api.openweathermap.org/geo/1.0/direct?q={location}&key={apiKey}&limit=1";
        var response = await httpClient.GetFromJsonAsync<LocationData[]>(url);
        if (response is null)
        {
            return Array.Empty<LocationData>();
        }

        return response;
    }

    [KernelFunction]
    [Description("Get weather information for a location, like a city or country.")]
    public async Task<string> GetWeather(
        [Description("The name of the location, city, region. Can include whitespaces, commas and åäö.")]
        string location)
    {
        var apiKey = config["OpenWeather:ApiKey"];
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apiKey}&units=metric";
        var response = await httpClient.GetFromJsonAsync<WeatherData>(url);
        if (response is null)
        {
            return "No data found.";
        }

        var weather = response.Weather?[0].Description ?? "Unknown";
        var temperature = response.Main?.Temperature ?? 0;
        return $"The weather in {response.CityName} is {weather} with a temperature of {temperature}°C.";
    }
}

public class WeatherData
{
    [JsonPropertyName("name")] public string? CityName { get; set; }

    [JsonPropertyName("main")] public MainData? Main { get; set; }

    [JsonPropertyName("weather")] public WeatherInfo[]? Weather { get; set; }

    public class MainData
    {
        [JsonPropertyName("temp")] public double Temperature { get; set; }

        [JsonPropertyName("feels_like")] public double FeelsLikeTemperature { get; set; }

        [JsonPropertyName("temp_min")] public double MinTemperature { get; set; }

        [JsonPropertyName("temp_max")] public double MaxTemperature { get; set; }

        [JsonPropertyName("humidity")] public double Humidity { get; set; }
    }

    public class WeatherInfo
    {
        [JsonPropertyName("main")] public string? Main { get; set; }

        [JsonPropertyName("description")] public string? Description { get; set; }
    }
}

public class LocationData
{
    [JsonPropertyName("name")] public string? Name { get; set; }

    [JsonPropertyName("country")] public string? Country { get; set; }

    [JsonPropertyName("lat")] public double Latitude { get; set; }

    [JsonPropertyName("lon")] public double Longitude { get; set; }
}