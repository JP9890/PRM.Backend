using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IAllocationRepository
    {
        Task<IEnumerable<Allocation>> GetAllActiveAsync(DateTime? asOfDate = null);
        Task<IEnumerable<Allocation>> GetActiveByResourceIdAsync(int resourceId, DateTime? asOfDate = null);
        Task<IEnumerable<Allocation>> GetActiveByProjectIdAsync(int projectId);
        Task<Allocation?> GetByIdAsync(int id);
        Task AddAsync(Allocation allocation);
        Task UpdateAsync(Allocation allocation);
    }
}
