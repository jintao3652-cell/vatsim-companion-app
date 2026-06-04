# Contributing to VATSIM Companion App

感谢您对 VATSIM Companion App 项目的兴趣！我们欢迎各种形式的贡献。

## 如何贡献

### 报告 Bug

在 GitHub Issues 中报告 Bug 时，请包含：

- 清晰的问题描述
- 重现步骤
- 预期行为 vs 实际行为
- 环境信息（操作系统、版本等）
- 相关日志或截图

### 提议新功能

在提议新功能前，请：

1. 检查是否已有类似的 Issue
2. 在 Discussions 中讨论可行性
3. 创建详细的功能请求 Issue

### 提交代码

1. **Fork 仓库**
   ```bash
   git clone https://github.com/yourusername/vatsim-companion-app.git
   cd vatsim-companion-app
   ```

2. **创建分支**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **编写代码**
   - 遵循现有代码风格
   - 添加必要的注释
   - 编写单元测试（如果适用）

4. **提交更改**
   ```bash
   git add .
   git commit -m "feat: add amazing feature"
   ```

   提交消息格式：
   - `feat:` 新功能
   - `fix:` Bug 修复
   - `docs:` 文档更新
   - `style:` 代码格式调整
   - `refactor:` 重构
   - `test:` 测试相关
   - `chore:` 构建/工具相关

5. **推送并创建 PR**
   ```bash
   git push origin feature/your-feature-name
   ```

## 代码规范

### C# (.NET)
- 遵循 Microsoft C# 编码规范
- 使用 4 空格缩进
- 类名使用 PascalCase
- 方法名使用 PascalCase
- 私有字段使用 _camelCase

### Dart (Flutter)
- 遵循 Effective Dart 指南
- 使用 2 空格缩进
- 类名使用 PascalCase
- 方法/变量名使用 camelCase
- 运行 `dart format .` 格式化代码

## Pull Request 流程

1. 确保代码通过所有测试
2. 更新相关文档
3. PR 描述应清楚说明更改内容
4. 等待代码审查
5. 根据反馈进行修改

## 开发环境

参考 [SETUP.md](docs/SETUP.md) 设置开发环境。

## 行为准则

- 尊重所有贡献者
- 欢迎建设性的批评
- 专注于对项目最有利的事情
- 遵守 VATSIM 社区规则

## 问题？

如有疑问，请通过以下方式联系：

- GitHub Discussions
- VATSIM Discord
- GitHub Issues

感谢您的贡献！ 🚀
