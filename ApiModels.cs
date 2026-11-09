namespace RaceDayMVC.Models;
public class ApiUser { public int UserID {get;set;} public string FirstName {get;set;}=""; public string LastName {get;set;}=""; public string Email {get;set;}=""; public string Role {get;set;}=""; }
public class ApiEvent { public int EventID {get;set;} public int OrganiserID {get;set;} public string EventName {get;set;}=""; public string Description {get;set;}=""; public DateTime EventDate {get;set;} public TimeSpan StartTime {get;set;} public string Location {get;set;}=""; public string Status {get;set;}="Upcoming"; public List<ApiCategory> Categories {get;set;}=new(); }
public class ApiCategory { public int CategoryID {get;set;} public int EventID {get;set;} public string CategoryName {get;set;}=""; public decimal DistanceKm {get;set;} public decimal EntryFee {get;set;} public int MaxParticipants {get;set;} }
public class ApiEnrollment { public int EnrollmentID {get;set;} public int ParticipantID {get;set;} public int CategoryID {get;set;} public DateTime EnrollmentDate {get;set;} public string RaceNumber {get;set;}=""; public string Status {get;set;}=""; public ApiCategory? Category {get;set;} public ApiResult? Result {get;set;} }
public class ApiResult { public int ResultID {get;set;} public int EnrollmentID {get;set;} public TimeSpan? FinishTime {get;set;} public int? Position {get;set;} public string ResultStatus {get;set;}=""; }
public class LoginVM { public string Email {get;set;}=""; public string Password {get;set;}=""; }
public class RegisterVM
{
    public string FirstName {get;set;}="";
    public string LastName {get;set;}="";
    public string Email {get;set;}="";
    public string Password {get;set;}="";
    public string Role {get;set;}="Participant";
    public string Phone {get;set;}="";
    public string Organisation {get;set;}="";
    public DateTime? DateOfBirth {get;set;}
    public string Gender {get;set;}="";
    public string EmergencyContact {get;set;}="";
    public string EmergencyPhone {get;set;}="";
}
public class EventVM { public string EventName {get;set;}=""; public string Description {get;set;}=""; public DateTime EventDate {get;set;}=DateTime.Today; public TimeSpan StartTime {get;set;}=new(9,0,0); public string Location {get;set;}=""; public string Status {get;set;}="Upcoming"; }
public class CategoryVM { public int EventID {get;set;} public string CategoryName {get;set;}=""; public decimal DistanceKm {get;set;} public decimal EntryFee {get;set;} public int MaxParticipants {get;set;} }
public class EnrolVM { public int CategoryID {get;set;} }
