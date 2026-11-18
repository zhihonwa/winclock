# winclock

#### 介绍
1. 适用于 Win10、Win11 的桌面时钟小组件，窗口贴靠屏幕右上角，无边框、可拖动。
2. 显示时间、日期、星期，可选显示农历。
3. 提供深色设置界面，可配置时区、字体、字号、置顶、透明度、最小化限制与开机自启。
4. 程序免安装，下载后直接运行。
5. 运行后图标位于系统托盘，右键托盘图标或时钟窗口可选择“设置”或“退出”。

![示例图片](winclock_demo.png)

#### 设置项
- 时区：默认 UTC+8。
- 时间字体与字号。
- 显示日期与星期：默认启用；日期与星期可单独设置字体和字号。
- 显示农历日期：默认关闭。
- 始终置顶：默认启用。
- 透明效果与透明度：默认启用，透明度 70%，可调 0%–100%。
- 禁止窗口最小化：默认关闭。
- 随操作系统自动启动：默认启用。

设置保存在 `%APPDATA%\WinClock\settings.xml`，保存后立即生效，重启程序后继续生效。
开机自启写入当前用户注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`，无需管理员权限。

#### 开发环境
- Visual Studio 2022
- .NET Framework 4.8
- C#

#### 本地构建
```powershell
msbuild winclock.sln /p:Configuration=Release /p:Platform="Any CPU"
```

生成文件：`winclock\bin\Release\winclock.exe`。

#### 自动构建与发布
仓库包含 GitHub Actions 工作流 `.github/workflows/release.yml`。推送符合 `vX.Y.Z` 格式的 tag 后自动编译并发布：

```powershell
git tag v1.0.0
git push origin v1.0.0
```

工作流会生成 `WinClock_v1.0.0.exe`，版本号与 tag 一致，并附加到对应的 GitHub Release。