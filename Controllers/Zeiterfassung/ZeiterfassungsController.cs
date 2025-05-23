using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;
using VeragTvApp.server.Services;
using System.Collections.Generic;
using VeragTvApp.server.Models.Zeiterfassung;  

using Microsoft.AspNetCore.Authorization;
 
namespace VeragTvApp.server.Controllers.Zeiterfassung
{
 
    [Route("api/[controller]")]
    public class ZeiterfassungController : ControllerBase
    {
        private readonly ZeiterfassungServices _zeiterfassungServices;

        public ZeiterfassungController(ZeiterfassungServices zeiterfassungServices)
        {
            _zeiterfassungServices = zeiterfassungServices;
        }

        /// <summary>
        /// Ruft die Liste der Mitarbeiter ab und gibt sie als JSON zurück.
        /// </summary>
        /// <returns>Liste der Mitarbeiter</returns>
        [HttpGet("GetEmployeeList")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetEmployeeList()
        {
            DataTable dt = new DataTable();
            string result = await _zeiterfassungServices.GetEmployeeListAsync(dt);

            if (result != "Success")
            {
                return BadRequest(new { error = result });
            }

            // Konvertieren der DataTable in eine Liste von Objekten
            var employees = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                employees.Add(new
                {
                    Id = row["Id"],
                    PersonalNr = row["Personal-Nr (Lohn)"],
                    Vorname = row["Vorname"],
                    Nachname = row["Nachname"],
                    Geschlecht = row["Geschlecht"],
                    AusweisNr = row["Ausweis-Nr"],
                    Info = row["Info"]
                });
            }

            return Ok(employees);
        }

        [HttpGet("GetEmployeeHolidayList")]
        [Authorize(Roles = "admin")]
            public async Task<IActionResult> GetEmployeeHolidayListAsync([FromQuery] string groupName, [FromQuery] int month, [FromQuery] int year)
        {
            // Übergibt die Query-Parameter an die Service-Methode
            string result = await _zeiterfassungServices.GetEmployeeHolidayListAsync(groupName, month, year);
            return Ok(result);
        }



        /// <summary>
        /// Ruft einen einzelnen Mitarbeiter anhand der Mitarbeiter-ID ab und gibt ihn als JSON zurück.
        /// </summary>
        /// <param name="id">Die ID des Mitarbeiters.</param>
        /// <returns>Ein einzelner Mitarbeiter oder ein Fehler.</returns>
        [HttpGet("GetEmployee/{id}")]
        public async Task<IActionResult> GetEmployee(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { error = "Mitarbeiter-ID darf nicht leer sein." });
            }

            TimasEmployee employee = await _zeiterfassungServices.GetEmployeeAsync(id);

            if (employee == null)
            {
                return NotFound(new { error = "Mitarbeiter nicht gefunden oder ein Fehler ist aufgetreten." });
            }

            // Optional: Mapping zu einem DTO, falls Sie nicht direkt das Modell zurückgeben möchten
            var employeeDto = new TimasEmployee
            {
                ID = employee.ID,
                ExternId = employee.ExternId,
                Pnr1 = employee.Pnr1,
                Pnr2 = employee.Pnr2,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Gender = employee.Gender,
                ClientNumber = employee.ClientNumber,
                Card = employee.Card,
                Info = employee.Info,
                RFID = employee.RFID,
                Login = employee.Login,
                Email = employee.Email,
                LoginActive = employee.LoginActive,
                Password = employee.Password, 
                Street = employee.Street,
                Zipcode = employee.Zipcode,
                City = employee.City,
                Phone1 = employee.Phone1,
                Phone2 = employee.Phone2,
                BirthdayDate = employee.BirthdayDate,
                EntryDate = employee.EntryDate,
                ExitDate = employee.ExitDate,
                Groups = employee.Groups.Select(g => new TimasGroup(g.EmpID)
                {
                    Id = g.Id,
                    Name = g.Name, // Falls verfügbar
                    GroupType = g.GroupType, // Falls verfügbar
                    Info = g.Info // Falls verfügbar
                }).ToList(),
                CustomFields = employee.CustomFields.Select(cf => new TimasCustomField(cf.EmpID)
                {
                    Id = cf.Id,
                    Name = cf.Name,
                    Type = cf.Type,
                    Value = cf.Value
                }).ToList()
            };

