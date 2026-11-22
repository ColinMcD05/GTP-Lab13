using System;
using System.Collections;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    public enum City
    {
        Paris,
        London,
        Tokyo,
        NewYork,
        Sydney
    }

    [Header("Lighting")]
    public Light sun;

    [Header("Skyboxes")]
    public Material skyboxDay;
    public Material skyboxRain;
    public Material skyboxClear;
    public Material skyboxSnowy;
    public Material skyboxNight;

    [Header("City Selection")]
    [SerializeField] private City selectedCity;

    [Header("Sun Settings")]
    [SerializeField] private float dayIntensity = 1.2f;
    [SerializeField] private float nightIntensity = 0.1f;

    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.3f, 0.35f, 0.6f);

    private const string API_KEY = "a0559212e55e03fed969e540e219cef8";

    private string xmlApi;
    private Coroutine weatherRequest;

    private void Start()
    {
        UpdateWeather();
    }
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateWeather();
        }
    }

    // Request weather for city in inspector
    public void UpdateWeather()
    {
        if (weatherRequest != null)
        {
            StopCoroutine(weatherRequest);
        }

        GetCityInfo(out string city, out string countryCode);

        xmlApi = $"https://api.openweathermap.org/data/2.5/weather?q={UnityWebRequest.EscapeURL(city)},{countryCode}&mode=xml&appid={API_KEY}";

        weatherRequest = StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    private void GetCityInfo(out string city, out string countryCode)
    {
        switch (selectedCity)
        {
            case City.Paris:
                city = "Paris";
                countryCode = "FR";
                break;

            case City.London:
                city = "London";
                countryCode = "GB";
                break;

            case City.Tokyo:
                city = "Tokyo";
                countryCode = "JP";
                break;

            case City.NewYork:
                city = "New York";
                countryCode = "US";
                break;

            case City.Sydney:
                city = "Sydney";
                countryCode = "AU";
                break;

            default:
                city = "Paris";
                countryCode = "FR";
                break;
        }
    }

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }

        weatherRequest = null;
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        yield return CallAPI(xmlApi, callback);
    }

    //Reads XML data and changes light and skybox
    public void OnXMLDataLoaded(string data)
    {
        //Get XML data
        var xml = new XmlDocument();
        xml.LoadXml(data);

        XmlNode weatherNode = xml.SelectSingleNode("/current/weather");

        //Get weather and change skybox
        int weather = int.Parse(weatherNode.Attributes["number"].Value);

        int timezoneOffset = int.Parse(xml.SelectSingleNode("/current/city/timezone").InnerText);

        // Get current time in city
        DateTime cityTime = DateTime.UtcNow.AddSeconds(timezoneOffset);

        Debug.Log("City: " + selectedCity);
        Debug.Log("Weather code: " + weather);
        Debug.Log("City time: " + cityTime);

        ChangeSkybox(weather, cityTime);
        ChangeSun(weather, cityTime);
    }


    //Changes the skybox based on weather and time
    private void ChangeSkybox(int weather, DateTime cityTime)
    {
        bool isNight = cityTime.Hour < 6 || cityTime.Hour >= 18;

        if (isNight)
        {
            RenderSettings.skybox = skyboxNight;
        }
        else if (weather >= 200 && weather < 600)
        {
            RenderSettings.skybox = skyboxRain;
        }
        else if (weather >= 600 && weather < 700)
        {
            RenderSettings.skybox = skyboxSnowy;
        }
        else if (weather == 800)
        {
            RenderSettings.skybox = skyboxClear;
        }
        else
        {
            RenderSettings.skybox = skyboxDay;
        }

        DynamicGI.UpdateEnvironment();
    }

    // Changes sunlight based on weather and time
    private void ChangeSun(int weather, DateTime cityTime)
    {
        bool isNight = cityTime.Hour < 6 || cityTime.Hour >= 18;

        if (isNight)
        {
            sun.intensity = nightIntensity;
            sun.color = nightColor;
        }
        else
        {
            sun.intensity = dayIntensity;
            sun.color = dayColor;

            // Reduce sunlight during cloudy or rainy weather
            if (weather >= 200 && weather < 600)
            {
                sun.intensity *= 0.5f;
                sun.color = Color.gray;
            }
            else if (weather >= 801 && weather <= 804)
            {
                sun.intensity *= 0.7f;
                sun.color = new Color(0.85f, 0.85f, 0.9f);
            }
        }
    }
}
