#if FANTASY_CONSOLE
using System.Threading;
using Fantasy.Async;
using Fantasy.Serialize;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Fantasy.Platform.Console
{
    /// <summary>
    /// Scene创建完成后发布的事件。
    /// </summary>
    public struct OnFantasyInit
    {
        public Scene Scene;
    }

    /// <summary>
    /// 一般的控制台启动入口，可以适用大部分客户端环境。
    /// </summary>
    public sealed class Entry
    {
        private static bool _isInit;
        private static Thread _updateThread;

        /// <summary>
        /// 由Entry创建并持有的Scene。
        /// </summary>
        public static Scene Scene { get; private set; }

        /// <summary>
        /// 初始化框架。
        /// 程序集的注册由Source Generator生成的ModuleInitializer在程序集加载时自动完成，
        /// 所以这里不需要手动传入程序集。
        /// </summary>
        /// <param name="logger">自定义日志实现，为null时使用ConsoleLog。</param>
        public static async FTask Initialize(ILog logger = null)
        {
            if (_isInit)
            {
                Log.Error("Fantasy has already been initialized and does not need to be initialized again!");
                return;
            }

            Log.Initialize(logger ?? new ConsoleLog());
            Log.Info($"Fantasy Version:{ProgramDefine.VERSION}");
            // 初始化序列化
            await SerializerManager.Initialize();
            _isInit = true;
            // 设置当前程序已经在运行中
            ProgramDefine.IsAppRunning = true;
            Log.Debug("Fantasy Initialize Complete!");
        }

        /// <summary>
        /// 启动框架的每帧更新。
        /// 如果您的平台有每帧更新逻辑的方法，请不要调用这个方法，改为在每帧调用Update。
        /// 如果没有实现每帧执行方法的平台需要调用这个方法，目的是开启一个新的线程来每帧执行Update。
        /// 注意因为开启了一个新的线程来处理更新逻辑，所以要注意多线程的问题。
        /// </summary>
        public static void StartUpdate()
        {
            _updateThread = new Thread(() =>
            {
                while (_isInit)
                {
                    Update();
                    Thread.Sleep(1);
                }
            })
            {
                IsBackground = true
            };
            _updateThread.Start();
        }

        /// <summary>
        /// 在Entry中创建一个Scene，如果Scene已经被创建过，将先销毁Scene再创建。
        /// </summary>
        /// <param name="sceneRuntimeMode">Scene的运行模式。</param>
        public static async FTask<Scene> CreateScene(string sceneRuntimeMode = SceneRuntimeMode.MainThread)
        {
            Scene?.Dispose();
            Scene = await Scene.Create(sceneRuntimeMode);
            await Scene.EventComponent.PublishAsync(new OnFantasyInit()
            {
                Scene = Scene
            });
            return Scene;
        }

        /// <summary>
        /// 如果宿主平台有每帧执行的方法，一定要在每帧执行这个方法。
        /// 已经调用过StartUpdate的情况下不需要再手动调用。
        /// </summary>
        public static void Update()
        {
            ThreadScheduler.Update();
            ThreadScheduler.LateUpdate();
        }

        /// <summary>
        /// 释放框架占用的资源。
        /// </summary>
        public static void Dispose()
        {
            SerializerManager.Dispose();
            Scene?.Dispose();
            Scene = null;
            _isInit = false;
            // 设置当前程序已经停止运行
            ProgramDefine.IsAppRunning = false;
        }
    }
}
#endif
