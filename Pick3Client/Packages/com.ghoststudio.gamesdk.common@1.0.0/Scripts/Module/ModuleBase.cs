using System;
using System.Collections.Generic;
using System.Linq;

namespace AppBase.Module
{
    /// <summary>
    /// 模块基类
    /// </summary>
    public class ModuleBase : IDisposable
    {
        protected string TAG => GetType().Name;
        protected List<ModuleBase> moduleList;
        protected Dictionary<Type, ModuleBase> moduleDict;
        private byte isModuleInited;
        public bool IsModuleInited => isModuleInited != 0;
        
        /// <summary>
        /// 模块数据
        /// </summary>
        protected object moduleData;

        /// <summary>
        /// 父组件
        /// </summary>
        public ModuleBase ParentModule => parentModule;
        private ModuleBase parentModule;

        /// <summary>
        /// 初始化模块前调用，在其中添加子模块不会立即Init
        /// </summary>
        protected virtual void OnBeforeInit()
        {
        }
        
        /// <summary>
        /// 类库内部使用，用于解决在类库中继承时，子类覆盖OnInit但是忘记调用base.OnInit导致异常
        /// </summary>
        protected virtual void OnInternalInit()
        {
        }
        
        /// <summary>
        /// 初始化模块时调用，用于在登录成功后，在里面加载数据、注册事件等，在其中添加子模块会立即Init
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 初始化模块后调用，此时所有子模块和父类的子模块都已经Init完毕时执行
        /// </summary>
        protected virtual void OnAfterInit()
        {
        }

        /// <summary>
        /// 析构模块时调用，用于当切换账号时，会先调用此方法，在里面清除数据、注销事件等
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
        
        /// <summary>
        /// 类库内部使用，用于解决在类库中继承时，子类覆盖OnDestroy但是忘记调用base.OnDestroy导致异常
        /// </summary>
        protected virtual void OnInternalDestroy()
        {
        }

        /// <summary>
        /// 当添加子模块时调用
        /// </summary>
        protected virtual void OnAddModule(ModuleBase module)
        {
        }

        /// <summary>
        /// 当移除子模块时调用
        /// </summary>
        protected virtual void OnRemoveModule(ModuleBase module)
        {
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        public void Init()
        {
            if (IsModuleInited) return;
            OnBeforeInit();
            isModuleInited = 1;
            OnInternalInit();
            OnInit();
            moduleList?.ForEach(m => m.Init());
            if (ParentModule == null)
            {
                AfterInit();
            }
        }

        /// <summary>
        /// 所有子模块都初始化完毕后，调用此方法
        /// </summary>
        public void AfterInit()
        {
            if (isModuleInited == 1)
            {
                isModuleInited = 2;
                OnAfterInit();
            }
            moduleList?.ForEach(m => m.AfterInit());
        }

        /// <summary>
        /// 析构模块
        /// </summary>
        public void Dispose()
        {
            RemoveAllModules();
            if (IsModuleInited)
            {
                OnDestroy();
                OnInternalDestroy();
            }
            parentModule = null;
            moduleData = null;
            isModuleInited = 0;
        }

        /// <summary>
        /// 添加模块，如果模块类型已存在，则返回已存在的模块
        /// </summary>
        /// <param name="moduleData">模块初始化数据</param>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>模块引用</returns>
        public T AddModule<T>(object moduleData = null) where T : ModuleBase, new()
        {
            var type = typeof(T);
            if (moduleDict != null && moduleDict.TryGetValue(type, out var m))
            {
                return (T)m;
            }
            return AddModule(new T(), moduleData);
        }

        /// <summary>
        /// 添加模块，如果模块类型已存在，则会重复添加
        /// </summary>
        /// <param name="module">模块引用</param>
        /// <param name="moduleData">模块初始化数据</param>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>模块引用</returns>
        public T AddModule<T>(T module, object moduleData = null) where T : ModuleBase
        {
            if (module == null) return null;
            moduleList ??= new List<ModuleBase>();
            moduleDict ??= new Dictionary<Type, ModuleBase>();
            moduleList.Add(module);
            moduleDict.TryAdd(module.GetType(), module);
            module.parentModule = this;
            module.moduleData = moduleData;
            //子模块在父模块初始化后再初始化
            if (IsModuleInited)
            {
                module.Init();
            }
            OnAddModule(module);
            return module;
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>模块引用</returns>
        public T GetModule<T>() where T : ModuleBase
        {
            return GetModule(typeof(T)) as T;
        }

        /// <summary>
        /// 获取模块
        /// </summary>
        /// <param name="type">模块类型</param>
        /// <returns>模块引用</returns>
        public ModuleBase GetModule(Type type)
        {
            if (moduleDict == null) return null;
            if (moduleDict.TryGetValue(type, out var m))
            {
                return m;
            }
            //找不到的情况下，查找子类
            foreach (var pair in moduleDict)
            {
                if (type.IsAssignableFrom(pair.Key))
                {
                    m = pair.Value;
                    break;
                }
            }
            if (m != null)
            {
                //缓存起来，下次直接返回，避免再次遍历
                moduleDict[type] = m;
            }
            return m;
        }

        /// <summary>
        /// 移除模块
        /// </summary>
        /// <typeparam name="T">模块类型</typeparam>
        /// <returns>模块引用</returns>
        public T RemoveModule<T>() where T : ModuleBase
        {
            return RemoveModule(typeof(T)) as T;
        }
        
        /// <summary>
        /// 移除模块
        /// </summary>
        /// <param name="type">模块类型</param>
        /// <returns>模块引用</returns>
        public ModuleBase RemoveModule(Type type)
        {
            var module = GetModule(type);
            if (module != null)
            {
                RemoveModule(module);
            }
            return module;
        }

        /// <summary>
        /// 移除模块
        /// </summary>
        /// <param name="module">模块引用</param>
        /// <returns>是否存在</returns>
        public bool RemoveModule(ModuleBase module)
        {
            if (module == null || moduleList == null) return false;
            if (moduleList.Remove(module))
            {
                moduleDict.Where(x => x.Value == module).Select(x => x.Key).ToList().ForEach(x => moduleDict.Remove(x));
                OnRemoveModule(module);
                module.Dispose();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 移除所有模块
        /// </summary>
        public void RemoveAllModules()
        {
            if (moduleList != null)
            {
                for (int i = moduleList.Count - 1; i >= 0; i--)
                {
                    OnRemoveModule(moduleList[i]);
                    moduleList[i].Dispose();
                }
                moduleList.Clear();
                moduleList = null;
            }
            if (moduleDict != null)
            {
                moduleDict.Clear();
                moduleDict = null;
            }
        }
    }
}
