using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VeragTvApp.server.Models.Zeiterfassung
{
    public class ZeiterfassungModel
    {
    }

    public class TimasEmployee
    {
        public int ID { get; set; }
        public string ExternId { get; set; }
        public string Pnr1 { get; set; }
        public string Pnr2 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string ClientNumber { get; set; }
        public string RFID { get; set; }
        public int Card { get; set; }
        public string Info { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public bool LoginActive { get; set; }
        public string Password { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string BirthdayDate { get; set; }
        public string EntryDate { get; set; }
        public string ExitDate { get; set; }

        public List<TimasGroup> Groups { get; set; }
        public List<TimasCustomField> CustomFields { get; set; }

        public TimasEmployee()
        {
            Groups = new List<TimasGroup>();
            CustomFields = new List<TimasCustomField>();
        }
    }
 
public class TimasGroup
    {
        public int EmpID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string GroupType { get; set; }
        public string Info { get; set; }

        public TimasGroup(int empId)
        {
            EmpID = empId;
        }
    }

    public class TimasCustomField
    {
        public int EmpID { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public TimasCustomField(int empId)
        {
            EmpID = empId;
        }
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class cMitarbeiter
    {
        public string mit_id { get; set; }
        public string mit_PersonalNr { get; set; }
        public string mit_vname { get; set; }
        public string mit_nname { get; set; }
        public string mit_geschlecht { get; set; }
        public string mit_firma { get; set; }
        public string mit_abteilung { get; set; }
        public string mit_gebdat { get; set; }
        public string mit_einstiegsdatum { get; set; }
        public string mit_username { get; set; }
        public string mit_emailprivat { get; set; }
        public string mit_email { get; set; }
        public string mit_strasse { get; set; }
        public string mit_ort { get; set; }
        public string mit_plz { get; set; }
        public string mit_telefonnr { get; set; }
        public string mit_durchwahl { get; set; }
        public string mit_mobiltel { get; set; }
        public string mit_pwd { get; set; }
        public string mit_timasId { get; set; }
    }

    public class SetTimeAccountEntriesResult
    {
        public bool TimeEntryCreated { get; set; }
        public string Info { get; set; }
    }

    public class SetTimeAccountEntriesRequest
    {
        [Required]
        public int MitarbeiterID { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Es muss mindestens ein AccountEntry vorhanden sein.")]
        public List<AccountEntryDto> AccountEntrys { get; set; }

        [Required]
        public DateTime AccountDate { get; set; }
    }

    public class AccountEntryDto
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Value muss eine positive Zahl sein.")]
        public string Value { get; set; }
    }

    public class GetTimeAccountsRequest
    {
        [Required]
        public DateOnly From { get; set; }

        [Required]
        public DateOnly ToDate { get; set; }

        [Required]
        public int EmployeeNr { get; set; }
    }

    public class HolidayDto
    {
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public double Available { get; set; }
        public double Capping { get; set; }
        public double Correction { get; set; }
        public string HolidayType { get; set; }
        public double Old { get; set; }
        public double Planned { get; set; }
        public double SpecialClaim { get; set; }
        public double SummCorrCapp { get; set; }
        public double Taken { get; set; }
        public double TotalClaim { get; set; }
        public double TotalClaim2 { get; set; }
        public double YearClaim { get; set; }
        public int EmployeeID { get; set; }
        public double TakenPrevious { get; set; }
        public double AvaiblePrevious { get; set; }
        public double DifferenceTaken { get; set; }
        public double Aliqout { get; set; }
    }

    public class GetEmployeeHolidaysResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<HolidayDto> Holidays { get; set; }
    }

    public class GetEmployeeHolidaysRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Range(2000, 3000, ErrorMessage = "Bitte geben Sie ein gültiges Jahr ein.")]
        public int Year { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Bitte geben Sie einen gültigen Monat ein.")]
        public int Month { get; set; }
    }

    public class BookingDto
    {
        public long Id { get; set; }
        public DateTime Stamp { get; set; }
        public int EmployeeId { get; set; }
        public int Card { get; set; }
        public int StatusId { get; set; }
        public int StatusNr { get; set; }
        public string StatusName { get; set; }
        public string StatusShort { get; set; }
        public string StatusColor { get; set; }
        public string Type { get; set; }
        public string TypeShort { get; set; }
        public string TypeColor { get; set; }
        public string Source { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public string Ip { get; set; }

        public TerminalInfoDto Terminal { get; set; }
    }

    public class ShiftDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
        public string Color { get; set; }
    }

    public class DayValueDto
    {
        public DateTime Date { get; set; }
        public int ActualMins { get; set; }
        public int DebitMins { get; set; }
        public int ValuatedMins { get; set; }
        public int NotValuatedMins { get; set; }
        public int BreakDeductionMins { get; set; }
        public int DifferenceMins { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }
        public int ResultCode { get; set; }
        public StatusDto Status { get; set; }
        public ShiftDto Shift { get; set; }
        public string Workstation { get; set; }
    }

    public class StatusDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Short { get; set; }
        public string Color { get; set; }
    }

    public class EmployeeDataDto
    {
        public List<BookingDto> Bookings { get; set; }
        public List<DayValueDto> DayValues { get; set; }
        public ValuationFrameDto ValuationFrame { get; set; }
    }

    public class GetEmployeeDataResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public EmployeeDataDto EmployeeData { get; set; }
    }

    public class GetEmployeeBookingsResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<BookingDto> Bookings { get; set; }
    }

    public class GetEmployeeDayValuesResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<DayValueDto> DayValues { get; set; }
    }

    public class BookingWithDayValueDto
    {
        public BookingDto Booking { get; set; }
        public DayValueDto DayValue { get; set; }
    }

    public class GetEmployeeBookingsWithDayValuesResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<BookingWithDayValueDto> BookingsWithDayValues { get; set; }
    }

    public class DayValueWithBookingsDto
    {
        public DayValueDto DayValue { get; set; }
        public List<BookingDto> Bookings { get; set; }

        public DayValueWithBookingsDto()
        {
            Bookings = new List<BookingDto>();
        }
    }

    public class GetEmployeeDayValuesWithBookingsResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<DayValueWithBookingsDto> DayValuesWithBookings { get; set; }

        public GetEmployeeDayValuesWithBookingsResult()
        {
            DayValuesWithBookings = new List<DayValueWithBookingsDto>();
        }
    }

    public class DayValueWithBookingsAndTimeAccountsDto
    {
        public DayValueDto DayValue { get; set; }
        public List<BookingDto> Bookings { get; set; }
        public List<TimeAccountEntryDto> TimeAccountEntries { get; set; }
        public ValuationFrameDto ValuationFrame { get; set; }

        public BookingDto BeginBooking { get; set; }
        public BookingDto EndBooking { get; set; }
 
        public int? DurationMinutes { get; set; }
        public DayValueDto? BreakDeductionMins { get; set; }
        public int? DurationMinutesWithoutPause { get; set; }
        public int? NichtGewertet { get; set; }

        public DayValueWithBookingsAndTimeAccountsDto()
        {
            Bookings = new List<BookingDto>();
            TimeAccountEntries = new List<TimeAccountEntryDto>();
        }
    }



    public class GetEmployeeDayValuesWithBookingsAndTimeAccountsResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<DayValueWithBookingsAndTimeAccountsDto> Data { get; set; }
        public ValuationFrameDto ValuationFrame { get; set; }

        // Neues Property für die Monatsbilanz
        public MonthlyBalanceResult MonthlyOvertimeBalance { get; set; }

        public GetEmployeeDayValuesWithBookingsAndTimeAccountsResult()
        {
            Data = new List<DayValueWithBookingsAndTimeAccountsDto>();
        }
    }

    public class GetTimeAccountsResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }
        public List<TimeAccountEntryDto> TimeAccountEntries { get; set; }
    }

    public class TimeAccountEntryDto
    {
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string Multiplier { get; set; }
    }

    public class TerminalInfoDto
    {
        public int Id { get; set; }
        public int Tid { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
    }

    public class ValuationFrameDto
    {
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }

    public class DayBookingDto
    {
        public DateTime Date { get; set; }
        public BookingDto BeginBooking { get; set; }
        public BookingDto EndBooking { get; set; }
    }

    public class MonthlyBalanceResult
    {
        public bool Success { get; set; }
        public string Info { get; set; }

        public int EmployeeId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        /// <summary>
        /// dailybalanceyesterday in Millisekunden
        /// </summary>
        public long DailyBalanceYesterdayMs { get; set; }

        /// <summary>
        /// dailybalanceyesterday umgerechnet in dezimale Stunden
        /// </summary>
        public double DailyBalanceYesterdayHours { get; set; }

        /// <summary>
        /// Liste mit Tageswerten in diesem Monat (bis max. heute)
        /// </summary>
        public List<MonthlyDayBalanceDto> Days { get; set; } = new List<MonthlyDayBalanceDto>();

        /// <summary>
        /// Summe nur der in diesem Monat gemeldeten Plus/Minus-Stunden
        /// (accountId=65) – also Differenzsumme
        /// </summary>
        public double SumMonthHours { get; set; }

        /// <summary>
        /// Neuer Endsaldo: DailyBalanceYesterdayHours + SumMonthHours
        /// </summary>
        public double FinalBalanceHours { get; set; }

        /// <summary>
        /// Monatsendbilanz: Endsaldo des Vormonats, also der Stand am Tag vor dem 1. des gewählten Monats.
        /// Dieser Wert wird zumeist als Startsaldo des aktuellen Monats genutzt.
        /// </summary>
        public double PreviousMonthFinalBalanceHours { get; set; }

        public double MonthlyTotalBalance { get; set; }
    }

    public class MonthlyDayBalanceDto
{
    public DateTime Date { get; set; }

    /// <summary>
    /// Plus-/Minusstunden an diesem Tag (z. B. -1.5 oder +0.82)
    /// </summary>
    public double PlusMinusHours { get; set; }

    /// <summary>
    /// Absoluter Saldo = 
    /// (DailyBalanceYesterdayHours) + 
    /// (Summe aller Plus/Minus bis einschließlich dieses Tages)
    /// </summary>
    public double AbsoluteSaldoHours { get; set; }
}

}
