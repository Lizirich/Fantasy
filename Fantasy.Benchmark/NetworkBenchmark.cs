using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Fantasy.Async;
using Fantasy.InnerMessage;
using Fantasy.Network;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Fantasy.Benchmark;

// 使用 InProcess 执行，跟随当前宿主运行时，
// 避免硬编码 RuntimeMoniker 导致"未安装某版本 SDK"而无法运行。
[SimpleJob(RunStrategy.Throughput, baseline: true)]
[InProcess]
public class NetworkBenchmark
{
    private static Scene _scene;
    private static Session _session;
    private readonly BenchmarkRequest _benchmarkRequest = new BenchmarkRequest();

    /// <summary>
    /// 连接到服务器。连接成功后返回 true。
    /// </summary>
    public static async FTask<bool> Connect(string address, NetworkProtocolType protocolType)
    {
        // 初始化框架（不传 logger 时默认使用框架内置的 ConsoleLog）
        await Platform.Console.Entry.Initialize();
        // 控制台没有每帧回调，开启后台线程驱动 Update
        Platform.Console.Entry.StartUpdate();
        _scene = await Platform.Console.Entry.CreateScene();

        var connected = FTask<bool>.Create(false);
        _session = _scene.Connect(address, protocolType,
            () =>
            {
                Log.Debug("连接到目标服务器成功");
                connected.SetResult(true);
            },
            () =>
            {
                Log.Debug("无法连接到目标服务器");
                connected.SetResult(false);
            },
            () => { Log.Debug("与服务器断开连接"); }, false);

        return await connected;
    }

    public static void Run()
    {
        var summary = BenchmarkRunner.Run<NetworkBenchmark>();
        Console.WriteLine(summary);
    }

    public static void Dispose()
    {
        Platform.Console.Entry.Dispose();
    }

    [Benchmark]
    public async FTask Call()
    {
        await _session.Call(_benchmarkRequest);
    }
}
