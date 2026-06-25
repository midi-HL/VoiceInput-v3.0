import React, { useState } from 'react';
import { motion } from 'motion/react';
import { Terminal, Code, Cpu, ShieldCheck, HelpCircle, ArrowRight, Github, AlertTriangle, CheckCircle2 } from 'lucide-react';
import BuildTroubleshooter from './components/BuildTroubleshooter';
import CodeExplorer from './components/CodeExplorer';
import AppSimulator from './components/AppSimulator';

export default function App() {
  const [activeTab, setActiveTab] = useState<'troubleshooting' | 'code' | 'simulator'>('troubleshooting');
  const [selectedFilePath, setSelectedFilePath] = useState<string>('/.github/workflows/build.yml');

  const handleSelectFile = (path: string) => {
    setSelectedFilePath(path);
    setActiveTab('code');
  };

  return (
    <div id="developer-suite-container" className="min-h-screen bg-[#0A0A0C] text-gray-200 font-sans flex flex-col selection:bg-blue-600/30">
      
      {/* Top Banner / Header */}
      <header className="bg-[#121215]/80 backdrop-blur border-b border-[#202024] sticky top-0 z-50 px-6 py-4 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-blue-600 to-indigo-700 flex items-center justify-center text-white shadow-lg shadow-blue-500/10">
            <Cpu className="w-5 h-5" />
          </div>
          <div>
            <h1 className="text-base font-bold font-display tracking-tight text-white flex items-center gap-2">
              Windows Voice Input Developer Suite
              <span className="text-[10px] font-mono font-medium px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 flex items-center gap-0.5">
                <CheckCircle2 className="w-3 h-3" /> 编译故障已修复
              </span>
            </h1>
            <p className="text-[11px] text-gray-500 mt-0.5">
              Windows 托盘语音输入法（.NET 8 / WinUI 3 / MiMo-ASR）编译链排查、仿真与诊断工作台
            </p>
          </div>
        </div>

        <div className="flex items-center gap-4">
          <a
            href="https://github.com"
            target="_blank"
            rel="noopener noreferrer"
            className="text-xs text-gray-400 hover:text-gray-200 flex items-center gap-1.5 transition"
          >
            <Github className="w-4 h-4" />
            <span>GitHub 仓库</span>
          </a>
        </div>
      </header>

      {/* Main Workspace Body */}
      <main className="flex-1 max-w-7xl w-full mx-auto p-6 flex flex-col gap-6">
        
        {/* Core Fix Statement / Quick Summary Panel */}
        <section className="bg-[#121215] border border-blue-950/30 rounded-xl p-5 shadow-lg relative overflow-hidden">
          <div className="absolute top-0 right-0 w-64 h-64 bg-blue-600/5 rounded-full blur-3xl pointer-events-none" />
          <div className="flex flex-col md:flex-row gap-5 items-start md:items-center justify-between relative z-10">
            <div className="space-y-1">
              <h2 className="text-sm font-bold text-white uppercase tracking-wider text-blue-400">已应用核心技术干预 (Technical Interventions)</h2>
              <p className="text-xs text-gray-400 leading-relaxed max-w-3xl mt-1.5">
                针对 GitHub Actions 环境中 <code className="text-red-300 font-mono bg-black/30 px-1.5 py-0.5">XamlCompiler.exe</code> 的 Pass 2 崩溃故障（MSB3073 错误码 1），我们同步实施了
                <strong className="text-white">【工作流中改用 MSBuild.exe】</strong>与<strong className="text-white">【CSPROJ中强制关闭管道优化】</strong>的双重容错架构，使编译管道彻底稳定。
              </p>
            </div>
            
            <div className="flex gap-2 shrink-0">
              <button 
                onClick={() => handleSelectFile('/.github/workflows/build.yml')}
                className="text-xs bg-blue-600/10 hover:bg-blue-600/20 text-blue-400 border border-blue-500/20 font-semibold px-4 py-2 rounded-lg transition"
              >
                查看 build.yml 变更
              </button>
              <button 
                onClick={() => handleSelectFile('/src/VoiceInput/VoiceInput.csproj')}
                className="text-xs bg-teal-600/10 hover:bg-teal-600/20 text-teal-400 border border-teal-500/20 font-semibold px-4 py-2 rounded-lg transition"
              >
                查看 CSPROJ 优化
              </button>
            </div>
          </div>
        </section>

        {/* Tab Navigation */}
        <nav className="flex border-b border-[#202024] gap-2">
          <button
            onClick={() => setActiveTab('troubleshooting')}
            className={`pb-3 px-4 text-xs font-semibold tracking-wide border-b-2 transition flex items-center gap-2 ${
              activeTab === 'troubleshooting' 
                ? 'border-blue-500 text-white' 
                : 'border-transparent text-gray-500 hover:text-gray-300'
            }`}
          >
            <Terminal className="w-4 h-4" />
            构建流水线诊断与模拟
          </button>

          <button
            onClick={() => setActiveTab('code')}
            className={`pb-3 px-4 text-xs font-semibold tracking-wide border-b-2 transition flex items-center gap-2 ${
              activeTab === 'code' 
                ? 'border-blue-500 text-white' 
                : 'border-transparent text-gray-500 hover:text-gray-300'
            }`}
          >
            <Code className="w-4 h-4" />
            仓库代码与配置浏览器
          </button>

          <button
            onClick={() => setActiveTab('simulator')}
            className={`pb-3 px-4 text-xs font-semibold tracking-wide border-b-2 transition flex items-center gap-2 ${
              activeTab === 'simulator' 
                ? 'border-blue-500 text-white' 
                : 'border-transparent text-gray-500 hover:text-gray-300'
            }`}
          >
            <Cpu className="w-4 h-4" />
            WinUI 3 客户端虚拟机沙盒
          </button>
        </nav>

        {/* Dynamic Workspace Container */}
        <div className="flex-1 min-h-0">
          {activeTab === 'troubleshooting' && (
            <BuildTroubleshooter onSelectFile={handleSelectFile} />
          )}

          {activeTab === 'code' && (
            <CodeExplorer selectedFilePath={selectedFilePath} onFileSelect={setSelectedFilePath} />
          )}

          {activeTab === 'simulator' && (
            <AppSimulator />
          )}
        </div>

        {/* Technical Summary Panel */}
        <footer className="mt-4 bg-[#121215] border border-[#202024] rounded-xl p-5 flex flex-col md:flex-row gap-6 items-start justify-between">
          <div className="space-y-1 md:max-w-2xl">
            <h3 className="text-xs font-bold text-gray-400 uppercase tracking-wider flex items-center gap-1.5">
              <ShieldCheck className="w-4.5 h-4.5 text-emerald-500" />
              Windows App SDK 编译链已知缺陷说明 (Known Limitation)
            </h3>
            <p className="text-[11px] text-gray-500 leading-normal mt-1">
              由于 .NET CLI (dotnet build) 的轻量级运行时在对含有特定 ResourceDictionary 或 ControlTemplate 中 ElementName 元素进行编译阶段合并时，其底层宿主极易产生栈溢出或并发竞态锁死；
              而 Visual Studio 附带的完整版 MSBuild.exe 在全内存模式下执行编译，自带了完整的底层反射与代码生成优化，能完全规避此 Bug。本工作室已将编译链全面迁移至微软推荐的 MSBuild 构建模型。
            </p>
          </div>
          <div className="bg-[#1C1C1E] border border-[#2D2D30] rounded-lg p-3 text-[11px] text-gray-400 w-full md:w-80 shrink-0">
            <div className="font-bold mb-1 flex items-center gap-1">
              <HelpCircle className="w-3.5 h-3.5 text-blue-400" />
              本地复现与排查技巧
            </div>
            <p className="text-gray-500 leading-relaxed">
              若在本地开发机遇到此类报错，可在终端中执行带诊断输出的命令以抓取具体 XAML 嵌套位置：
            </p>
            <code className="block bg-black/40 p-2 rounded text-red-300 font-mono text-[10px] mt-2 border border-red-900/10">
              msbuild src/VoiceInput/VoiceInput.csproj /v:diag
            </code>
          </div>
        </footer>

      </main>
    </div>
  );
}
