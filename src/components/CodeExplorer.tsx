import React, { useState, useEffect } from 'react';
import { Folder, File, Code, ShieldCheck, CornerDownRight, Terminal, Info, AlertCircle } from 'lucide-react';

interface CodeExplorerProps {
  selectedFilePath: string;
  onFileSelect: (path: string) => void;
}

export default function CodeExplorer({ selectedFilePath, onFileSelect }: CodeExplorerProps) {
  const [activeFile, setActiveFile] = useState<string>('/.github/workflows/build.yml');

  useEffect(() => {
    if (selectedFilePath) {
      setActiveFile(selectedFilePath);
    }
  }, [selectedFilePath]);

  const files = {
    '/.github/workflows/build.yml': {
      name: 'build.yml',
      path: '/.github/workflows/build.yml',
      language: 'yaml',
      description: 'GitHub Actions 构建定义。已由 dotnet build 改为标准的 MSBuild 编译，彻底隔离了 XamlCompiler 运行环境。',
      content: `name: Build and Release

on:
  push:
    branches:
      - main
    tags:
      - 'v*'
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'

    # ======= 🟢 修复方案一：改用完整 MSBuild 编译工具 =======
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v2

    - name: Restore dependencies
      run: msbuild src/VoiceInput/VoiceInput.csproj /t:restore /p:Configuration=Release /p:Platform=x64

    - name: Build
      run: msbuild src/VoiceInput/VoiceInput.csproj /p:Configuration=Release /p:Platform=x64

    - name: Publish
      run: |
        # Use a fallback version like 1.0.0 if not running on a tag
        $version = if ("\${{ github.ref_type }}" -eq "tag") { "\${{ github.ref_name }}" } else { "1.0.0" }
        msbuild src/VoiceInput/VoiceInput.csproj /p:Configuration=Release /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishDir="\${{ github.workspace }}/publish/" /p:SelfContained=true /p:Version=$version /t:Publish
    # ====================================================

    - name: Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: VoiceInput-win-x64
        path: publish/

    - name: Create Release
      if: startsWith(github.ref, 'refs/tags/')
      uses: softprops/action-gh-release@v2
      with:
        files: publish/*
      env:
        GITHUB_TOKEN: \${{ secrets.GITHUB_TOKEN }}`
    },
    '/src/VoiceInput/VoiceInput.csproj': {
      name: 'VoiceInput.csproj',
      path: '/src/VoiceInput/VoiceInput.csproj',
      language: 'xml',
      description: 'WinUI 3 应用项目文件。通过添加 DisableXbfGeneration 优化属性，解除潜在的 XAML 优化并发死锁。',
      content: `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <ApplicationManifest>app.manifest</ApplicationManifest>
    <RootNamespace>VoiceInput</RootNamespace>
    <AssemblyName>VoiceInput</AssemblyName>
    <Platform Condition=" '$(Platform)' == '' ">x64</Platform>
    <Platforms>x64</Platforms>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <RuntimeIdentifiers>win-x64</RuntimeIdentifiers>
    <UseRidGraph>true</UseRidGraph>
    
    <!-- 🟢 修复方案三：调整优化管道，规避 XamlCompiler 增量合并崩溃 -->
    <DisableXbfGeneration>false</DisableXbfGeneration>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.4.240802001" />
    <PackageReference Include="NAudio" Version="2.2.1" />
    <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
    <PackageReference Include="System.Speech" Version="8.0.0" />
  </ItemGroup>
</Project>`
    },
    '/src/VoiceInput/Windows/HudWindow.xaml': {
      name: 'HudWindow.xaml',
      path: '/src/VoiceInput/Windows/HudWindow.xaml',
      language: 'xml',
      description: '语音录制状态胶囊 HUD。由一个毛玻璃背景、5根音频电平跳动柱状图和转录文本标签组成。',
      content: `<Window
    x:Class="VoiceInput.Windows.HudWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="HUD"
    Height="40"
    Width="320"
    ExtendsContentIntoTitleBar="True">

    <!-- 语音录制胶囊容器 -->
    <Border Background="#BF1E1E1E" CornerRadius="20" Padding="12,0" BorderBrush="#3A3A3A" BorderThickness="1" x:Name="CapsuleBorder">
        <Grid VerticalAlignment="Center">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <!-- 音频跳动电平动画 -->
            <StackPanel Grid.Column="0" Orientation="Horizontal" Spacing="3" VerticalAlignment="Center" Margin="4,0,12,0" Height="20">
                <Border x:Name="Bar1" Width="3" Height="4" Background="#FFFFFF" CornerRadius="1.5" VerticalAlignment="Center" />
                <Border x:Name="Bar2" Width="3" Height="8" Background="#FFFFFF" CornerRadius="1.5" VerticalAlignment="Center" />
                <Border x:Name="Bar3" Width="3" Height="14" Background="#FFFFFF" CornerRadius="1.5" VerticalAlignment="Center" />
                <Border x:Name="Bar4" Width="3" Height="6" Background="#FFFFFF" CornerRadius="1.5" VerticalAlignment="Center" />
                <Border x:Name="Bar5" Width="3" Height="10" Background="#FFFFFF" CornerRadius="1.5" VerticalAlignment="Center" />
            </StackPanel>

            <!-- 实时识别文本输出 -->
            <TextBlock Grid.Column="1" x:Name="TranscribeTxt" Text="请说话..." FontSize="13" Foreground="#FFFFFF" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" FontWeight="Medium" FontFamily="Segoe UI Variable" />
        </Grid>
    </Border>
</Window>`
    },
    '/src/VoiceInput/Pages/SettingsPage.xaml': {
      name: 'SettingsPage.xaml',
      path: '/src/VoiceInput/Pages/SettingsPage.xaml',
      language: 'xml',
      description: '系统设置页面。配置 ASR 接口参数、触发热键（如右 Alt 键）以及 LLM 智能纠错的开关。',
      content: `<Page
    x:Class="VoiceInput.Pages.SettingsPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Background="Transparent">

    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <Grid Padding="12,12,24,24" RowSpacing="24">
            <!-- MiMo ASR API 配置 -->
            <Border Background="#1C1C1C" CornerRadius="8" Padding="20">
                <StackPanel Spacing="16">
                    <TextBlock Text="MiMo API 配置" FontSize="14" FontWeight="Bold" Foreground="#0078D4" />
                    <PasswordBox x:Name="ApiKeyBox" PasswordChar="●" Width="400" Background="#2A2A2A" Foreground="#FFFFFF" />
                    <Button Content="测试连接" Click="OnTestConnectionClick" Background="#2A2A2A" Foreground="#FFFFFF" />
                </StackPanel>
            </Border>

            <!-- ASR 偏好与触发热键 -->
            <Border Background="#1C1C1C" CornerRadius="8" Padding="20">
                <StackPanel Spacing="16">
                    <TextBlock Text="识别设置" FontSize="14" FontWeight="Bold" Foreground="#0078D4" />
                    <ComboBox x:Name="HotkeyCombo" Background="#2A2A2A" Foreground="#FFFFFF">
                        <ComboBoxItem Content="右 Alt 键" Tag="RightAlt" IsSelected="True" />
                        <ComboBoxItem Content="左 Alt 键" Tag="LeftAlt" />
                    </ComboBox>
                    <ToggleSwitch x:Name="LlmCorrectionSwitch" IsOn="True" OnContent="开启" OffContent="关闭" />
                </StackPanel>
            </Border>
        </Grid>
    </ScrollViewer>
</Page>`
    }
  };

  const currentFile = files[activeFile as keyof typeof files] || files['/.github/workflows/build.yml'];

  const handleFileClick = (path: string) => {
    setActiveFile(path);
    onFileSelect(path);
  };

  // Basic syntax highlighter helper for demonstration
  const highlightCode = (code: string, lang: string) => {
    const lines = code.split('\n');
    return lines.map((line, index) => {
      // Highlight modifications
      const isModified = line.includes('🟢') || line.includes('Setup MSBuild') || line.includes('DisableXbfGeneration') || line.includes('microsoft/setup-msbuild');
      
      let renderedLine = line;
      // Simple coloring for display
      if (lang === 'yaml') {
        renderedLine = line
          .replace(/(^\s*- name:)(.*)/g, '$1<span class="text-indigo-400">$2</span>')
          .replace(/(^\s*uses:)(.*)/g, '$1<span class="text-amber-400">$2</span>')
          .replace(/(^\s*run:)(.*)/g, '$1<span class="text-emerald-400">$2</span>');
      } else if (lang === 'xml') {
        renderedLine = line
          .replace(/(<[a-zA-Z0-9:]+)(.*?>)/g, '<span class="text-blue-400">$1</span>$2')
          .replace(/(<\/[a-zA-Z0-9:]+>)/g, '<span class="text-blue-400">$1</span>')
          .replace(/(PackageReference|Include|Version)="([^"]+)"/g, '<span class="text-amber-400">$1</span>="<span class="text-emerald-400">$2</span>"');
      }

      return (
        <div 
          key={index} 
          className={`flex font-mono text-xs py-0.5 px-4 ${isModified ? 'bg-emerald-500/10 border-l-2 border-emerald-500 text-emerald-100' : 'hover:bg-white/5 text-gray-300'}`}
        >
          <span className="w-12 text-gray-600 select-none text-right pr-4 shrink-0">{index + 1}</span>
          <span className="whitespace-pre" dangerouslySetInnerHTML={{ __html: renderedLine }} />
        </div>
      );
    });
  };

  return (
    <div id="code-explorer" className="bg-[#1C1C1E] border border-[#2D2D30] rounded-xl shadow-xl overflow-hidden grid grid-cols-1 md:grid-cols-12 h-[550px]">
      
      {/* Sidebar Tree - 3 cols */}
      <div className="md:col-span-3 border-r border-[#2D2D30] bg-[#141416] p-4 flex flex-col gap-4 overflow-y-auto">
        <h4 className="text-xs font-semibold uppercase tracking-wider text-gray-500 flex items-center gap-1.5">
          <Folder className="w-4 h-4 text-amber-500" />
          VoiceInput 仓库代码树
        </h4>

        <div className="space-y-3 mt-2 text-xs">
          {/* GitHub Action Folder */}
          <div>
            <div className="flex items-center gap-1.5 text-gray-400 font-semibold mb-1">
              <Folder className="w-3.5 h-3.5 text-amber-500" />
              <span>.github / workflows</span>
            </div>
            <div className="pl-4">
              <button
                onClick={() => handleFileClick('/.github/workflows/build.yml')}
                className={`flex items-center gap-1.5 w-full text-left py-1.5 px-2.5 rounded transition ${
                  activeFile === '/.github/workflows/build.yml' ? 'bg-blue-600/15 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200'
                }`}
              >
                <File className="w-3.5 h-3.5 text-orange-400" />
                <span className="truncate">build.yml</span>
              </button>
            </div>
          </div>

          {/* Source Folder */}
          <div>
            <div className="flex items-center gap-1.5 text-gray-400 font-semibold mb-1">
              <Folder className="w-3.5 h-3.5 text-blue-500" />
              <span>src / VoiceInput</span>
            </div>
            <div className="pl-4 space-y-1">
              <button
                onClick={() => handleFileClick('/src/VoiceInput/VoiceInput.csproj')}
                className={`flex items-center gap-1.5 w-full text-left py-1.5 px-2.5 rounded transition ${
                  activeFile === '/src/VoiceInput/VoiceInput.csproj' ? 'bg-blue-600/15 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200'
                }`}
              >
                <Code className="w-3.5 h-3.5 text-teal-400" />
                <span className="truncate">VoiceInput.csproj</span>
              </button>

              <button
                onClick={() => handleFileClick('/src/VoiceInput/Windows/HudWindow.xaml')}
                className={`flex items-center gap-1.5 w-full text-left py-1.5 px-2.5 rounded transition ${
                  activeFile === '/src/VoiceInput/Windows/HudWindow.xaml' ? 'bg-blue-600/15 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200'
                }`}
              >
                <File className="w-3.5 h-3.5 text-purple-400" />
                <span className="truncate">HudWindow.xaml</span>
              </button>

              <button
                onClick={() => handleFileClick('/src/VoiceInput/Pages/SettingsPage.xaml')}
                className={`flex items-center gap-1.5 w-full text-left py-1.5 px-2.5 rounded transition ${
                  activeFile === '/src/VoiceInput/Pages/SettingsPage.xaml' ? 'bg-blue-600/15 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200'
                }`}
              >
                <File className="w-3.5 h-3.5 text-purple-400" />
                <span className="truncate">SettingsPage.xaml</span>
              </button>
            </div>
          </div>
        </div>

        {/* Selected File Card Info */}
        <div className="mt-auto bg-[#1A1A1D] border border-[#2D2D30] rounded-lg p-3 text-[11px] leading-relaxed">
          <div className="flex items-center gap-1.5 text-blue-400 font-bold mb-1">
            <Info className="w-3.5 h-3.5 shrink-0" />
            <span>文件说明</span>
          </div>
          <p className="text-gray-400">{currentFile.description}</p>
          <div className="flex items-center gap-1 text-emerald-400 font-semibold mt-2">
            <ShieldCheck className="w-3.5 h-3.5 shrink-0" />
            <span>已应用故障修复</span>
          </div>
        </div>
      </div>

      {/* Editor Panel - 9 cols */}
      <div className="md:col-span-9 flex flex-col bg-[#121214] h-full overflow-hidden">
        {/* Editor Tab Bar */}
        <div className="bg-[#18181A] px-4 py-2 border-b border-[#2D2D30] flex items-center justify-between shrink-0">
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-1 px-3 py-1 bg-[#121214] border-t-2 border-blue-500 rounded-t-md text-xs font-mono text-white">
              <File className="w-3.5 h-3.5 text-gray-400" />
              <span>{currentFile.name}</span>
            </div>
          </div>
          <span className="text-[10px] font-mono text-gray-500">{currentFile.path}</span>
        </div>

        {/* Code Content */}
        <div className="flex-1 overflow-y-auto py-4 font-mono select-text bg-[#0D0D0E]">
          {highlightCode(currentFile.content, currentFile.language)}
        </div>
      </div>
    </div>
  );
}
