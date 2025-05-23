using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
 using VeragTvApp.server.Models.Zeiterfassung;
using System.Collections.Generic;
using System.Linq;

namespace VeragTvApp.server.Services
{
 

    public class ZeiterfassungServices 
    {
        private readonly ILogger<ZeiterfassungServices> _logger;
         private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public ZeiterfassungServices(
            ILogger<ZeiterfassungServices> logger,
             IHttpClientFactory httpClientFactory,
            IOptions<ApiSettings> apiSettingsOptions)
        {
            _logger = logger;
             _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettingsOptions.Value;
        }

        /// <summary>
        /// Ruft die Mitarbeiterliste ab und füllt die übergebene DataTable.
        /// </summary>
        /// <param name="dt">DataTable, die mit Mitarbeiterdaten gefüllt wird.</param>
        /// <returns>String, der den Status der Operation angibt.</returns>
        public async Task<string> GetEmployeeListAsync(DataTable dt)
        {
            string failureText = "";
            string result = "Success";

            try
            {
           

                // HttpClient erstellen
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // HTTP GET-Anfrage senden
                string requestUri = "/rest/web-api/employees/";
                HttpResponseMessage response = await client.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {response.StatusCode} {errorText}");
                    return $"{(int)response.StatusCode} {errorText}";
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Antwort: {responseContent}");

                // JSON-Antwort parsen
                JArray jsonArray;
                try
                {
                    jsonArray = JArray.Parse(responseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler: {jsonEx.Message}");
                    return $"JSON-Parsing-Fehler: {jsonEx.Message}";
                }

                if (jsonArray.Count == 0)
                {
                    _logger.LogWarning("Array Failure");
                    return "Array Failure";
                }

                // DataTable-Spalten definieren, falls nicht vorhanden
                if (dt != null && !dt.Columns.Contains("Id"))
                {
                    dt.Columns.Add("Id", typeof(int));
                    dt.Columns.Add("Personal-Nr (Lohn)", typeof(string));
                    dt.Columns.Add("Vorname", typeof(string));
                    dt.Columns.Add("Nachname", typeof(string));
                    dt.Columns.Add("Geschlecht", typeof(string));
                    dt.Columns.Add("Ausweis-Nr", typeof(int));
                    dt.Columns.Add("Info", typeof(string));
                }

                dt.Clear();

                // DataTable mit Mitarbeiterdaten füllen
                foreach (var employee in jsonArray)
                {
                    DataRow row = dt.NewRow();
                    row["Id"] = employee.Value<int>("id");
                    row["Personal-Nr (Lohn)"] = employee.Value<string>("pnr1");
                    row["Vorname"] = employee.Value<string>("firstname");
                    row["Nachname"] = employee.Value<string>("lastname");
                    row["Geschlecht"] = employee.Value<string>("gender");
                    row["Ausweis-Nr"] = employee.Value<int>("card");
                    row["Info"] = employee.Value<string>("info");
                    dt.Rows.Add(row);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                return ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                return ex.Message;
            }

            return result;
        }




        public async Task<string> GetEmployeeHolidayListAsync(string groupName, int month, int year)
        {
            string failureText = "";
            string result = "Success";

            try
            {
             

                // HttpClient erstellen und konfigurieren
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // 1. Gruppen abrufen
                string groupRequestUri = "/rest/web-api/groups3/";
                HttpResponseMessage groupResponse = await client.GetAsync(groupRequestUri);
                if (!groupResponse.IsSuccessStatusCode)
                {
                    string errorText = groupResponse.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler beim Abrufen der Gruppen: {groupResponse.StatusCode} {errorText}");
                    return $"{(int)groupResponse.StatusCode} {errorText}";
                }
                string groupResponseContent = await groupResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Gruppen Antwort: {groupResponseContent}");

                JArray groupsArray;
                try
                {
                    groupsArray = JArray.Parse(groupResponseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler der Gruppen: {jsonEx.Message}");
                    return $"JSON-Parsing-Fehler der Gruppen: {jsonEx.Message}";
                }

                // Filter: Nur die Gruppe mit dem angegebenen Namen berücksichtigen
                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    var filteredGroups = groupsArray
                        .Where(g => string.Equals(g.Value<string>("name"), groupName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!filteredGroups.Any())
                    {
                        _logger.LogWarning($"Gruppe '{groupName}' nicht gefunden.");
                        return $"Gruppe '{groupName}' nicht gefunden.";
                    }
                    groupsArray = new JArray(filteredGroups);
                }
                else
                {
                    _logger.LogWarning("Kein Gruppenname als Query-Parameter angegeben.");
                    return "Kein Gruppenname als Query-Parameter angegeben.";
                }

                // 2. Mitarbeiter abrufen
                string employeeRequestUri = "/rest/web-api/employees/";
                HttpResponseMessage employeeResponse = await client.GetAsync(employeeRequestUri);
                if (!employeeResponse.IsSuccessStatusCode)
                {
                    string errorText = employeeResponse.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler beim Abrufen der Mitarbeiter: {employeeResponse.StatusCode} {errorText}");
                    return $"{(int)employeeResponse.StatusCode} {errorText}";
                }
                string employeeResponseContent = await employeeResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"Mitarbeiter Antwort: {employeeResponseContent}");

                JArray employeesArray;
                try
                {
                    employeesArray = JArray.Parse(employeeResponseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler der Mitarbeiter: {jsonEx.Message}");
                    return $"JSON-Parsing-Fehler der Mitarbeiter: {jsonEx.Message}";
                }
                if (employeesArray.Count == 0)
                {
                    _logger.LogWarning("Array Failure");
                    return "Array Failure";
                }

                // Dictionary erstellen: Mitarbeiter-ID -> "Vorname Nachname"
                Dictionary<int, string> employeeNames = new Dictionary<int, string>();
                foreach (var employee in employeesArray)
                {
                    int id = employee.Value<int>("id");
                    string fullName = $"{employee.Value<string>("firstname")} {employee.Value<string>("lastname")}";
                    employeeNames[id] = fullName;
                }

                // Gruppen aktualisieren: "members" von einfachen IDs zu Objekten umwandeln
                foreach (var group in groupsArray)
                {
                    JArray membersArray = group.Value<JArray>("members");
                    JArray updatedMembersArray = new JArray();
                    if (membersArray != null)
                    {
                        foreach (var member in membersArray)
                        {
                            int memberId = member.Value<int>();

                            // Abrufen der Urlaubsdaten für den Mitarbeiter
                            var holidayResult = await GetEmployeeHolidaysAsync(memberId, year, month);

                            // Abrufen des monatlichen Überstundensaldos für den Mitarbeiter
                            var monthlyBalance = await GetDailyBalanceYesterdayAsync(memberId);

                            // Mitarbeiterobjekt inkl. Urlaubsdaten und Überstundensaldo erstellen
                            JObject memberObj = new JObject
                            {
                                ["id"] = memberId,
                                ["name"] = employeeNames.ContainsKey(memberId) ? employeeNames[memberId] : "Unbekannt",
                                ["holidays"] = JToken.FromObject(holidayResult),
                                ["monthlyOvertimeBalance"] = JToken.FromObject(monthlyBalance)
                            };

                            updatedMembersArray.Add(memberObj);
                        }
                    }
                    group["members"] = updatedMembersArray;
                }


                // Zusammengesetztes Ergebnisobjekt erstellen, das nur die (gefilterte) Gruppe enthält
                JObject resultObject = new JObject
                {
                    ["groups"] = groupsArray
                };

                _logger.LogInformation($"Ergebnis: {resultObject.ToString()}");
                return resultObject.ToString();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                return ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                return ex.Message;
            }
        }







        /// <summary>
        /// Ruft einen einzelnen Mitarbeiter anhand der Mitarbeiter-ID ab.
        /// </summary>
        /// <param name="mitarbeiterID">Die ID des Mitarbeiters.</param>
        /// <returns>Ein Objekt vom Typ TimasEmployee oder null bei Fehlern.</returns>
        public async Task<TimasEmployee> GetEmployeeAsync(string mitarbeiterID)
        {
            try
            {
                // Verbindung überprüfen
                string failureText = string.Empty;
            

                // HttpClient erstellen
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // HTTP GET-Anfrage senden
                string requestUri = $"/rest/web-api/employees/{mitarbeiterID}";
                HttpResponseMessage response = await client.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    return null;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Antwort: {responseContent}");

                // JSON-Antwort parsen
                JObject json;
                try
                {
                    json = JObject.Parse(responseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler: {jsonEx.Message}");
                    return null;
                }

                if (json == null || !json.HasValues)
                {
                    _logger.LogWarning("Leere JSON-Antwort erhalten.");
                    return null;
                }

                // TimasEmployee-Objekt erstellen und füllen
                var emp = new TimasEmployee
                {
                    ID = json.Value<int>("id"),
                    ExternId = json.Value<string>("externid"),
                    Pnr1 = json.Value<string>("pnr1"),
                    Pnr2 = json.Value<string>("pnr2"),
                    FirstName = json.Value<string>("firstname"),
                    LastName = json.Value<string>("lastname"),
                    Gender = json.Value<string>("gender"),
                    ClientNumber = json.Value<string>("clientNumber"),
                    Card = json["card"] != null && int.TryParse(json.Value<string>("card").ToString(), out int cardValue) ? cardValue : 0,
                    Info = json.Value<string>("info"),
                    RFID = json.Value<string>("rfid"),
                    Login = json.Value<string>("login"),
                    Email = json.Value<string>("email"),
                    LoginActive = json.Value<bool?>("loginActive") ?? false,
                    Password = json.Value<string>("password"),
                    Street = json.Value<string>("street"),
                    Zipcode = json.Value<string>("zipcode"),
                    City = json.Value<string>("city"),
                    Phone1 = json.Value<string>("phone1"),
                    Phone2 = json.Value<string>("phone2")
                };

                // Datumfelder prüfen und setzen
                emp.BirthdayDate = !string.IsNullOrEmpty(json.Value<string>("birthday")) && json.Value<string>("birthday") != "null"
                    ? DateTime.Parse(json.Value<string>("birthday")).ToString("yyyy-MM-dd")
                    : null;

                emp.EntryDate = !string.IsNullOrEmpty(json.Value<string>("entry")) && json.Value<string>("entry") != "null"
                    ? DateTime.Parse(json.Value<string>("entry")).ToString("yyyy-MM-dd")
                    : null;

                emp.ExitDate = !string.IsNullOrEmpty(json.Value<string>("exit")) && json.Value<string>("exit") != "null"
                    ? DateTime.Parse(json.Value<string>("exit")).ToString("yyyy-MM-dd")
                    : null;

                // Gruppen verarbeiten
                JArray groups = (JArray)json["groups"];
                if (groups == null)
                {
                    _logger.LogWarning("Gruppenmitgliedschaft nicht gefunden.");
                }
                else
                {
                    emp.Groups = new List<TimasGroup>();
                    foreach (var group in groups)
                    {
                        if (group.Type == JTokenType.Integer)
                        {
                            var timasGroup = new TimasGroup(emp.ID)
                            {
                                Id = group.Value<int>(),
                                // Name, GroupType und Info können hier gesetzt werden, falls verfügbar
                                // Beispiel: Name = "Gruppenname", falls aus einer anderen Quelle verfügbar
                            };
                            emp.Groups.Add(timasGroup);
                        }
                        else
                        {
                            _logger.LogWarning($"Unerwarteter Gruppentyp: {group.Type}");
                        }
                    }
                }

                // CustomFields verarbeiten
                JArray customFields = (JArray)json["customFields"];
                if (customFields != null)
                {
                    foreach (var field in customFields)
                    {
                        var customField = new TimasCustomField(emp.ID)
                        {
                            // Id kann hier nicht gesetzt werden, da es nicht in der JSON-Antwort vorhanden ist
                            // Setzen Sie es auf 0 oder einen anderen Standardwert
                            Id = 0, // oder generieren Sie eine eindeutige ID, falls erforderlich
                            Name = field.Value<string>("name"),
                            Value = field.Value<string>("value"),
                            Type = field.Value<string>("type")
                        };
                        emp.CustomFields.Add(customField);
                    }
                }
                else
                {
                    _logger.LogWarning("CustomFields nicht gefunden.");
                }

                _logger.LogDebug($"Mitarbeiterdaten: {json}");

                return emp;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                return null;
            }
        }
   
 
/// <summary>
/// Setzt Zeitkonten-Einträge für einen Mitarbeiter.
/// </summary>
/// <param name="mitarbeiterID">Die ID des Mitarbeiters.</param>
/// <param name="accountEntrys">Die Zeitkonten-Einträge.</param>
/// <param name="accountDate">Das Datum der Einträge.</param>
/// <returns>Ein Ergebnisobjekt, das den Erfolg und zusätzliche Informationen enthält.</returns>
public async Task<SetTimeAccountEntriesResult> SetTimeAccountEntriesAsync(int mitarbeiterID, List<AccountEntryDto> accountEntrys, DateTime accountDate)
        {
            var result = new SetTimeAccountEntriesResult
            {
                TimeEntryCreated = false,
                Info = string.Empty
            };

            string failureText = string.Empty;

            try
            {
                _logger.LogInformation($"SetTimeAccountEntriesAsync aufgerufen für Mitarbeiter ID: {mitarbeiterID}.");

                // Verbindung überprüfen
               

                // Überprüfen, ob der Mitarbeiter eine Timas-Zuordnung hat
                if (!(mitarbeiterID > 0))
                {
                    _logger.LogWarning("Mitarbeiter besitzt keine Timas-Zuordnung.");
                    result.Info = "Mitarbeiter besitzt keine Timas-Zuordnung";
                    return result;
                }

                // JSON-Array erstellen
                var jsonArr = new JArray();

                foreach (var ae in accountEntrys)
                {
                    var jsonObj = new JObject
                    {
                        { "accountid", ae.AccountId },
                        { "employeeid", mitarbeiterID },
                        { "date", accountDate.ToString("yyyy-MM-dd") },
                        { "value", ae.Value }
                    };

                    jsonArr.Add(jsonObj);
                }

                _logger.LogDebug($"JSON-Array erstellt: {jsonArr.ToString()}");

                // HttpClient erstellen
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // HTTP POST-Anfrage senden
                var content = new StringContent(jsonArr.ToString(), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("/rest/web-api/accounts/values", content);

                _logger.LogInformation($"HTTP POST an /rest/web-api/accounts/values gesendet. Statuscode: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    result.Info = $"{(int)response.StatusCode} {errorText}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }

                // Überprüfen, ob der Statuscode 200 ist
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    string responseHeader = response.Headers.ToString();
                    _logger.LogWarning($"HTTP-Fehler: {response.StatusCode} {response.ReasonPhrase}");
                    result.Info = $"{(int)response.StatusCode} {response.ReasonPhrase}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }
                else
                {
                    _logger.LogInformation("Zeitkonten-Einträge erfolgreich erstellt.");
                    result.TimeEntryCreated = true;
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                result.Info = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Ruft Zeitkonten-Einträge für einen Mitarbeiter ab.
        /// </summary>
        /// <param name="from">Startdatum.</param>
        /// <param name="toDate">Enddatum.</param>
        /// <param name="employeeNr">Mitarbeiternummer.</param>
        /// <returns>Ein Ergebnisobjekt mit den Zeitkonten-Einträgen.</returns>
        public async Task<GetTimeAccountsResult> GetTimeAccountsAsync(DateTime from, DateTime toDate, int employeeNr)
        {
            var result = new GetTimeAccountsResult
            {
                Success = false,
                Info = string.Empty,
                TimeAccountEntries = new List<TimeAccountEntryDto>()
            };

            try
            {
                _logger.LogInformation($"GetTimeAccountsAsync aufgerufen für Mitarbeiter Nr: {employeeNr} von {from:yyyy-MM-dd} bis {toDate:yyyy-MM-dd}.");

                // Verbindung überprüfen
                string failureText = string.Empty;
          

                List<int> accountIds;

                accountIds = new List<int> { 65, 69, 70, 72, 73, 438, 986, 987, 993, 1209, 1210, 1227, 1231, 1235, 1257, 1272, 1366, 1393, 1550, 1551, 1553, 1598 };

                // HTTP GET-Anfrage vorbereiten
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Query-Parameter hinzufügen
                var queryParams = new Dictionary<string, string>
                {
                    { "from", from.ToString("yyyy-MM-dd") },
                    { "to", toDate.ToString("yyyy-MM-dd") },
                    { "employees", employeeNr.ToString() }
                };

                // Account-IDs als kommaseparierte Liste
                string accounts = string.Join(",", accountIds);
                queryParams.Add("accounts", accounts);

                // Aufbau der Query-String
                var query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                var requestUri = $"/rest/web-api/accounts/values?{query}";

                _logger.LogInformation($"HTTP GET an {requestUri} gesendet.");

                // HTTP GET-Anfrage senden
                HttpResponseMessage response = await client.GetAsync(requestUri);

                _logger.LogInformation($"HTTP GET erhalten. Statuscode: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    result.Info = $"{(int)response.StatusCode} {errorText}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }

                string sbResponseBody = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Antwort-Body empfangen: {sbResponseBody}");

                // JSON-Array laden
                var jsonArray = JArray.Parse(sbResponseBody);

                foreach (var item in jsonArray)
                {
                    if (item["value"] != null && !string.IsNullOrWhiteSpace(item["value"].ToString()))
                    {
                        var entry = new TimeAccountEntryDto
                        {
                            AccountId = item["accountid"].Value<int>(),
                            Date = DateTime.Parse(item["date"].ToString()),
                            Value = ParseTimeToDouble(item["value"].ToString()),
                            Multiplier = item["value"].ToString().Contains("-") ? "-" : "+"
                        };

                        result.TimeAccountEntries.Add(entry);
                    }
                }

                // Sortieren nach AccountId und Date
                result.TimeAccountEntries.Sort((x, y) =>
                {
                    int accountComparison = x.AccountId.CompareTo(y.AccountId);
                    if (accountComparison == 0)
                    {
                        return x.Date.CompareTo(y.Date);
                    }
                    return accountComparison;
                });

                result.Success = true;
                _logger.LogInformation("Zeitkonten-Einträge erfolgreich abgerufen.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                result.Info = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Ruft Urlaubsdaten für einen Mitarbeiter ab.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr für die Urlaubsdaten.</param>
        /// <param name="month">Der Monat für die Urlaubsdaten.</param>
        /// <returns>Ein Ergebnisobjekt mit den Urlaubsdaten.</returns>
        public async Task<GetEmployeeHolidaysResult> GetEmployeeHolidaysAsync(int employeeId, int year, int month)
        {
            var result = new GetEmployeeHolidaysResult
            {
                Success = false,
                Info = string.Empty,
                Holidays = new List<HolidayDto>()
            };

            try
            {
                _logger.LogInformation($"GetEmployeeHolidaysAsync aufgerufen für Mitarbeiter ID: {employeeId}, Jahr: {year}, Monat: {month}.");

                // Verbindung überprüfen
           
                // HTTP GET-Anfrage vorbereiten
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Aufbau der Request-URI für den aktuellen Monat
                var requestUri = $"/rest/web-api/employees/{employeeId}/holidays/{year}/{month}";
                _logger.LogInformation($"HTTP GET an {requestUri} gesendet.");

                // HTTP GET-Anfrage senden (Aktueller Monat)
                HttpResponseMessage response = await client.GetAsync(requestUri);
                _logger.LogInformation($"HTTP GET erhalten. Statuscode: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    result.Info = $"{(int)response.StatusCode} {errorText}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }

                string sbResponseBody = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Antwort-Body empfangen: {sbResponseBody}");

                // JSON-Array laden und die Urlaubsdaten für den aktuellen Monat erstellen
                var jsonArray = JArray.Parse(sbResponseBody);

                foreach (var item in jsonArray)
                {
                    var holiday = new HolidayDto
                    {
                        AccountName = item["accountName"].ToString(),
                        AccountID = item["accountID"].Value<int>(),
                        Available = item["available"].Value<double>(),
                        Capping = item["capping"].Value<double>(),
                        Correction = item["correction"].Value<double>(),
                        HolidayType = item["holidayType"].ToString(),
                        Old = item["old"].Value<double>(),
                        Planned = item["planned"].Value<double>(),
                        SpecialClaim = item["specialClaim"].Value<double>(),
                        SummCorrCapp = item["summCorrCapp"].Value<double>(),
                        Taken = item["taken"].Value<double>(),
                        TotalClaim = item["totalClaim"].Value<double>(),
                        TotalClaim2 = item["totalClaim2"].Value<double>(),
                        YearClaim = item["yearClaim"].Value<double>(),
                        EmployeeID = item["employeeID"].Value<int>()
                    };

                    // Vorinitialisierung der neuen Felder (Default: 0)
                    holiday.TakenPrevious = 0.0;
                    holiday.DifferenceTaken = holiday.Taken;

                    result.Holidays.Add(holiday);
                }

                // Abruf der Urlaubsdaten für den Vormonat:
                int previousMonth = month - 1;
                int previousYear = year;
                if (previousMonth < 1)
                {
                    previousMonth = 12;
                    previousYear = year - 1;
                }

                var requestUriPrevious = $"/rest/web-api/employees/{employeeId}/holidays/{previousYear}/{previousMonth}";
                _logger.LogInformation($"HTTP GET an {requestUriPrevious} (Vormonat) gesendet.");
                HttpResponseMessage responsePrevious = await client.GetAsync(requestUriPrevious);
                _logger.LogInformation($"HTTP GET vom Vormonat erhalten. Statuscode: {responsePrevious.StatusCode}");

                if (responsePrevious.IsSuccessStatusCode)
                {
                    string sbResponseBodyPrevious = await responsePrevious.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Antwort-Body (Vormonat) empfangen: {sbResponseBodyPrevious}");

                    var jsonArrayPrevious = JArray.Parse(sbResponseBodyPrevious);

                    // Erstellen einer Liste der HolidayDto für den Vormonat
                    var holidaysPrevious = new List<HolidayDto>();

                    foreach (var item in jsonArrayPrevious)
                    {
                        var holidayPrev = new HolidayDto
                        {
                            AccountName = item["accountName"].ToString(),
                            AccountID = item["accountID"].Value<int>(),
                            Available = item["available"].Value<double>(),
                            Capping = item["capping"].Value<double>(),
                            Correction = item["correction"].Value<double>(),
                            HolidayType = item["holidayType"].ToString(),
                            Old = item["old"].Value<double>(),
                            Planned = item["planned"].Value<double>(),
                            SpecialClaim = item["specialClaim"].Value<double>(),
                            SummCorrCapp = item["summCorrCapp"].Value<double>(),
                            Taken = item["taken"].Value<double>(),
                            TotalClaim = item["totalClaim"].Value<double>(),
                            TotalClaim2 = item["totalClaim2"].Value<double>(),
                            YearClaim = item["yearClaim"].Value<double>(),
                            EmployeeID = item["employeeID"].Value<int>()
                        };

                        holidaysPrevious.Add(holidayPrev);
                    }

                    // Vergleichen: Den „Taken“-Wert des Vormonats den aktuellen Einträgen zuordnen
                    if (previousMonth != 12)
                    {
                        foreach (var currentHoliday in result.Holidays)
                        {
                            var previousHoliday = holidaysPrevious.FirstOrDefault(ph =>
                                ph.AccountID == currentHoliday.AccountID &&
                                ph.HolidayType.Equals(currentHoliday.HolidayType, StringComparison.OrdinalIgnoreCase));

                            if (previousHoliday != null)
                            {
                                currentHoliday.TakenPrevious = previousHoliday.Taken;
                                currentHoliday.DifferenceTaken = previousHoliday.Taken - currentHoliday.Taken;
                                currentHoliday.AvaiblePrevious = currentHoliday.TotalClaim2 - currentHoliday.DifferenceTaken;
                                currentHoliday.Available = currentHoliday.TotalClaim2;
                            }
                        }
                    }
                    else
                    {
                        foreach (var currentHoliday in result.Holidays)
                        {
                            var previousHoliday = holidaysPrevious.FirstOrDefault(ph =>
                                ph.AccountID == currentHoliday.AccountID &&
                                ph.HolidayType.Equals(currentHoliday.HolidayType, StringComparison.OrdinalIgnoreCase));

                            if (previousHoliday != null)
                            {
                                currentHoliday.TakenPrevious = previousHoliday.Taken;
                                currentHoliday.DifferenceTaken = -currentHoliday.Taken;
                                currentHoliday.AvaiblePrevious = previousHoliday.Available;
                                currentHoliday.Available = currentHoliday.TotalClaim2;
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Abruf der Urlaubsdaten für den Vormonat schlug fehl. Es werden nur die aktuellen Werte geliefert.");
                }

                // Nur Datensätze mit einem definierten Jahresurlaubsanspruch (YearClaim != 0) behalten
                result.Holidays = result.Holidays.Where(h => h.YearClaim != 0).ToList();

                // Berechnung des aliquoten Urlaubs basierend auf dem aktuellen Monat
                foreach (var holiday in result.Holidays)
                {
                    // Aliquot = (Monate / 12) * Jahresurlaub, wobei 'month' die aktuell verstrichenen Monate repräsentiert.
                    var test = holiday.Available - holiday.YearClaim;

                    holiday.Aliqout  = (month / 12.0) * holiday.YearClaim + test;
                }

                // Sortierung der Urlaubsdaten
                result.Holidays.Sort((x, y) =>
                {
                    int accountComparison = x.AccountID.CompareTo(y.AccountID);
                    if (accountComparison == 0)
                    {
                        return x.HolidayType.CompareTo(y.HolidayType);
                    }
                    return accountComparison;
                });

                result.Success = true;
                _logger.LogInformation("Urlaubsdaten erfolgreich abgerufen.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                // HTTP-spezifische Ausnahme
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");

                // Baue Info-Text inkl. InnerException auf
                var inner = ex.InnerException != null
                    ? $" Innere Ausnahme: {ex.InnerException.Message}"
                    : string.Empty;
                result.Info = $"HTTP-Fehler: {ex.Message}.{inner}";

                return result;
            }
            catch (Exception ex)
            {
                // Allgemeine Ausnahme
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");

                var inner = ex.InnerException != null
                    ? $" Innere Ausnahme: {ex.InnerException.Message}"
                    : string.Empty;
                result.Info = $"{ex.Message}.{inner}";

                return result;
            }
        }



        /// <summary>
        /// Ruft die Buchungen eines Mitarbeiters anhand der Mitarbeiter-ID ab.
        /// Optional können Jahr und Monat angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr zur Filterung (optional).</param>
        /// <param name="month">Der Monat zur Filterung (optional).</param>
        /// <returns>Ein Ergebnisobjekt mit den Buchungen.</returns>
        public async Task<GetEmployeeBookingsResult> GetEmployeeBookingsAsync(int employeeId, int? year, int? month)
        {
            var result = new GetEmployeeBookingsResult
            {
                Success = false,
                Info = string.Empty,
                Bookings = new List<BookingDto>()
            };

            try
            {
                _logger.LogInformation($"GetEmployeeBookingsAsync aufgerufen für Mitarbeiter ID: {employeeId}, Jahr: {year}, Monat: {month}.");

                // Verbindung überprüfen
           

                // HttpClient erstellen
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // HTTP GET-Anfrage senden
                string requestUri = $"/rest/web-api/employees/{employeeId}/bookings";
                HttpResponseMessage response = await client.GetAsync(requestUri);

                _logger.LogInformation($"HTTP GET an {requestUri} gesendet. Statuscode: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    result.Info = $"{(int)response.StatusCode} {errorText}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Antwort-Body empfangen: {responseContent}");

                // JSON-Array parsen
                JArray jsonArray;
                try
                {
                    jsonArray = JArray.Parse(responseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler: {jsonEx.Message}");
                    result.Info = $"JSON-Parsing-Fehler: {jsonEx.Message}";
                    return result;
                }

                // Buchungsdaten verarbeiten
                foreach (var booking in jsonArray)
                {
                    var bookingDto = new BookingDto
                    {
                        Id = booking.Value<long>("id"),
                        Stamp = booking.Value<DateTime>("stamp"),
                        EmployeeId = booking.Value<int>("employeeid"),
                        Card = booking.Value<int>("card"),
                        StatusId = booking.Value<int>("statusid"),
                        StatusNr = booking.Value<int>("statusnr"),
                        StatusName = booking.Value<string>("statusname"),
                        StatusShort = booking.Value<string>("statusshort"),
                        StatusColor = booking.Value<string>("statuscolor"),
                        Type = booking.Value<string>("type"),
                        TypeShort = booking.Value<string>("typeshort"),
                        TypeColor = booking.Value<string>("typecolor"),
                        Source = booking.Value<string>("source"),
                        Lat = booking["lat"]?.Type != JTokenType.Null ? booking.Value<double?>("lat") : null,
                        Lon = booking["lon"]?.Type != JTokenType.Null ? booking.Value<double?>("lon") : null,
                        Ip = booking.Value<string>("ip"),
                        Terminal = null // Initialisieren Sie es als null
                    };

                    // Terminal-Feld verarbeiten
                    var terminalToken = booking["terminal"];
                    if (terminalToken != null && terminalToken.Type != JTokenType.Null)
                    {
                        if (terminalToken.Type == JTokenType.Object)
                        {
                            // Deserialisieren in TerminalInfoDto
                            bookingDto.Terminal = terminalToken.ToObject<TerminalInfoDto>();
                        }
                        else
                        {
                            _logger.LogWarning($"Unerwarteter Typ für 'terminal': {terminalToken.Type}. Erwartet JObject oder null.");
                        }
                    }

                    result.Bookings.Add(bookingDto);
                }

                // Filtern der Buchungen basierend auf Jahr und Monat, falls angegeben
                if (year.HasValue || month.HasValue)
                {
                    result.Bookings = result.Bookings.Where(b =>
                        (!year.HasValue || b.Stamp.Year == year.Value) &&
                        (!month.HasValue || b.Stamp.Month == month.Value)
                    ).ToList();

                    _logger.LogInformation($"Buchungsdaten nach Jahr und Monat gefiltert. Gefundene Einträge: {result.Bookings.Count}");
                }

                result.Success = true;
                _logger.LogInformation($"Buchungsdaten für Mitarbeiter ID: {employeeId} erfolgreich abgerufen.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                result.Info = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Ruft die Tageswerte eines Mitarbeiters anhand der Mitarbeiter-ID ab.
        /// Optional können Jahr und Monat angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr zur Filterung (optional).</param>
        /// <param name="month">Der Monat zur Filterung (optional).</param>
        /// <returns>Ein Ergebnisobjekt mit den Tageswerten.</returns>
        public async Task<GetEmployeeDayValuesResult> GetEmployeeDayValuesAsync(int employeeId, int? year, int? month)
        {
            var result = new GetEmployeeDayValuesResult
            {
                Success = false,
                Info = string.Empty,
                DayValues = new List<DayValueDto>()
            };

            try
            {
                _logger.LogInformation($"GetEmployeeDayValuesAsync aufgerufen für Mitarbeiter ID: {employeeId}, Jahr: {year}, Monat: {month}.");

                // Verbindung überprüfen
               
                // HttpClient erstellen
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);

                // Basic Authentication Header setzen
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Berechnung von 'from' und 'to' basierend auf 'year' und 'month'
                DateTime fromDate;
                DateTime toDate;

                if (year.HasValue && month.HasValue)
                {
                    fromDate = new DateTime(year.Value, month.Value, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                }
                else if (year.HasValue)
                {
                    fromDate = new DateTime(year.Value, 1, 1);
                    toDate = new DateTime(year.Value, 12, 31);
                }
                else
                {
                    // Wenn weder Jahr noch Monat angegeben sind, holen Sie alle verfügbaren Daten
                    fromDate = DateTime.MinValue;
                    toDate = DateTime.MaxValue;
                }

                // HTTP GET-Anfrage senden mit 'from' und 'to' Parametern
                string requestUri = $"/rest/web-api/employees/{employeeId}/dayvalues?from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}";

                HttpResponseMessage response = await client.GetAsync(requestUri);

                _logger.LogInformation($"HTTP GET an {requestUri} gesendet. Statuscode: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    _logger.LogWarning($"HTTP-Fehler: {(int)response.StatusCode} {errorText}");
                    result.Info = $"{(int)response.StatusCode} {errorText}";

                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        result.Info += "\n" + responseBody;
                        _logger.LogWarning($"Antwort-Body: {responseBody}");
                    }

                    return result;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Antwort-Body empfangen: {responseContent}");

                // JSON-Array parsen
                JArray jsonArray;
                try
                {
                    jsonArray = JArray.Parse(responseContent);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogError($"JSON-Parsing-Fehler: {jsonEx.Message}");
                    result.Info = $"JSON-Parsing-Fehler: {jsonEx.Message}";
                    return result;
                }

                // Tageswerte verarbeiten
                foreach (var dayValue in jsonArray)
                {
                    var dayValueDto = new DayValueDto
                    {
                        Date = dayValue.Value<DateTime>("date"),
                        ActualMins = dayValue.Value<int>("actual_mins"),
                        DebitMins = dayValue.Value<int>("debit_mins"),
                        ValuatedMins = dayValue.Value<int>("valuated_mins"),
                        DifferenceMins = dayValue.Value<int>("valuated_mins") - dayValue.Value<int>("debit_mins"),
                        BreakDeductionMins = dayValue.Value<int>("breakdeduction_mins"),
                        Begin = dayValue["begin"]?.Type != JTokenType.Null ? dayValue.Value<DateTime?>("begin") : null,
                        End = dayValue["end"]?.Type != JTokenType.Null ? dayValue.Value<DateTime?>("end") : null,
                        ResultCode = dayValue.Value<int>("resultCode"),
                        Workstation = dayValue.Value<string>("workstation")
                    };

                    // Sicherstellen, dass 'shift' ein JObject ist, bevor es geparst wird
                    var shiftToken = dayValue["shift"];  
                    if (shiftToken != null && shiftToken.Type == JTokenType.Object)
                    {
                        var shiftObj = (JObject)shiftToken;
                        dayValueDto.Shift = new ShiftDto
                        {
                            Id = shiftObj.Value<int>("id"),
                            Number = shiftObj.Value<int>("number"),
                            Name = shiftObj.Value<string>("name"),
                            Short = shiftObj.Value<string>("short"),
                            Color = shiftObj.Value<string>("color")
                        };

                        // **Neue Logik: Überprüfen, ob shift.Short == "AF"**
                        if (dayValueDto.Shift.Short == "AF")
                        {
                            _logger.LogInformation($"Shift.Short ist 'AF' für Datum: {dayValueDto.Date}. Setze DebitMins und DifferenceMins auf 0.");
                            dayValueDto.DebitMins = 0;
                            dayValueDto.DifferenceMins = 0;
                        }
                    }
                    else if (shiftToken != null && shiftToken.Type != JTokenType.Null)
                    {
                        // Falls 'shift' vorhanden ist, aber kein JObject, loggen wir dies als Warnung
                        _logger.LogWarning($"Unerwarteter Typ für 'shift': {shiftToken.Type}. Erwartet JObject oder null.");
                    }

                    // Sicherstellen, dass 'status' korrekt geparst wird
                    var statusToken = dayValue["status"];
                    if (statusToken != null && statusToken.Type == JTokenType.Object)
                    {
                        try
                        {
                            dayValueDto.Status = statusToken.ToObject<StatusDto>();
                            _logger.LogDebug($"Parsed status name: {dayValueDto.Status.Name} for date: {dayValueDto.Date}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Fehler beim Deserialisieren von 'status' für Datum: {dayValueDto.Date}");
                        }
                    }
                    else if (statusToken != null && statusToken.Type != JTokenType.Null)
                    {
                        // Falls 'status' vorhanden ist, aber kein JObject, loggen wir dies als Warnung
                        _logger.LogWarning($"Unerwarteter Typ für 'status': {statusToken.Type}. Erwartet JObject oder null.");
                    }

                    result.DayValues.Add(dayValueDto);
                }

                // Lokale Filterung nach 'resultCode', falls erforderlich
                if (year.HasValue || month.HasValue)
                {
                    // Da die API bereits nach Datum filtert, ist keine zusätzliche Filterung nach Jahr und Monat nötig
                    // Wir filtern nur noch nach 'resultCode: -10', falls dies gewünscht ist
                    var originalCount = result.DayValues.Count;
                    result.DayValues = result.DayValues.Where(d => d.ResultCode != -10).ToList();
                    var filteredCount = result.DayValues.Count;

                    if (originalCount != filteredCount)
                    {
                        _logger.LogInformation($"Tageswerte mit resultCode: -10 wurden gefiltert. Original: {originalCount}, Gefiltert: {filteredCount}");
                    }
                }

                result.Success = true;
                _logger.LogInformation($"Tageswerte für Mitarbeiter ID: {employeeId} erfolgreich abgerufen.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                result.Info = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }


        /// <summary>
        /// Ruft die kombinierten Daten (Buchungen und Tageswerte) eines Mitarbeiters ab.
        /// Optional können Jahr und Monat angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr zur Filterung (optional).</param>
        /// <param name="month">Der Monat zur Filterung (optional).</param>
        /// <returns>Ein Ergebnisobjekt mit den kombinierten Daten.</returns>
        public async Task<GetEmployeeDataResult> GetEmployeeDataAsync(int employeeId, int? year, int? month)
        {
            var result = new GetEmployeeDataResult
            {
                Success = false,
                Info = string.Empty,
                EmployeeData = new EmployeeDataDto
                {
                    Bookings = new List<BookingDto>(),
                    DayValues = new List<DayValueDto>()
                }
            };

            try
            {
                _logger.LogInformation($"GetEmployeeDataAsync aufgerufen für Mitarbeiter ID: {employeeId}, Jahr: {year}, Monat: {month}.");

                // Parallel beide APIs aufrufen
                var bookingsTask = GetEmployeeBookingsAsync(employeeId, year, month);
                var dayValuesTask = GetEmployeeDayValuesAsync(employeeId, year, month);

                await Task.WhenAll(bookingsTask, dayValuesTask);

                var bookingsResult = bookingsTask.Result;
                var dayValuesResult = dayValuesTask.Result;

                if (!bookingsResult.Success && !dayValuesResult.Success)
                {
                    result.Info = $"Buchungsdatenfehler: {bookingsResult.Info}; Tageswertefehler: {dayValuesResult.Info}";
                    return result;
                }

                if (!bookingsResult.Success)
                {
                    _logger.LogWarning($"Buchungsdaten konnten nicht abgerufen werden: {bookingsResult.Info}");
                    result.Info += $"Buchungsdatenfehler: {bookingsResult.Info}; ";
                }
                else
                {
                    result.EmployeeData.Bookings = bookingsResult.Bookings;
                }

                if (!dayValuesResult.Success)
                {
                    _logger.LogWarning($"Tageswerte konnten nicht abgerufen werden: {dayValuesResult.Info}");
                    result.Info += $"Tageswertefehler: {dayValuesResult.Info}; ";
                }
                else
                {
                    result.EmployeeData.DayValues = dayValuesResult.DayValues;
                }

                if (string.IsNullOrEmpty(result.Info))
                {
                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }

        public async Task<GetEmployeeDayValuesWithBookingsAndTimeAccountsResult> GetEmployeeDayValuesWithBookingsAndTimeAccountsAsync(int employeeId, int? year, int? month)
        {
            var result = new GetEmployeeDayValuesWithBookingsAndTimeAccountsResult
            {
                Success = false,
                Info = string.Empty,
                Data = new List<DayValueWithBookingsAndTimeAccountsDto>()
            };

            try
            {
                _logger.LogInformation($"GetEmployeeDayValuesWithBookingsAndTimeAccountsAsync aufgerufen für Mitarbeiter ID: {employeeId}, Jahr: {year}, Monat: {month}.");

                // Berechnung von 'from' und 'to' basierend auf 'year' und 'month'
                DateTime fromDate;
                DateTime toDate;

                if (year.HasValue && month.HasValue)
                {
                    fromDate = new DateTime(year.Value, month.Value, 1);
                    toDate = fromDate.AddMonths(1).AddDays(-1);
                }
                else if (year.HasValue)
                {
                    fromDate = new DateTime(year.Value, 1, 1);
                    toDate = new DateTime(year.Value, 12, 31);
                }
                else
                {
                    fromDate = DateTime.MinValue;
                    toDate = DateTime.MaxValue;
                }

                // Parallel alle APIs aufrufen
                var dayValuesTask = GetEmployeeDayValuesAsync(employeeId, year, month);
                var bookingsTask = GetEmployeeBookingsAsync(employeeId, year, month);
                var timeAccountsTask = GetTimeAccountsAsync(fromDate, toDate, employeeId);
                var dayBookingsTask = GetEmployeeDayBookingsAsync(employeeId, year, month);

                await Task.WhenAll(dayValuesTask, bookingsTask, timeAccountsTask, dayBookingsTask);

                var dayValuesResult = dayValuesTask.Result;
                var bookingsResult = bookingsTask.Result;
                var timeAccountsResult = timeAccountsTask.Result;
                var dayBookingsResult = dayBookingsTask.Result;

                // Fehlerbehandlung
                if (!dayValuesResult.Success || !bookingsResult.Success || !timeAccountsResult.Success)
                {
                    if (!dayValuesResult.Success)
                        result.Info += $"Tageswertefehler: {dayValuesResult.Info}; ";
                    if (!bookingsResult.Success)
                        result.Info += $"Buchungsdatenfehler: {bookingsResult.Info}; ";
                    if (!timeAccountsResult.Success)
                        result.Info += $"Zeitkontenfehler: {timeAccountsResult.Info}; ";

                    return result;
                }

                if (dayValuesResult.DayValues == null || dayValuesResult.DayValues.Count == 0)
                    _logger.LogWarning("Keine Tageswerte für den Mitarbeiter gefunden.");
                if (bookingsResult.Bookings == null || bookingsResult.Bookings.Count == 0)
                    _logger.LogWarning("Keine Buchungen für den Mitarbeiter gefunden.");
                if (timeAccountsResult.TimeAccountEntries == null || timeAccountsResult.TimeAccountEntries.Count == 0)
                    _logger.LogWarning("Keine Zeitkonten-Einträge für den Mitarbeiter gefunden.");

                // Lookups für Buchungen, TimeAccounts und DayBookings nach Datum
                var bookingsLookup = bookingsResult.Bookings?
                    .GroupBy(b => b.Stamp.Date)
                    .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<DateTime, List<BookingDto>>();

                var timeAccountsLookup = timeAccountsResult.TimeAccountEntries?
                    .GroupBy(t => t.Date.Date)
                    .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<DateTime, List<TimeAccountEntryDto>>();

                var dayBookingsLookup = dayBookingsResult
                    .GroupBy(db => db.Date.Date)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault());

                _logger.LogInformation($"Anzahl der Tageswerte: {dayValuesResult.DayValues.Count}");
                _logger.LogInformation($"Anzahl der verschiedenen Buchungsdaten: {bookingsLookup.Count}");
                _logger.LogInformation($"Anzahl der verschiedenen Zeitkonten-Daten: {timeAccountsLookup.Count}");
                _logger.LogInformation($"Anzahl DayBookings (erster/letzter Stempel): {dayBookingsLookup.Count}");

                // Falls Jahr und Monat vorhanden, rufe zusätzlich die Monatsbilanz ab und erzeuge ein Dictionary mit Salden pro Tag.
                Dictionary<DateTime, double> monthlyDayBalances = null;
                if (year.HasValue && month.HasValue)
                {
                    var monthlyBalanceResult = await GetMonthlyOvertimeBalanceAsync(employeeId, year.Value, month.Value);
                    if (monthlyBalanceResult.Success && monthlyBalanceResult.Days != null)
                    {
                        // Dictionary: Datum -> AbsoluteSaldoHours (oder anderer Saldo, den Du verwenden möchtest)
                        monthlyDayBalances = monthlyBalanceResult.Days
                            .GroupBy(d => d.Date.Date)
                            .ToDictionary(g => g.Key, g => g.First().AbsoluteSaldoHours);
                        _logger.LogInformation("Monatliche Tages-Salden wurden abgerufen.");
                    }
                    else
                    {
                        _logger.LogWarning($"Monatsbilanz konnte nicht abgerufen werden: {monthlyBalanceResult.Info}");
                        result.Info += $"Monatsbilanzfehler: {monthlyBalanceResult.Info}; ";
                    }
                }

                // Verwende eine for-Schleife, um den Index zu kontrollieren und auf den nächsten Tag zugreifen zu können
                for (int i = 0; i < dayValuesResult.DayValues.Count; i++)
                {
                    var dayValue = dayValuesResult.DayValues[i];
                    var date = dayValue.Date.Date;

                    // Buchungen für diesen Tag
                    var bookingsForDay = bookingsLookup.ContainsKey(date) ? bookingsLookup[date] : new List<BookingDto>();
                    // Sortiere die Buchungen nach Zeitstempel
                    var sortedBookings = bookingsForDay.OrderBy(b => b.Stamp).ToList();

                    double totalDurationMinutesDouble = 0.0;
                    DateTime? lastInStamp = null;

                    // Liste der genutzten 'out'-Buchungen am nächsten Tag
                    BookingDto nextDayOutBooking = null;

                    foreach (var booking in sortedBookings)
                    {
                        if (booking.Type.Equals("in", StringComparison.OrdinalIgnoreCase))
                        {
                            if (lastInStamp.HasValue)
                            {
                                _logger.LogWarning($"Doppelte 'in'-Buchung ohne vorheriges 'out' für Mitarbeiter ID: {employeeId} am {date.ToShortDateString()}.");
                            }
                            lastInStamp = RoundDownToNearestMinute(booking.Stamp);
                        }
                        else if (booking.Type.Equals("out", StringComparison.OrdinalIgnoreCase))
                        {
                            if (lastInStamp.HasValue)
                            {
                                var roundedOutStamp = RoundDownToNearestMinute(booking.Stamp);
                                var duration = (roundedOutStamp - lastInStamp.Value).TotalMinutes;
                                if (duration > 0)
                                {
                                    totalDurationMinutesDouble += duration;
                                }
                                else
                                {
                                    _logger.LogWarning($"'out'-Buchung vor der 'in'-Buchung für Mitarbeiter ID: {employeeId} am {date.ToShortDateString()}.");
                                }
                                lastInStamp = null;
                            }
                            else
                            {
                                _logger.LogWarning($"'out'-Buchung ohne vorheriges 'in' für Mitarbeiter ID: {employeeId} am {date.ToShortDateString()}.");
                            }
                        }
                    }

                    // Überprüfe, ob ein 'in' ohne 'out' übrig geblieben ist
                    if (lastInStamp.HasValue)
                    {
                        // Prüfe, ob es einen nächsten Tag gibt
                        if (i + 1 < dayValuesResult.DayValues.Count)
                        {
                            var nextDayValue = dayValuesResult.DayValues[i + 1];
                            var nextDate = nextDayValue.Date.Date;

                            // Buchungen für den nächsten Tag
                            var bookingsNextDay = bookingsLookup.ContainsKey(nextDate) ? bookingsLookup[nextDate] : new List<BookingDto>();
                            var sortedNextDayBookings = bookingsNextDay.OrderBy(b => b.Stamp).ToList();

                            // Finde die erste 'out'-Buchung am nächsten Tag, die noch nicht verwendet wurde
                            nextDayOutBooking = sortedNextDayBookings.FirstOrDefault(b => b.Type.Equals("out", StringComparison.OrdinalIgnoreCase));

                            if (nextDayOutBooking != null)
                            {
                                var roundedOutStamp = RoundDownToNearestMinute(nextDayOutBooking.Stamp);
                                var duration = (roundedOutStamp - lastInStamp.Value).TotalMinutes;
                                if (duration > 0)
                                {
                                    totalDurationMinutesDouble += duration;
                                    _logger.LogInformation($"Verwende 'out'-Buchung vom nächsten Tag ({nextDate.ToShortDateString()}) für die 'in'-Buchung am {date.ToShortDateString()}.");

                               
                                    bookingsLookup[nextDate] = bookingsNextDay;
                                }
                                else
                                {
                                    _logger.LogWarning($"Verwendete 'out'-Buchung vom nächsten Tag liegt vor der 'in'-Buchung am {date.ToShortDateString()}.");
                                }
                                lastInStamp = null;
                            }
                            else
                            {
                                _logger.LogWarning($"Keine 'out'-Buchung am nächsten Tag gefunden für die 'in'-Buchung am {date.ToShortDateString()}.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Letztes 'in' ohne 'out' und kein nächster Tag vorhanden für Mitarbeiter ID: {employeeId} am {date.ToShortDateString()}.");
                        }
                    }

                    int totalDurationMinutes = (int)Math.Round(totalDurationMinutesDouble);
                    int durationMinutesWithoutPause = totalDurationMinutes - (dayValue.BreakDeductionMins / 60);
                    int nichtGewertet = totalDurationMinutes - durationMinutesWithoutPause;

                    BookingDto beginBooking = null;
                    BookingDto endBooking = null;
                    if (dayBookingsLookup.ContainsKey(date))
                    {
                        var dayBooking = dayBookingsLookup[date];
                        beginBooking = dayBooking.BeginBooking;
                        endBooking = dayBooking.EndBooking;
                    }

                    // Zeitkonten für diesen Tag
                    var timeAccountsForDay = timeAccountsLookup.ContainsKey(date) ? timeAccountsLookup[date] : new List<TimeAccountEntryDto>();

                    // Falls ein Tages-Saldo vorhanden ist (aus der Monatsbilanz), dann füge einen speziellen Eintrag hinzu.
                    if (monthlyDayBalances != null && monthlyDayBalances.ContainsKey(date))
                    {
                        var saldoEntry = new TimeAccountEntryDto
                        {
                            AccountId = 9999,
                            Date = date,
                            Value = monthlyDayBalances[date],
                            Multiplier = monthlyDayBalances[date] >= 0 ? "+" : "-",
                        };
                        timeAccountsForDay.Add(saldoEntry);
                    }

                    // Zusammenführen in ein DTO für den Tag
                    var combinedDto = new DayValueWithBookingsAndTimeAccountsDto
                    {
                        DayValue = dayValue,
                        Bookings = bookingsForDay,
                        TimeAccountEntries = timeAccountsForDay,
                        BeginBooking = beginBooking,
                        EndBooking = endBooking,
                        DurationMinutes = totalDurationMinutes,
                        DurationMinutesWithoutPause = durationMinutesWithoutPause,
                        NichtGewertet = nichtGewertet
                    };

                    // Beispiel: Bei bestimmten Statuswerten (z.B. "KR", "UR", "U2") Anpassungen vornehmen
                    if (dayValue.Status != null &&
                        (dayValue.Status.Short == "KR" || dayValue.Status.Short == "UR" || dayValue.Status.Short == "U2" || dayValue.Status.Short == "KAR" || dayValue.Status.Short == "PURL" || dayValue.Status.Short == "UU" || dayValue.Status.Short == "URh" || dayValue.Status.Short == "SF"))
                    {
                        dayValue.DebitMins = 0;
                        dayValue.DifferenceMins = 0;
                        combinedDto.DurationMinutesWithoutPause = 0;
                    }

                    result.Data.Add(combinedDto);
                }

                result.Success = true;
                _logger.LogInformation($"Kombinierte Tageswerte, Buchungsdaten und Zeitkonten-Daten für Mitarbeiter ID: {employeeId} erfolgreich abgerufen.");
                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP-Anfrage fehlgeschlagen.");
                result.Info = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");
                result.Info = ex.Message;
                return result;
            }
        }


        /// <summary>
        /// Rundet einen DateTime-Wert auf die nächste ganze Minute ab.
        /// </summary>
        /// <param name="dt">Der zu rundende DateTime-Wert.</param>
        /// <returns>Der abgerundete DateTime-Wert.</returns>
        private DateTime RoundDownToNearestMinute(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }


        /// <summary>
        /// Hilfsmethode zur Umwandlung eines Zeit-Strings in Double (Stunden).
        /// </summary>
        /// <param name="time">Zeit im Format "hh:mm" oder "-hh:mm".</param>
        /// <returns>Die Gesamtstunden als Double.</returns>
        private double ParseTimeToDouble(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                throw new FormatException("Zeit darf nicht leer sein.");

            bool isNegative = time.StartsWith("-");
            if (isNegative)
            {
                time = time.Substring(1);
            }

            var parts = time.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Ungültiges Zeitformat.");

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);

            double totalHours = hours + minutes / 60.0;

            return isNegative ? -totalHours : totalHours;
        }


        /// <summary>
        /// NEUE Methode: Ermittelt für jeden Tag die erste "in"-Buchung und die letzte "out"-Buchung.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters</param>
        /// <param name="year">Jahr zum Filtern (optional)</param>
        /// <param name="month">Monat zum Filtern (optional)</param>
        /// <returns>Liste von DayBookingDto pro Tag</returns>
        public async Task<List<DayBookingDto>> GetEmployeeDayBookingsAsync(int employeeId, int? year, int? month)
        {
            var dayBookingResult = new List<DayBookingDto>();

            // Rufe parallel die Tageswerte und Buchungen auf
            var dayValuesResult = await GetEmployeeDayValuesAsync(employeeId, year, month);
            var bookingsResult = await GetEmployeeBookingsAsync(employeeId, year, month);

            // Nur wenn beide Abfragen erfolgreich waren, weiterverarbeiten
            if (dayValuesResult.Success && bookingsResult.Success)
            {
                // Gehe jeden Tageswert durch
                foreach (var dayValue in dayValuesResult.DayValues)
                {
                    // Alle Buchungen, die auf diesem Datum liegen
                    var dayBookings = bookingsResult.Bookings
                        .Where(b => b.Stamp.Date == dayValue.Date.Date)
                        .OrderBy(b => b.Stamp)
                        .ToList();

                    // Finde die **erste** "in"-Buchung (chronologisch)
                    var firstInBooking = dayBookings
                        .Where(b => b.Type == "in")
                        .OrderBy(b => b.Stamp)
                        .FirstOrDefault();

                    // Finde die **letzte** "out"-Buchung (chronologisch)
                    var lastOutBooking = dayBookings
                        .Where(b => b.Type == "out")
                        .OrderBy(b => b.Stamp)
                        .LastOrDefault();

                    // Erstelle das DayBookingDto
                    var dayBookingDto = new DayBookingDto
                    {
                        Date = dayValue.Date,
                        BeginBooking = firstInBooking,
                        EndBooking = lastOutBooking
                    };

                    dayBookingResult.Add(dayBookingDto);
                }
            }
            else
            {
                // Ggf. Fehler ausgeben oder im Return verarbeiten
                _logger.LogWarning($"Tageswerte-Abfrage: {dayValuesResult.Info}");
                _logger.LogWarning($"Buchungs-Abfrage: {bookingsResult.Info}");
            }

            return dayBookingResult;
        }
        public async Task<MonthlyBalanceResult> GetMonthlyOvertimeBalanceAsync(
    int employeeId, int year, int month)
        {
            var result = new MonthlyBalanceResult
            {
                Success = false,
                Info = string.Empty,
                EmployeeId = employeeId,
                Year = year,
                Month = month
 
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // API-Aufruf: Liefert den Saldo von gestern (bzw. zum gestrigen Zeitpunkt)
                var response = await client.GetAsync($"/rest/web-api/employees/{employeeId}/balance");
                if (!response.IsSuccessStatusCode)
                {
                    string errorText = response.ReasonPhrase;
                    result.Info = $"HTTP-Fehler (balance): {(int)response.StatusCode} {errorText}";
                    return result;
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                var balanceJson = JObject.Parse(responseContent);

                // Der API-Aufruf liefert den Saldo von gestern in Sekunden.
                // Hier wird 60 Sekunden abgezogen.
                long dailyBalanceSec = balanceJson.Value<long>("dailyBalanceYesterday");
                // Umrechnung in Stunden (ohne Rundung)
                double dailyBalanceYesterdayHours = dailyBalanceSec / 3600.0;

                // ----------------------------------------------------
                // Zeitraum: ersten und letzten Tag des gewünschten Monats
                // ----------------------------------------------------
                DateTime firstOfMonth = new DateTime(year, month, 1);
                DateTime lastOfMonth = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                DateTime today = DateTime.Today;

                // Wenn der gewünschte Monat komplett in der Zukunft liegt, gibt es noch keine Daten
                if (today < firstOfMonth)
                {
                    result.Info = "Der angeforderte Monat liegt komplett in der Zukunft. Keine Daten bis heute.";
                    result.Success = true;
                    return result;
                }

                // Für den aktuellen Monat: den Endtag auf heute setzen (falls der Monatsabschluss noch in der Zukunft liegt)
                if (year == today.Year && month == today.Month && lastOfMonth > today)
                {
                    lastOfMonth = today;
                }

                // ----------------------------------------------------
                // Startsaldo-Anpassung: Berechnung des Saldos zum Start des Monats
                // ----------------------------------------------------
                // Wir wollen den Saldo ermitteln, wie er **am Tag vor** dem 1. des gewünschten Monats war.
                // Da uns der API-Aufruf den Saldo von gestern liefert, müssen wir alle Veränderungen (PlusMinus-Werte)
                // der Tage zwischen dem 1. des Monats und gestern (inklusive) ermitteln und rückwirkend abziehen.
                double startSaldoForMonth = dailyBalanceYesterdayHours;

                // Setze den Berechnungszeitraum: Vom ersten Tag des gewünschten Monats bis gestern.
                DateTime periodStart = firstOfMonth;
                // Wir gehen davon aus, dass 'gestern' immer vor heute liegt.
                DateTime periodEnd = today.AddDays(-1);

                // Hole alle TimeAccount-Entries für den Zeitraum [periodStart, periodEnd].
                var timeAccountsResult = await GetTimeAccountsAsync(periodStart, periodEnd, employeeId);
                if (!timeAccountsResult.Success)
                {
                    result.Info = "Fehler beim Abrufen der TimeAccounts: " + timeAccountsResult.Info;
                    return result;
                }

                // Filtere für den relevanten Account (z. B. AccountId = 65)
                var relevantEntries = timeAccountsResult.TimeAccountEntries
                    .Where(t => t.AccountId == 65)
                    .ToList();

                // Berechne die Summe der Plus/Minus-Werte für den Zeitraum ab dem 1. des Monats bis gestern.
                double deltaFromStartOfMonthToYesterday = relevantEntries
                    .Where(e => e.Date.Date >= periodStart && e.Date.Date <= periodEnd)
                    .Sum(e => e.Value);

                // Um den Saldo **zum Start des Monats** zu ermitteln, ziehen wir
                // die Veränderungen ab, die seit dem 1. des Monats bis gestern passiert sind.
                startSaldoForMonth = dailyBalanceYesterdayHours - deltaFromStartOfMonthToYesterday;

                // ----------------------------------------------------
                // Hier: Saldo vom Vormonat einfügen
                // ----------------------------------------------------
                // Der Startsaldo des gewünschten Monats entspricht dem Endsaldo des Vormonats.
                result.PreviousMonthFinalBalanceHours = startSaldoForMonth;

                // ----------------------------------------------------
                // Berechne den Tagessaldo für jeden Tag im gewünschten Monat
                // ----------------------------------------------------
                double runningSum = 0.0;
                // Hole die TimeAccounts für den Zeitraum des Monats
                var monthTimeAccountsResult = await GetTimeAccountsAsync(firstOfMonth, lastOfMonth, employeeId);
                if (!monthTimeAccountsResult.Success)
                {
                    result.Info = "Fehler beim Abrufen der TimeAccounts: " + monthTimeAccountsResult.Info;
                    return result;
                }

                // Filtere Einträge für den relevanten Account (z. B. AccountId = 65)
                var monthRelevantEntries = monthTimeAccountsResult.TimeAccountEntries
                    .Where(t => t.AccountId == 65)
                    .ToList();

                // Gruppiere die Einträge nach Tag
                var plusMinusByDay = monthRelevantEntries
                    .GroupBy(e => e.Date.Date)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

                // Berechne für jeden Tag im Monat die täglichen Veränderungen sowie den absoluten Saldo
                for (DateTime day = firstOfMonth; day <= lastOfMonth; day = day.AddDays(1))
                {
                    var dayDto = new MonthlyDayBalanceDto
                    {
                        Date = day
                    };

                    double plusMinusToday = plusMinusByDay.ContainsKey(day.Date) ? plusMinusByDay[day.Date] : 0.0;
                    dayDto.PlusMinusHours = plusMinusToday;

                    runningSum += plusMinusToday;

                    // Absoluter Saldo: Startsaldo zum Monatsbeginn + Laufender Summenstand
                    double absoluteSaldo = startSaldoForMonth + runningSum;
                    dayDto.AbsoluteSaldoHours = absoluteSaldo;

                    result.Days.Add(dayDto);
                }

                // Summe der Plus/Minus-Werte über den Monat
                result.SumMonthHours = runningSum;
                // Finaler Saldo am Ende des Monats
                result.FinalBalanceHours = startSaldoForMonth + runningSum;

                result.MonthlyTotalBalance = result.FinalBalanceHours;

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Info = ex.Message;
                return result;
            }
        }

        public async Task<double> GetDailyBalanceYesterdayAsync(int employeeId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(_apiSettings.BaseUrl);
                var byteArray = System.Text.Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // API-Aufruf: Liefert den Saldo von gestern (in Sekunden)
                var response = await client.GetAsync($"/rest/web-api/employees/{employeeId}/balance");
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"HTTP-Fehler (balance): {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                var balanceJson = JObject.Parse(responseContent);

                // Saldo von gestern in Sekunden
                long dailyBalanceSec = balanceJson.Value<long>("dailyBalanceYesterday");
                // Optional: Falls 60 Sekunden abgezogen werden sollen, könnte folgender Code verwendet werden:
                // dailyBalanceSec = Math.Max(dailyBalanceSec - 60, 0);

                // Umrechnung in Stunden (ohne Rundung)
                double dailyBalanceYesterdayHours = dailyBalanceSec / 3600.0;
                return dailyBalanceYesterdayHours;
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung: Hier kann alternativ auch ein Default-Wert zurückgegeben werden.
                throw new Exception("Fehler beim Abrufen des täglichen Saldos: " + ex.Message, ex);
            }
        }


    }


}

 