# Fantasy.Benchmark

使用 Fantasy.Benchmark 工具，我们能够快速评估框架网络的处理性能。目前，该工具提供的基准测试主要集中在 RPC（远程过程调用）消息 方面。这一项测试能够有效测量系统在处理远程调用时的响应时间、吞吐量和资源利用率，帮助开发者优化网络通信性能，确保在高负载情况下系统依然能够稳定运行。

本工具基于 `Fantasy-Console`（控制台客户端），**不需要启动 Unity** 即可压测服务端。

## 操作步骤

1. 启动服务端：打开 `examples/Server/APP/Main`，以 `-m Develop` 参数运行。
2. 运行本工具：

```bash
# 默认连接 127.0.0.1:20000 (KCP)
dotnet run -c Release

# 指定地址与协议（KCP / TCP / WebSocket）
dotnet run -c Release -- 127.0.0.1:20000 KCP
```

## 注意事项

- **必须用 `-c Release`**。BenchmarkDotNet 会拒绝在 Debug 配置下产出数据，因为
  未优化的代码测得的数值没有参考价值。
- 协议要与服务端该端口实际监听的协议一致。示例服务端默认只有
  `Gate` 场景开放 KCP `20000`；若要压测 TCP / WebSocket，
  需先在服务端 `Fantasy.config` 的 `<scenes>` 中增加对应的 Gate 场景。
- 测试使用框架内置的 `BenchmarkRequest` / `BenchmarkResponse`，
  服务端 handler（`Runtime/Core/Benchmark/Handler/`）是空实现，
  因此测得的是**框架网络层往返开销**，不含业务逻辑耗时。
- 采用 `[InProcess]` 执行，跟随当前宿主运行时，不要求安装特定版本的 .NET SDK。
