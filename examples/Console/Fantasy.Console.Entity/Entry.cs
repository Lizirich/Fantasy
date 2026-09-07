using Fantasy.Async;
using Fantasy.Network;

namespace Fantasy.Console.Entity;

/// <summary>
/// 控制台客户端示例：连接Gate、发送消息、发起RPC。
/// </summary>
public static class Entry
{
    private static Scene _scene = null!;
    private static Session _session = null!;

    public static async FTask Show()
    {
        // Entry.CreateScene 已经创建好Scene，这里直接复用，避免重复创建。
        _scene = Fantasy.Platform.Console.Entry.Scene;

        _session = _scene.Connect(
            "127.0.0.1:20000",
            NetworkProtocolType.KCP,
            OnConnectComplete,
            OnConnectFail,
            OnConnectDisconnect,
            false, 5000);

        await FTask.CompletedTask;
    }

    private static void OnConnectComplete()
    {
        Log.Debug("连接成功");

        // 添加心跳组件给Session，2000表示2000毫秒发送一次心跳。
        _session.AddComponent<SessionHeartbeatComponent>().Start(2000);

        SendAndCall().Coroutine();
    }

    private static async FTask SendAndCall()
    {
        // 1) 单向消息
        var message = C2G_TestMessage.Create();
        message.Tag = "console-send";
        _session.Send(message);
        Log.Debug("已发送 C2G_TestMessage");

        // 2) RPC请求，等待服务端应答
        var request = C2G_TestRequest.Create();
        request.Tag = "console-call";
        var response = (G2C_TestResponse)await _session.Call(request);
        Log.Debug($"收到 G2C_TestResponse ErrorCode={response.ErrorCode} Tag={response.Tag}");
    }

    private static void OnConnectFail()
    {
        Log.Debug("连接失败");
    }

    private static void OnConnectDisconnect()
    {
        Log.Debug("连接断开");
    }
}
