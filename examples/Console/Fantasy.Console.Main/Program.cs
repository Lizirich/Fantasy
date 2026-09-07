using Fantasy;
using Fantasy.Async;

// 框架的主线程调度依赖同步上下文，必须先设置。
SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

// 初始化框架。程序集的注册由Source Generator生成的ModuleInitializer自动完成。
await Fantasy.Platform.Console.Entry.Initialize();

// 控制台没有每帧回调，开启一个后台线程来驱动Update。
Fantasy.Platform.Console.Entry.StartUpdate();

// 创建Scene（内部会发布OnFantasyInit事件）。
await Fantasy.Platform.Console.Entry.CreateScene();

Fantasy.Console.Entity.Entry.Show().Coroutine();

Log.Info("按 Ctrl+C 退出...");

// 注意：不要用 Console.ReadKey() 阻塞。
// Connect 的回调需要在主线程的同步上下文里被派发，
// ReadKey 会占住主线程，导致连接回调迟迟不执行（表现为"连接成功"不打印）。
// 在没有输入设备的环境（重定向stdin、容器、CI）里 ReadKey 还会直接抛异常。
var exit = new TaskCompletionSource<bool>();
System.Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exit.TrySetResult(true);
};
await exit.Task;

Fantasy.Platform.Console.Entry.Dispose();
