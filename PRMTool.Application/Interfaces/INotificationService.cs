using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PRMTool.Domain.Entities;

namespace PRMTool.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendTimesheetReminder1Async(User employee, DateTime weekStart);
        Task SendTimesheetReminder2Async(User employee, DateTime weekStart);
        Task SendTimesheetFreezeNotificationAsync(User employee, User manager, DateTime weekStart);
    }
}
