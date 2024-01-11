using System;
using System.Collections.Generic;
using WordGame.Utils;

namespace WordGame
{
    /// <summary>
    /// 引用池
    /// </summary>
    public static partial class ReferencePool
    {
        /// <summary>
        /// 引用集合
        /// </summary>
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> m_References;
            private readonly Type m_ReferenceType;
            private int m_UsingReferenceCount;
            private int m_AcquireReferenceCount;
            private int m_ReleaseReferenceCount;
            private int m_AddReferenceCount;
            private int m_RemoveReferenceCount;

            /// <summary>
            /// 初始化引用集合
            /// </summary>
            /// <param name="referenceType">引用类型</param>
            public ReferenceCollection(Type referenceType)
            {
                m_References = new Queue<IReference>();
                m_ReferenceType = referenceType;
                m_UsingReferenceCount = 0;
                m_AcquireReferenceCount = 0;
                m_ReleaseReferenceCount = 0;
                m_AddReferenceCount = 0;
                m_RemoveReferenceCount = 0;
            }

            /// <summary>
            /// 引用集合类型
            /// </summary>
            public Type ReferencesType { get { return m_ReferenceType; } }

            /// <summary>
            /// 获取未使用引用数量
            /// </summary>
            public int UnusedReferenceCount { get { return m_References.Count; } }

            /// <summary>
            /// 获取正在使用引用数量
            /// </summary>
            public int UsingReferenceCount { get { return m_UsingReferenceCount; } }

            /// <summary>
            /// 获取获取引用数量
            /// </summary>
            public int AcquireReferenceCount { get { return m_AcquireReferenceCount; } }

            /// <summary>
            /// 获取归还引用数量
            /// </summary>
            public int ReleaseReferenceCount { get { return m_ReleaseReferenceCount; } }

            /// <summary>
            /// 获取增加引用数量
            /// </summary>
            public int AddReferenceCount { get { return m_AddReferenceCount; } }

            /// <summary>
            /// 获取移除引用数量
            /// </summary>
            public int RemoveReferenceCount { get { return m_RemoveReferenceCount; } }

            /// <summary>
            /// 获取引用
            /// </summary>
            /// <typeparam name="T">引用类型</typeparam>
            /// <returns>引用实例</returns>
            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T)m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return new T();
            }

            /// <summary>
            /// 获取引用
            /// </summary>
            /// <returns>引用实例</returns>
            public IReference Acquire()
            {
                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return (IReference)Activator.CreateInstance(m_ReferenceType);
            }

            /// <summary>
            /// 回收引用
            /// </summary>
            /// <param name="reference">引用实例</param>
            public void Release(IReference reference)
            {
                reference.Clear();
                lock (m_References)
                {
                    if (m_EnableStrictCheck && m_References.Contains(reference))
                    {
                       Debugger.LogDWarning("The reference has been released.");
                    }

                    m_References.Enqueue(reference);
                }

                m_ReleaseReferenceCount++;
                m_UsingReferenceCount--;
            }

            /// <summary>
            /// 批量添加引用
            /// </summary>
            /// <typeparam name="T">引用类型</typeparam>
            /// <param name="count">添加数量</param>
            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }
            }

            /// <summary>
            /// 批量添加引用
            /// </summary>
            /// <param name="count">添加数量</param>
            public void Add(int count)
            {
                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference)Activator.CreateInstance(m_ReferenceType));
                    }
                }
            }

            /// <summary>
            /// 批量删除引用
            /// </summary>
            /// <param name="count"></param>
            public void Remove(int count)
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    m_RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }
            }

            /// <summary>
            /// 删除所有引用
            /// </summary>
            public void RemoveAll()
            {
                lock (m_References)
                {
                    m_RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }

        }
    }
}

