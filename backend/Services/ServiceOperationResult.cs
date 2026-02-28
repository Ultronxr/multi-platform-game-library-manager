namespace GameLibrary.Api.Services;

/// <summary>
/// 无返回数据的服务操作结果。
/// </summary>
public sealed record ServiceOperationResult(
    bool IsSuccess,
    int StatusCode,
    string? Message)
{
    /// <summary>
    /// 创建成功结果。
    /// </summary>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="message">提示信息。</param>
    /// <returns>成功结果。</returns>
    public static ServiceOperationResult Success(int statusCode = StatusCodes.Status200OK, string? message = null) =>
        new(true, statusCode, message);

    /// <summary>
    /// 创建失败结果。
    /// </summary>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="message">错误信息。</param>
    /// <returns>失败结果。</returns>
    public static ServiceOperationResult Failure(int statusCode, string message) =>
        new(false, statusCode, message);
}

/// <summary>
/// 带返回数据的服务操作结果。
/// </summary>
/// <typeparam name="TData">返回数据类型。</typeparam>
public sealed record ServiceOperationResult<TData>(
    bool IsSuccess,
    int StatusCode,
    TData? Data,
    string? Message)
{
    /// <summary>
    /// 创建成功结果。
    /// </summary>
    /// <param name="data">返回数据。</param>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="message">提示信息。</param>
    /// <returns>成功结果。</returns>
    public static ServiceOperationResult<TData> Success(
        TData data,
        int statusCode = StatusCodes.Status200OK,
        string? message = null) =>
        new(true, statusCode, data, message);

    /// <summary>
    /// 创建失败结果。
    /// </summary>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="message">错误信息。</param>
    /// <returns>失败结果。</returns>
    public static ServiceOperationResult<TData> Failure(int statusCode, string message) =>
        new(false, statusCode, default, message);
}