            return Ok(employeeDto);
        }
 
 

        /// <summary>
        /// Setzt Zeitkonten-Einträge für einen Mitarbeiter.
        /// </summary>
        /// <param name="request">Das Anfrageobjekt mit Mitarbeiter und Einträgen.</param>
        /// <returns>Erfolg oder Fehlerinformation.</returns>
        [HttpPost("SetTimeAccountEntries")]
        public async Task<IActionResult> SetTimeAccountEntries([FromBody] SetTimeAccountEntriesRequest request)
        {
            if (request == null || request.MitarbeiterID <= 0 || request.AccountEntrys == null || request.AccountEntrys.Count == 0 || request.AccountDate == default)
            {
                 return BadRequest(new { error = "Ungültige Daten." });
            }

 
            SetTimeAccountEntriesResult result = await _zeiterfassungServices.SetTimeAccountEntriesAsync(request.MitarbeiterID, request.AccountEntrys, request.AccountDate);

            if (result.TimeEntryCreated)
            {
                 return Ok(new { success = "Zeitkonten-Einträge erfolgreich erstellt." });
            }
            else
            {
                 return BadRequest(new { error = result.Info });
            }
        }
        /// <summary>
        /// Ruft Zeitkonten-Einträge für einen Mitarbeiter ab.
        /// </summary>
        /// <param name="employeeNr">Die Mitarbeiternummer.</param>
        /// <param name="from">Startdatum (yyyy-MM-dd).</param>
        /// <param name="toDate">Enddatum (yyyy-MM-dd).</param>
        /// <returns>Liste der Zeitkonten-Einträge oder ein Fehler.</returns>
        [HttpGet("GetTimeAccounts")]
        public async Task<IActionResult> GetTimeAccounts([FromQuery] int employeeNr, [FromQuery] DateTime from, [FromQuery] DateTime toDate)
        {
            if (employeeNr <= 0 || from == default || toDate == default)
            {
                return BadRequest(new { error = "Ungültige Daten." });
            }

            GetTimeAccountsResult result = await _zeiterfassungServices.GetTimeAccountsAsync(from, toDate, employeeNr);

            if (result.Success)
            {
                return Ok(result.TimeAccountEntries);
            }
            else
            {
                return BadRequest(new { error = result.Info });
            }
        }
        /// <summary>
        /// Ruft Urlaubsdaten für einen Mitarbeiter ab.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr für die Urlaubsdaten.</param>
        /// <param name="month">Der Monat für die Urlaubsdaten.</param>
        /// <returns>Liste der Urlaubsdaten oder ein Fehler.</returns>
        [HttpGet("GetEmployeeHolidays")]
        public async Task<IActionResult> GetEmployeeHolidays([FromQuery] int employeeId, [FromQuery] int year, [FromQuery] int month)
        {
            if (employeeId <= 0 || year < 2000 || year > 3000 || month < 1 || month > 12)
            {
                return BadRequest(new { error = "Ungültige Daten." });
            }

            GetEmployeeHolidaysResult result = await _zeiterfassungServices.GetEmployeeHolidaysAsync(employeeId, year, month);

            if (result.Success)
            {
                return Ok(result.Holidays);
            }
            else
            {
                return BadRequest(new { error = result.Info });
            }
        }


        /// <summary>
        /// Ruft die Buchungen eines Mitarbeiters anhand der Mitarbeiter-ID ab und gibt sie als JSON zurück.
        /// Optional können Jahr und Monat als Query-Parameter angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="id">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr, nach dem gefiltert werden soll (optional).</param>
        /// <param name="month">Der Monat, nach dem gefiltert werden soll (optional).</param>
        /// <returns>Liste der Buchungen oder ein Fehler.</returns>
        [HttpGet("GetEmployeeBookings/{id}")]
        public async Task<IActionResult> GetEmployeeBookings(int id, [FromQuery] int? year, [FromQuery] int? month)
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Ungültige Mitarbeiter-ID." });
            }

            // Validierung der Query-Parameter, falls sie angegeben sind
            if (year.HasValue && (year < 2000 || year > 3000))
            {
                return BadRequest(new { error = "Ungültiges Jahr angegeben." });
            }

            if (month.HasValue && (month < 1 || month > 12))
            {
                return BadRequest(new { error = "Ungültiger Monat angegeben." });
            }

            var bookingsResult = await _zeiterfassungServices.GetEmployeeBookingsAsync(id, year, month);

            if (!bookingsResult.Success)
            {
                return BadRequest(new { error = bookingsResult.Info });
            }

            return Ok(bookingsResult.Bookings);
        }

        /// <summary>
        /// Ruft kombinierte Daten (Tageswerte, Buchungen und Zeitkonten-Einträge) eines Mitarbeiters ab.
        /// Optional können Jahr und Monat als Query-Parameter angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr zur Filterung (optional).</param>
        /// <param name="month">Der Monat zur Filterung (optional).</param>
        /// <returns>Kombinierte Daten der Tageswerte, Buchungen und Zeitkonten-Einträge oder ein Fehler.</returns>
        [HttpGet("GetEmployeeData/{employeeId}")]
        public async Task<IActionResult> GetEmployeeData(int employeeId, [FromQuery] int? year, [FromQuery] int? month)
        {
            // Validierung der Mitarbeiter-ID
            if (employeeId <= 0)
            {
                return BadRequest(new { error = "Ungültige Mitarbeiter-ID." });
            }

            // Validierung des Jahres, falls angegeben
            if (year.HasValue && (year < 2000 || year > 3000))
            {
                return BadRequest(new { error = "Ungültiges Jahr angegeben." });
            }

            // Validierung des Monats, falls angegeben
            if (month.HasValue && (month < 1 || month > 12))
            {
                return BadRequest(new { error = "Ungültiger Monat angegeben." });
            }

            // Aufruf der kombinierten Service-Methode
            var dataResult = await _zeiterfassungServices.GetEmployeeDayValuesWithBookingsAndTimeAccountsAsync(employeeId, year, month);

            // Überprüfung des Service-Ergebnisses
            if (dataResult.Success)
            {
                return Ok(dataResult.Data);
            }
            else
            {
                return BadRequest(new { error = dataResult.Info });
            }
        }


        

        /// <summary>
        /// Ruft die Tageswerte eines Mitarbeiters anhand der Mitarbeiter-ID ab.
        /// Optional können Jahr und Monat als Query-Parameter angegeben werden, um die Daten zu filtern.
        /// </summary>
        /// <param name="employeeId">Die ID des Mitarbeiters.</param>
        /// <param name="year">Das Jahr zur Filterung (optional).</param>
        /// <param name="month">Der Monat zur Filterung (optional).</param>
        /// <returns>Liste der Tageswerte oder ein Fehler.</returns>
        [HttpGet("GetEmployeeDayValues/{employeeId}")]
        public async Task<IActionResult> GetEmployeeDayValues(int employeeId, [FromQuery] int? year, [FromQuery] int? month)
        {
            // Validierung der Mitarbeiter-ID
            if (employeeId <= 0)
            {
                return BadRequest(new { error = "Ungültige Mitarbeiter-ID." });
            }

            // Validierung des Jahres, falls angegeben
            if (year.HasValue && (year < 2000 || year > 3000))
            {
                return BadRequest(new { error = "Ungültiges Jahr angegeben." });
            }

            // Validierung des Monats, falls angegeben
            if (month.HasValue && (month < 1 || month > 12))
            {
                return BadRequest(new { error = "Ungültiger Monat angegeben." });
            }

            // Aufruf des Service-Methods
            GetEmployeeDayValuesResult result = await _zeiterfassungServices.GetEmployeeDayValuesAsync(employeeId, year, month);

            // Überprüfung des Service-Ergebnisses
            if (result.Success)
            {
                return Ok(result.DayValues);
            }
            else
            {
                return BadRequest(new { error = result.Info });
            }
        }

        
            [HttpGet("{employeeId}/monthly-balance")]
            public async Task<IActionResult> GetMonthlyBalance(int employeeId, [FromQuery] int year, [FromQuery] int month)
            {
                var result = await _zeiterfassungServices.GetMonthlyOvertimeBalanceAsync(employeeId, year, month);

                if (!result.Success)
                {
                    // Du kannst hier z. B. einen entsprechenden Statuscode setzen
                    return BadRequest(new { error = result.Info });
                }

                return Ok(result);
            }
        }

    }

  