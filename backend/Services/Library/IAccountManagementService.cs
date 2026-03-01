using GameLibrary.Api.Models;

namespace GameLibrary.Api.Services;

/// <summary>
/// 平台账号管理服务。
/// </summary>
public interface IAccountManagementService
{
    /// <summary>
    /// 使用已保存凭证重新拉取指定账号的库存。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>同步结果（返回同步数量）。</returns>
    Task<ServiceOperationResult<int>> ResyncSavedAccountAsync(long accountId, CancellationToken cancellationToken);

    /// <summary>
    /// 更新指定账号基础信息与凭证。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="request">更新请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    Task<ServiceOperationResult> UpdateSavedAccountAsync(
        long accountId,
        UpdateSavedAccountRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 删除指定账号及其关联库存。
    /// </summary>
    /// <param name="accountId">账号主键。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>操作结果。</returns>
    Task<ServiceOperationResult> DeleteSavedAccountAsync(long accountId, CancellationToken cancellationToken);
}
