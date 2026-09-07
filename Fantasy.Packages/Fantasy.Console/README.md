# Fantasy-Console

Fantasy 框架的**控制台客户端**库，用于在标准 .NET 宿主环境中运行 Fantasy 客户端，
不依赖 Unity。

## 适用场景

- 机器人 / 压测客户端
- 自动化测试、CI 中的协议回归
- 无头（headless）客户端、命令行工具
- 服务端开发期联调，不必打开 Unity

## 与其它包的关系

| 包 | 预编译符号 | 用途 |
|---|---|---|
| `Fantasy-Net` | `FANTASY_NET` | 服务端 |
| `Fantasy.Unity` | `FANTASY_UNITY` | Unity 客户端 |
| `Fantasy-Console` | `FANTASY_CONSOLE` | 控制台客户端（本包） |

本包与 Unity 客户端**共享同一份 `Runtime/Core` 源码**，仅编译符号不同，
因此协议、ECS、序列化行为与 Unity 端保持一致。

## 快速开始

```csharp
using Fantasy;
using Fantasy.Network;

// 必须设置同步上下文，框架的主线程调度依赖它
SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

await Fantasy.Platform.Console.Entry.Initialize();
// 平台没有每帧回调时，用 StartUpdate 开一个后台更新线程
Fantasy.Platform.Console.Entry.StartUpdate();

var scene = await Fantasy.Platform.Console.Entry.CreateScene();

var session = scene.Connect("127.0.0.1:20000",
    NetworkProtocolType.KCP,
    onConnectComplete: () => Log.Info("连接成功"),
    onConnectFail:     () => Log.Info("连接失败"),
    onConnectDisconnect: () => Log.Info("连接断开"),
    isHttps: false,
    connectTimeout: 5000);
```

如果宿主本身有每帧循环（例如自己的主循环），**不要**调用 `StartUpdate`，
改为在每帧调用 `Fantasy.Platform.Console.Entry.Update()`。

## 注意

- 程序集注册由 Source Generator 生成的 `[ModuleInitializer]` 自动完成，无需手动传程序集。
- 协议文件可直接复用 Unity 客户端导出的产物（不含平台条件编译）。
- `BsonPack` 在本包中不可用，数据库序列化属服务端能力。
