
namespace Cqrs.Infrastructure
{
    /// <summary>
    /// 对象生命周期调用
    /// </summary>
    public interface ILifecycle
    {
        /// <summary>
        /// Insert前回调
        /// </summary>
        LifecycleVeto OnSaving(object context);
        /// <summary>
        /// Update前回调
        /// </summary>
        LifecycleVeto OnUpdating(object context);
        /// <summary>
        /// Delete前回调
        /// </summary>
        LifecycleVeto OnDeleting(object context);
        /// <summary>
        /// Load后回调
        /// </summary>
        void OnLoaded(object context);
    }
}
