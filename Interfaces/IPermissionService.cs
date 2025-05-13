namespace backend.Interfaces;

public interface IPermissionService
{
    bool HasPermission(string permissionName);
    Task<bool> CheckPermissionAsync(int adminId, string permissionName);
}
