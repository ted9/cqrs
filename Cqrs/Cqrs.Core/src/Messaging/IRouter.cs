
namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示继承该接口的类型支持路由规则。
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// 获取路由值。
        /// </summary>
        string GetRoutingKey();
    }
}
