namespace Sunspot.EFCore.Interface;

/// <summary>
/// 实体管理接口
/// </summary>
public interface IEntityManager
{
    void Initialize();

    IEntityRegister[] GetEntityRegisters(Type dbContextType);

    Type GetDbContextTypeByEntity(Type entityType);

    Type GetDbContextTypeByName(string dbContextTypeName);
}