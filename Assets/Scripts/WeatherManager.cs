using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    public Light sun;

    public Skybox skyboxComponent;
    public Material skyboxDay, skyboxRain, skyboxClear, skyboxSnowy;

    [SerializeField] private string city = "Paris";

    private string xmlApi;

    private void Start()
    {
        xmlApi = $"http://api.openweathermap.org/data/2.5/weather?q={city},us&mode=xml&appid=fb331fce4a7c02fdbe515baf40f51213";
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
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
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        return CallAPI(xmlApi, callback);
    }

    //Reads XML data and changes light and skybox
    public void OnXMLDataLoaded(string data)
    {
        //Get XML data
        var xml = new XmlDocument();
        xml.LoadXml(data);

        //Get weather and change skybox
        int weather = int.Parse(xml.SelectSingleNode("/current/weather")?.Attributes?["number"]?.Value ?? "Unknown");

        ChangeSkybox(weather);

        Debug.Log(weather);
    }
    

    //Changes the skybox based on weather
    private void ChangeSkybox(int weather)
    {
        //Change weather based on code
        if(weather >= 200 && weather < 600)
        {
            skyboxComponent.material = skyboxRain;
        }
        else if(weather >= 600 && weather < 700)
        {
            skyboxComponent.material = skyboxSnowy;
        }
        else if(weather == 800)
        {
            skyboxComponent.material = skyboxClear;
        }
        else
        {
            skyboxComponent.material = skyboxDay;
        }
    }

    //Changes the city and starts looking at weather
    public void ChangeCity(string newCity)
    {
        city = newCity;
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }
}
