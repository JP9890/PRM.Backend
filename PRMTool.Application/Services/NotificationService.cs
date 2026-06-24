using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;

namespace PRMTool.Application.Services
{
    /// <summary>
    /// Notification service that logs to the Serilog/ILogger output.
    /// To plug in real email (SMTP / SendGrid), replace the _logger calls
    /// with your email client calls in each method below.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendTimesheetReminder1Async(User employee, DateTime weekStart)
        {
            _logger.LogInformation(
                "[NOTIFICATION - REMINDER 1] TO: {Email} ({Name}) | " +
                "SUBJECT: Action Required: Timesheet Missing for week of {Week} | " +
                "BODY: Hi {Name}, your timesheet for the week starting {Week} has not been submitted. " +
                "Please submit it as soon as possible to avoid account restrictions.",
                employee.Email, employee.FullName,
                weekStart.ToString("dd MMM yyyy"),
                employee.FullName,
                weekStart.ToString("dd MMM yyyy"));

            // TODO: Replace above with real SMTP call:
            // await _smtpClient.SendAsync(new MailMessage { To = employee.Email, ... });

            return Task.CompletedTask;
        }

        public Task SendTimesheetReminder2Async(User employee, DateTime weekStart)
        {
            _logger.LogWarning(
                "[NOTIFICATION - REMINDER 2 - FINAL WARNING] TO: {Email} ({Name}) | " +
                "SUBJECT: Final Warning: Timesheet Missing for week of {Week} | " +
                "BODY: Hi {Name}, this is your final reminder. Your timesheet for the week of {Week} is still missing. " +
                "If not submitted today, your timesheet submission access will be suspended.",
                employee.Email, employee.FullName,
                weekStart.ToString("dd MMM yyyy"),
                employee.FullName,
                weekStart.ToString("dd MMM yyyy"));

            return Task.CompletedTask;
        }

        public Task SendTimesheetFreezeNotificationAsync(User employee, User manager, DateTime weekStart)
        {
            _logger.LogError(
                "[NOTIFICATION - ACCOUNT FREEZE] " +
                "EMPLOYEE EMAIL TO: {EmpEmail} ({EmpName}) | " +
                "MANAGER EMAIL TO: {MgrEmail} ({MgrName}) | " +
                "SUBJECT: Timesheet Access Suspended | " +
                "BODY: {EmpName}'s timesheet submission access has been suspended due to a missing timesheet " +
                "for the week of {Week}. Manager {MgrName} can restore access from the Team Timesheets dashboard.",
                employee.Email, employee.FullName,
                manager.Email, manager.FullName,
                employee.FullName,
                weekStart.ToString("dd MMM yyyy"),
                manager.FullName);

            return Task.CompletedTask;
        }
    }
}
