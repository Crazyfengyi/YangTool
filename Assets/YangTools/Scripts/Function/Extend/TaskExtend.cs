using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

/// <summary>
/// Task 和 UniTask 的取消扩展
/// </summary>
public static class TaskExtensions
{
    #region Task

    /// <summary>
    /// 为不支持取消的泛型 Task 添加取消能力
    /// </summary>
    /// <typeparam name="T">任务返回值类型</typeparam>
    /// <param name="task">待等待的任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>原任务的返回值</returns>
    /// <exception cref="ArgumentNullException">任务为空时抛出</exception>
    /// <exception cref="OperationCanceledException">取消令牌触发时抛出</exception>
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        ValidateTask(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!cancellationToken.CanBeCanceled)
        {
            return await task;
        }

        TaskCompletionSource<bool> cancellationSource = CreateCancellationSource(
            cancellationToken,
            out CancellationTokenRegistration registration);

        try
        {
            Task completedTask = await Task.WhenAny(task, cancellationSource.Task);
            if (completedTask != task)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            return await task;
        }
        finally
        {
            registration.Dispose();
        }
    }

    /// <summary>
    /// 为不支持取消的非泛型 Task 添加取消能力
    /// </summary>
    /// <param name="task">待等待的任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可等待的任务</returns>
    /// <exception cref="ArgumentNullException">任务为空时抛出</exception>
    /// <exception cref="OperationCanceledException">取消令牌触发时抛出</exception>
    public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {
        ValidateTask(task);
        cancellationToken.ThrowIfCancellationRequested();

        if (!cancellationToken.CanBeCanceled)
        {
            await task;
            return;
        }

        TaskCompletionSource<bool> cancellationSource = CreateCancellationSource(
            cancellationToken,
            out CancellationTokenRegistration registration);

        try
        {
            Task completedTask = await Task.WhenAny(task, cancellationSource.Task);
            if (completedTask != task)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            await task;
        }
        finally
        {
            registration.Dispose();
        }
    }

    #endregion

    #region UniTask

    /// <summary>
    /// 为不支持取消的泛型 UniTask 添加取消能力
    /// </summary>
    /// <typeparam name="T">任务返回值类型</typeparam>
    /// <param name="task">待等待的任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>原任务的返回值</returns>
    /// <exception cref="OperationCanceledException">取消令牌触发时抛出</exception>
    public static async UniTask<T> WithCancellation<T>(this UniTask<T> task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!cancellationToken.CanBeCanceled)
        {
            return await task;
        }

        (bool taskCompleted, T result) waitResult = await UniTask.WhenAny(
            task,
            UniTask.WaitUntilCanceled(cancellationToken));

        if (waitResult.taskCompleted)
        {
            return waitResult.result;
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new OperationCanceledException(cancellationToken);
    }

    /// <summary>
    /// 为不支持取消的非泛型 UniTask 添加取消能力
    /// </summary>
    /// <param name="task">待等待的任务</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可等待的任务</returns>
    /// <exception cref="OperationCanceledException">取消令牌触发时抛出</exception>
    public static async UniTask WithCancellation(this UniTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!cancellationToken.CanBeCanceled)
        {
            await task;
            return;
        }

        int winnerIndex = await UniTask.WhenAny(
            task,
            UniTask.WaitUntilCanceled(cancellationToken));

        if (winnerIndex == 0)
        {
            await task;
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new OperationCanceledException(cancellationToken);
    }

    #endregion

    #region Other

    /// <summary>
    /// 创建用于等待取消通知的任务源
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="registration">取消回调注册</param>
    /// <returns>取消任务源</returns>
    private static TaskCompletionSource<bool> CreateCancellationSource(
        CancellationToken cancellationToken,
        out CancellationTokenRegistration registration)
    {
        TaskCompletionSource<bool> source = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        registration = cancellationToken.Register(CompleteCancellationSource, source);
        return source;
    }

    /// <summary>
    /// 完成取消任务源
    /// </summary>
    /// <param name="state">取消任务源状态</param>
    private static void CompleteCancellationSource(object state)
    {
        ((TaskCompletionSource<bool>)state).TrySetResult(true);
    }

    /// <summary>
    /// 检查泛型 Task 是否有效
    /// </summary>
    /// <typeparam name="T">任务返回值类型</typeparam>
    /// <param name="task">待检查的任务</param>
    /// <exception cref="ArgumentNullException">任务为空时抛出</exception>
    private static void ValidateTask<T>(Task<T> task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }
    }

    /// <summary>
    /// 检查非泛型 Task 是否有效
    /// </summary>
    /// <param name="task">待检查的任务</param>
    /// <exception cref="ArgumentNullException">任务为空时抛出</exception>
    private static void ValidateTask(Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }
    }
    
    #endregion
}

/*
使用示例

Task 示例
private async Task LoadDataAsync(CancellationToken cancellationToken)
{
    await LoadFromServerAsync().WithCancellation(cancellationToken);
}

UniTask 示例
private async UniTask<int> GetDataAsync(CancellationToken cancellationToken)
{
    return await GetValueAsync().WithCancellation(cancellationToken);
}

取消操作示例
using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
Task loadTask = LoadFromServerAsync();
await loadTask.WithCancellation(cancellationTokenSource.Token);
cancellationTokenSource.Cancel();
*/
