using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorCalendar.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Newtonsoft.Json;

namespace BlazorCalendar.Services
{
 public class MicrosoftCalendarEventsProvider : ICalendarEventsProvider
 {

  //Get AccessToken
  private readonly IAccessTokenProvider _accessTokenProvider;

  private readonly HttpClient _httpClient;

  private readonly string Base_URL = "https://graph.microsoft.com/v1.0/me/events";

  public MicrosoftCalendarEventsProvider(IAccessTokenProvider accessToken, HttpClient httpClient)
  {
    _accessTokenProvider = accessToken;
    _httpClient = httpClient;
  }
     public async Task<IEnumerable<CalendarEvent>> GetEventsInMonthAsync(int year, int month)
     {
       // 1- Get Token 
            var accessToken = await GetAccessTokenAsync();
            if(accessToken == null)
                return null;

            // 2- Set the access token in the authorization header 
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);

            // 3-  Send the request 
            var response = await _httpClient.GetAsync(ConstructGraphUrl(year, month));
            var resp = response.ReasonPhrase.ToString();
            if(!response.IsSuccessStatusCode)
            {
                return null;
            }

       var contentAsString = await response.Content.ReadAsStringAsync();

       var microsoftEvents = JsonConvert.DeserializeObject<GraphEventsResponse>(contentAsString);
       var events = new List<CalendarEvent>();
       foreach (var item in microsoftEvents.Value)
       {
           events.Add(new CalendarEvent
           {
             Subject = item.Subject,
             StartDate = item.Start.ConvertToLocalDateTime(),
             EndDate = item.End.ConvertToLocalDateTime()
           });
       }

       return events; 

     }

  private async Task<string> GetAccessTokenAsync()
  {
    var tokenRequest = await _accessTokenProvider.RequestAccessToken(new AccessTokenRequestOptions
            {
                Scopes = new[] { "https://graph.microsoft.com/Calendars.ReadWrite" }
            });

            // Try to fetch the token 
            if(tokenRequest.TryGetToken(out var token))
            {
                if(token != null)
                {
                    return token.Value;
                }
            }

            return null; 
  }
        private string ConstructGraphUrl(int year, int month)
        {
            return $"{Base_URL}?$filter=start/datetime ge '{year}-{month}-01T00:00' and end/dateTime lt '{year}-{month+1}-01T00:00'&$select=subject,start,end";
        }



 }
}