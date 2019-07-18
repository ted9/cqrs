using System;
using System.Linq;
using System.Reflection;

namespace Cqrs.Infrastructure
{
    /// <summary>
    /// 表示继承该抽象类的类型是一个值对象
    /// </summary>
    public abstract class ValueObject<T> : IEquatable<T>
        where T : ValueObject<T>
    {
        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        bool IEquatable<T>.Equals(T other)
        {
            if ((object)other == null) {
                return false;
            }

            if (Object.ReferenceEquals(this, other)) {
                return true;
            }

            var publicProperties = typeof(T).GetProperties();

            if (publicProperties.Any()) {
                return publicProperties.All(p => {
                    var left = p.GetValue(this, null);
                    var right = p.GetValue(other, null);

                    if (left is T) {
                        return Object.ReferenceEquals(left, right);
                    }

                    return Object.Equals(left, right);
                });
            }

            return true;
        }

        private bool Equals(T other)
        {
            if (other == null || other.GetType() != this.GetType()) {
                return false;
            }

            if (Object.ReferenceEquals(this, other)) {
                return true;
            }

            var publicProperties = typeof(T).GetProperties();

            if (publicProperties.Any()) {
                return publicProperties.All(p => {
                    var left = p.GetValue(this, null);
                    var right = p.GetValue(other, null);

                    if (left is T) {
                        return Object.ReferenceEquals(left, right);
                    }

                    return Object.Equals(left, right);
                });
            }

            return true;
        }


        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        public override bool Equals(object obj)
        {
            return this.Equals((T)obj);
        }


        /// <summary>
        /// 哈希函数
        /// </summary>
        public override int GetHashCode()
        {
            return typeof(T).GetProperties().Select(prop => prop.GetValue(this, null))
                .Select(item => item != null ? item.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
        }
        /// <summary>
        /// 判断是否相等
        /// </summary>
        public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
        {
            if (Object.Equals(left, null)) {
                return (Object.Equals(right, null));
            }

            return left.Equals(right);
        }
        /// <summary>
        /// 判断是否不相等
        /// </summary>
        public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
        {
            return !(left == right);
        }
    }
}