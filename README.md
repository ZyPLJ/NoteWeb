# NoteWeb - 便签墙应用

一个简洁美观的在线便签墙应用，支持添加、查看、拖动和管理便签。

## 项目概述

`NoteWeb` 是一个基于 `Web` 技术构建的便签墙应用，用户可以在界面上添加简短的便签，这些便签会以卡片形式显示在虚拟的便签墙上。每个便签都支持拖动、最大化、最小化和关闭等操作，提供了良好的用户交互体验。

快速访问：https://www.pljzy.top/noteweb

纯前端版本：https://pljzy.top/note.html

前端页面由：https://github.com/uninto/notes 而来。

![](NoteWeb/wwwroot/img/1.png)

## 技术栈

### 前端
- HTML5
- CSS3
- JavaScript (原生)

### 后端
- ASP.NET Core 9.0
- Entity Framework Core
- SQLite

## 功能特点

### 用户界面
- 美观的毛玻璃效果输入框
- 随机颜色生成的便签卡片
- 桌面应用风格的窗口控制按钮
- 响应式设计，支持移动端

### 便签操作
- 拖动：通过拖动便签标题栏移动便签
- 最大化：双击标题栏或点击最大化按钮使便签全屏显示
- 最小化：点击最小化按钮移除便签
- 关闭：点击关闭按钮移除便签
- 添加新便签：通过底部输入框添加新的便签内容

### 数据持久化
- 所有便签内容保存到SQLite数据库
- 支持最多100条最近的便签记录
- 自动按时间降序排列显示

## API 文档

### 获取所有便签
```
GET /api/notes
```

- **响应**：返回最近100条便签内容，按创建时间降序排列
- **数据格式**：字符串数组

### 添加新便签
```
POST /api/notes
Content-Type: application/json

{
  "content": "便签内容"
}
```

- **请求体**：
  - `content`：便签内容，最大长度30字符
- **响应**：
  ```json
  {
    "StatusCode": 200,
    "Successful": true,
    "Message": "新增便签成功",
    "Data": null
  }
  ```

## 项目结构

```
NoteWeb/
├── Entity/
│   ├── Model/
│   │   └── Note.cs         # 便签数据模型
│   ├── MyDbContext.cs      # 数据库上下文
│   └── MyDbContextDesignFac.cs
├── wwwroot/                # 静态资源
│   ├── img/                # 图片资源
│   └── index.html          # 前端页面
├── Program.cs              # 应用入口和API定义
├── appsettings.json        # 应用配置
└── noteweb.db              # SQLite数据库文件
```

## 开始使用

### 前提条件
- .NET 9.0 SDK 或更高版本
- 支持的浏览器：Chrome, Firefox, Safari, Edge

### 运行应用

1. 克隆项目到本地
2. 进入项目目录
3. 运行以下命令启动应用：

```bash
cd NoteWeb

dotnet restore

dotnet build -c Release

dotnet run
```

4. 打开浏览器访问 `http://localhost:1556/index.html`

### API文档访问

在开发环境中，可以通过以下地址访问API文档：
- `http://localhost:1556/scalar`

## 数据库设计

### Note表

| 字段名 | 数据类型 | 描述 |
|--------|----------|------|
| Id | Guid | 主键，自动生成 |
| Content | VARCHAR(30) | 便签内容 |
| CreatedAt | DATETIME | 创建时间，UTC时间 |

## Docker部署

- 在任意目录克隆本项目：`git clone https://github.com/ZyPLJ/NoteWeb.git`
- 进入项目根目录：`cd ../NoteWeb`
- 执行`Docekr`命令：`docker-compose up --build -d`

## 注意事项

- 数据库连接配置在 `appsettings.json` 中，默认为本地的 `noteweb.db` 文件
- 本项目尚未完结，有许多bug！！！

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。
