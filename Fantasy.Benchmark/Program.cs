using Fantasy;
using Fantasy.Benchmark;
using Fantasy.Network;

// Fantasy 网络基准测试
//
// 用法:
//   dotnet run -c Release                       默认连 127.0.0.1:20000 (KCP)
//   dotnet run -c Release -- <address> [proto]  proto 可选 KCP / TCP / WebSocket
//
// 需要先启动服务端: examples/Server/APP/Main (-m Develop)
// 注意: Benchmark 必须用 Release 编译，Debug 下的数据没有意义。

SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

var address = args.Length > 0 ? args[0] : "127.0.0.1:20000";
var protocol = args.Length > 1 && Enum.TryParse<NetworkProtocolType>(args[1], true, out var p)
    ? p
    : NetworkProtocolType.KCP;

// 注意: Log 在 Entry.Initialize 之后才可用，这里先用 Console.WriteLine。
Console.WriteLine($"正在连接 {address} ({protocol}) ...");

if (!await NetworkBenchmark.Connect(address, protocol))
{
    Log.Error($"无法连接到 {address}，请确认服务端已启动，且该端口使用的协议是 {protocol}。");
    NetworkBenchmark.Dispose();
    return 1;
}

NetworkBenchmark.Run();
NetworkBenchmark.Dispose();
return 0;
