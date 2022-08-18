using Microsoft.EntityFrameworkCore;

namespace Sunspot.EFCore.Interface;

/// <summary>
/// 实体注册接口
/// </summary>
public interface IEntityRegister
{
    Type DbContextType { get; }

    Type EntityType { get; }

    void RegisterTo(ModelBuilder modelBuilder);
    
}