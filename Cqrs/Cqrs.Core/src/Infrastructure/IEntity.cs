using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cqrs.Infrastructure
{
    /// <summary>
    /// 表示继承该接口的类型是一个实体。
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 主键标识
        /// </summary>
        object Id { get; }
    }

    /// <summary>
    /// <see cref="IEntity"/> 的扩展类。
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// 验证模型的正确性
        /// </summary>
        public static bool IsValid(this IEntity entity, out IEnumerable<ModelValidationResult> errors)
        {
            errors = from property in TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>()
                     from attribute in property.Attributes.OfType<ValidationAttribute>()
                     where !attribute.IsValid(property.GetValue(entity))
                     select new ModelValidationResult {
                         MemberName = property.Name,
                         Message = attribute.FormatErrorMessage(property.DisplayName ?? string.Empty)
                     };

            return errors.IsEmpty();
        }
    }
}
