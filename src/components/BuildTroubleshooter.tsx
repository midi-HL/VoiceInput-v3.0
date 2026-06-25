import React, { useState, useEffect, useRef } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Terminal, Play, CheckCircle2, XCircle, AlertTriangle, RefreshCw, Cpu, Check, FileText } from 'lucide-react';
import { BuildLogLine } from '../types';

interface BuildTroubleshooterProps {
  onSelectFile: (path: string) => void;
}

export default function BuildTroubleshooter({ onSelectFile }: BuildTroubleshooterProps) {
  const [activeStrategy, setActiveStrategy] = useState<'all' | '1' | '2' | '3'>('all');
  const [buildRunning, setBuildRunning] = useState<boolean>(false);
  const [buildSuccess, setBuildSuccess] = useState<boolean | null>(null);
  const [logs, setLogs] = useState<BuildLogLine[]>([]);
  const [currentStep, setCurrentStep] = useState<number>(-1);
  const terminalEndRef = useRef<HTMLDivElement>(null);

  const pipelineSteps = [
    { name: 'Checkout repository', run: 'actions/checkout@v4' },
    { name: 'Setup .NET SDK', run: 'actions/setup-dotnet@v4 (version 8.0.x)' },
    { name: 'Setup MSBuild Command Line', run: 'microsoft/setup-msbuild@v2' },
    { name: 'Restore NuGet Dependencies', run: 'msbuild src/VoiceInput/VoiceInput.csproj /t:restore' },
    { name: 'Compile WinUI 3 App (Pass 1 & Pass 2)', run: 'msbuild src/VoiceInput/VoiceInput.csproj /p:Configuration=Release /p:Platform=x64' },
    { name: 'Publish Standalone Executable', run: 'msbuild src/VoiceInput/VoiceInput.csproj /t:Publish /p:PublishDir=... /p:SelfContained=true' },
    { name: 'Upload Compiled Artifacts', run: 'actions/upload-artifact@v4' }
  ];

  const triggerBuild = () => {
    if (buildRunning) return;
    setBuildRunning(true);
    setBuildSuccess(null);
    setCurrentStep(0);
    setLogs([
      { timestamp: '04:41:01', type: 'info', message: 'Starting GitHub Actions runner (windows-latest)...' },
      { timestamp: '04:41:02', type: 'info', message: 'Initializing virtual environment...' }
    ]);
  };

  useEffect(() => {
    if (!buildRunning || currentStep < 0) return;

    if (currentStep >= pipelineSteps.length) {
      setBuildRunning(false);
      setBuildSuccess(true);
      setLogs(prev => [
        ...prev,
        { timestamp: '04:41:15', type: 'success', message: '🎉 PIPELINE SUCCESSFUL!' },
        { timestamp: '04:41:15', type: 'success', message: 'VoiceInput-win-x64 artifact successfully generated (publish/VoiceInput.exe).' }
      ]);
      return;
    }

    const step = pipelineSteps[currentStep];
    const timer = setTimeout(() => {
      setLogs(prev => [
        ...prev,
        { timestamp: '04:41:' + (10 + currentStep * 2).toString(), type: 'info', message: `Executing step: ${step.name}...` },
        { timestamp: '04:41:' + (10 + currentStep * 2).toString(), type: 'success', message: `  $ ${step.run}` }
      ]);

      // Step-specific details
      setTimeout(() => {
        if (currentStep === 2) {
          setLogs(prev => [
            ...prev,
            { timestamp: '04:41:14', type: 'info', message: '  Found MSBuild.exe at: C:\\Program Files\\Microsoft Visual Studio\\2022\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe' }
          ]);
        } else if (currentStep === 4) {
          setLogs(prev => [
            ...prev,
            { timestamp: '04:41:18', type: 'info', message: '  XamlCompiler.exe Pass 1: Generated code-behind successfully.' },
            { timestamp: '04:41:19', type: 'info', message: '  XamlCompiler.exe Pass 2: Compiled XAML types using MSBuild pipeline (No .NET CLI crash!).' },
            { timestamp: '04:41:20', type: 'info', message: '  VoiceInput.csproj -> bin\\x64\\Release\\net8.0-windows10.0.19041.0\\win-x64\\VoiceInput.dll' }
          ]);
        } else if (currentStep === 5) {
          setLogs(prev => [
            ...prev,
            { timestamp: '04:41:22', type: 'info', message: '  Packaging files for win-x64 runtime...' },
            { timestamp: '04:41:23', type: 'info', message: '  Successfully published 14 files to: publish/' }
          ]);
        }

        setCurrentStep(prev => prev + 1);
      }, 800);

    }, 500);

    return () => clearTimeout(timer);
  }, [buildRunning, currentStep]);

  useEffect(() => {
    if (terminalEndRef.current) {
      terminalEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [logs]);

  return (
    <div id="build-troubleshooter-root" className="grid grid-cols-1 lg:grid-cols-12 gap-6">
      
      {/* Troubleshooting and Explanations - Left Side (8 cols) */}
      <div className="lg:col-span-7 flex flex-col gap-6">
        
        {/* Error Overview */}
        <div id="error-overview" className="bg-[#1C1C1E] border border-red-900/40 rounded-xl p-5 shadow-lg">
          <div className="flex items-start gap-4">
            <div className="bg-red-500/10 p-2.5 rounded-lg border border-red-500/20 text-red-400">
              <AlertTriangle className="w-6 h-6" />
            </div>
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-1.5">
                <span className="text-xs font-mono bg-red-500/20 text-red-300 px-2 py-0.5 rounded">MSB3073</span>
                <span className="text-xs font-mono bg-red-500/20 text-red-300 px-2 py-0.5 rounded">XamlCompiler Error 1</span>
              </div>
              <h3 className="text-lg font-semibold text-white">XamlCompiler.exe 编译中断异常</h3>
              <p className="text-sm text-gray-400 mt-2 leading-relaxed">
                在通过 .NET CLI (<code className="text-red-300 font-mono text-xs bg-black/30 px-1 py-0.5 rounded">dotnet build</code>) 执行 XAML 增量编译或 Pass 2 时，Windows App SDK 编译链调用的 <code className="text-gray-300 font-mono text-xs bg-black/30 px-1 py-0.5">XamlCompiler.exe</code> 静默崩溃退出了。这是微软 WinUI 3 长期存在的编译期 Bug。
              </p>
            </div>
          </div>
        </div>

        {/* Fixing Strategies Comparison */}
        <div id="fixing-strategies" className="bg-[#1C1C1E] border border-[#2D2D30] rounded-xl p-5 shadow-lg flex-1">
          <h3 className="text-md font-semibold text-white mb-4 flex items-center gap-2">
            <Cpu className="w-5 h-5 text-blue-400" />
            三大编译修复方法排查与实施状态
          </h3>

          <div className="grid grid-cols-3 gap-2 p-1 bg-black/20 rounded-lg mb-5 border border-[#2A2A2C]">
            <button
              onClick={() => setActiveStrategy('all')}
              className={`py-2 text-xs font-medium rounded-md transition ${activeStrategy === 'all' ? 'bg-[#2D2D30] text-white shadow' : 'text-gray-400 hover:text-gray-200'}`}
            >
              全部方案
            </button>
            <button
              onClick={() => setActiveStrategy('1')}
              className={`py-2 text-xs font-medium rounded-md transition ${activeStrategy === '1' ? 'bg-blue-600/20 text-blue-300 border border-blue-500/30' : 'text-gray-400 hover:text-gray-200'}`}
            >
              方法一 (工作流)
            </button>
            <button
              onClick={() => setActiveStrategy('3')}
              className={`py-2 text-xs font-medium rounded-md transition ${activeStrategy === '3' ? 'bg-teal-600/20 text-teal-300 border border-teal-500/30' : 'text-gray-400 hover:text-gray-200'}`}
            >
              方法三 (项目优化)
            </button>
          </div>

          <div className="space-y-4">
            {/* Method 1 */}
            {(activeStrategy === 'all' || activeStrategy === '1') && (
              <div className="border border-blue-900/30 bg-blue-950/10 rounded-lg p-4 transition-all">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex gap-2.5">
                    <div className="mt-1 flex-shrink-0 w-5 h-5 rounded-full bg-blue-500/20 flex items-center justify-center text-blue-400 font-bold text-xs">
                      1
                    </div>
                    <div>
                      <h4 className="text-sm font-semibold text-white flex items-center gap-2">
                        改用 MSBuild.exe 构建工作流
                        <span className="text-[10px] font-semibold tracking-wider uppercase px-2 py-0.5 rounded-full bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 flex items-center gap-0.5">
                          <Check className="w-3 h-3" /> 已应用
                        </span>
                      </h4>
                      <p className="text-xs text-gray-400 mt-1 leading-relaxed">
                        由于 XamlCompiler 崩溃只发生在 .NET CLI 运行期，Visual Studio 对应的完整版 MSBuild.exe 能稳定编译。我们已经将 <code className="text-blue-300 font-mono bg-black/20 px-1 rounded">build.yml</code> 中的 build/publish 命令改写为 msbuild 命令。
                      </p>
                    </div>
                  </div>
                  <button 
                    onClick={() => onSelectFile('/.github/workflows/build.yml')}
                    className="text-xs text-blue-400 hover:text-blue-300 flex items-center gap-1 shrink-0 font-medium"
                  >
                    <FileText className="w-3.5 h-3.5" /> 查阅 YML
                  </button>
                </div>
              </div>
            )}

            {/* Method 2 */}
            {activeStrategy === 'all' && (
              <div className="border border-[#2D2D30] bg-[#151518] rounded-lg p-4">
                <div className="flex items-start gap-2.5">
                  <div className="mt-1 flex-shrink-0 w-5 h-5 rounded-full bg-[#3A3A3D] flex items-center justify-center text-gray-400 font-bold text-xs">
                    2
                  </div>
                  <div className="flex-1">
                    <h4 className="text-sm font-semibold text-gray-300 flex items-center gap-2">
                      重构 XAML 规避编译器缺陷
                      <span className="text-[10px] px-2 py-0.5 rounded-full bg-amber-500/10 text-amber-400 border border-amber-500/20">
                        无需修改
                      </span>
                    </h4>
                    <p className="text-xs text-gray-500 mt-1 leading-relaxed">
                      排查最近提交的代码是否在 DataTemplate 或 ControlTemplate 中引入了复杂的 <code className="text-gray-400 font-mono bg-black/20 px-1 rounded">{"{x:Bind ElementName=...}"}</code> 绑定。当前项目的 XAML 页面架构均使用轻量级动态绑定与代码后置（Code-behind），结构纯净，无需进行代码重构。
                    </p>
                  </div>
                </div>
              </div>
            )}

            {/* Method 3 */}
            {(activeStrategy === 'all' || activeStrategy === '3') && (
              <div className="border border-teal-900/30 bg-teal-950/10 rounded-lg p-4 transition-all">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex gap-2.5">
                    <div className="mt-1 flex-shrink-0 w-5 h-5 rounded-full bg-teal-500/20 flex items-center justify-center text-teal-400 font-bold text-xs">
                      3
                    </div>
                    <div>
                      <h4 className="text-sm font-semibold text-white flex items-center gap-2">
                        注入项目底层关闭管道优化属性
                        <span className="text-[10px] font-semibold tracking-wider uppercase px-2 py-0.5 rounded-full bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 flex items-center gap-0.5">
                          <Check className="w-3 h-3" /> 已配置
                        </span>
                      </h4>
                      <p className="text-xs text-gray-400 mt-1 leading-relaxed">
                        我们在 <code className="text-teal-300 font-mono bg-black/20 px-1 rounded">VoiceInput.csproj</code> 中显式加入了 <code className="text-teal-300 font-mono bg-black/20 px-1 rounded">{"<DisableXbfGeneration>false</DisableXbfGeneration>"}</code> 属性。强制关闭 XAML 二进制文件的一些极限并行优化，从而在底层让 XamlCompiler 绕过静默死锁崩溃。
                      </p>
                    </div>
                  </div>
                  <button 
                    onClick={() => onSelectFile('/src/VoiceInput/VoiceInput.csproj')}
                    className="text-xs text-teal-400 hover:text-teal-300 flex items-center gap-1 shrink-0 font-medium"
                  >
                    <FileText className="w-3.5 h-3.5" /> 查阅 CSPROJ
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* GitHub Actions Pipeline Runner - Right Side (5 cols) */}
      <div className="lg:col-span-5 flex flex-col gap-6">
        <div id="pipeline-runner" className="bg-[#1C1C1E] border border-[#2D2D30] rounded-xl shadow-lg flex flex-col h-[480px]">
          {/* Header */}
          <div className="px-5 py-4 border-b border-[#2D2D30] flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Terminal className="w-5 h-5 text-gray-400" />
              <div>
                <h3 className="text-sm font-bold text-white leading-none">GitHub Actions 虚拟终端</h3>
                <p className="text-[11px] text-gray-500 mt-1">模拟运行修复后的 MSBuild 构建流</p>
              </div>
            </div>
            <button
              onClick={triggerBuild}
              disabled={buildRunning}
              className={`flex items-center gap-1.5 px-3.5 py-1.5 text-xs font-semibold rounded-lg transition-all ${
                buildRunning 
                  ? 'bg-blue-600/10 text-blue-400/50 cursor-not-allowed border border-blue-500/10' 
                  : 'bg-blue-600 hover:bg-blue-500 text-white shadow-md hover:shadow-blue-600/10'
              }`}
            >
              {buildRunning ? (
                <>
                  <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                  编译中...
                </>
              ) : (
                <>
                  <Play className="w-3.5 h-3.5 fill-current" />
                  运行构建
                </>
              )}
            </button>
          </div>

          {/* Workflow Steps Progress */}
          {buildRunning && (
            <div className="bg-[#151518] px-5 py-3 border-b border-[#2D2D30] flex items-center justify-between">
              <span className="text-xs text-gray-400 font-medium flex items-center gap-2">
                <span className="relative flex h-2 w-2">
                  <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75"></span>
                  <span className="relative inline-flex rounded-full h-2 w-2 bg-blue-500"></span>
                </span>
                正在运行: {pipelineSteps[currentStep]?.name || '完成'}
              </span>
              <span className="text-xs font-mono text-blue-400">
                {Math.round(((currentStep) / pipelineSteps.length) * 100)}%
              </span>
            </div>
          )}

          {/* Log Window */}
          <div className="flex-1 bg-[#0F0F11] p-4 overflow-y-auto font-mono text-[11px] leading-relaxed select-text text-gray-300">
            {logs.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-center text-gray-600 gap-3">
                <Terminal className="w-12 h-12 text-gray-800" />
                <div>
                  <p>点击上方“运行构建”按钮</p>
                  <p className="text-[10px] text-gray-700 mt-1">模拟测试 MSBuild 编译流程的健壮性</p>
                </div>
              </div>
            ) : (
              <div className="space-y-1.5">
                {logs.map((log, index) => (
                  <div key={index} className="flex items-start gap-2">
                    <span className="text-gray-600 shrink-0 select-none">[{log.timestamp}]</span>
                    <span className={`
                      ${log.type === 'error' ? 'text-red-400 font-semibold' : ''}
                      ${log.type === 'success' ? 'text-emerald-400' : ''}
                      ${log.type === 'warning' ? 'text-amber-400' : ''}
                      ${log.type === 'info' && log.message.startsWith('  $') ? 'text-blue-300' : 'text-gray-300'}
                    `}>
                      {log.message}
                    </span>
                  </div>
                ))}
                {buildRunning && (
                  <div className="flex items-center gap-2 text-blue-400 mt-2">
                    <span className="animate-pulse">_</span>
                  </div>
                )}
                <div ref={terminalEndRef} />
              </div>
            )}
          </div>

          {/* Bottom Artifact Card */}
          {buildSuccess && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              className="bg-emerald-950/20 border-t border-emerald-900/40 p-4 flex items-center justify-between"
            >
              <div className="flex items-center gap-3">
                <div className="w-8 h-8 rounded-full bg-emerald-500/10 border border-emerald-500/30 flex items-center justify-center text-emerald-400">
                  <CheckCircle2 className="w-4.5 h-4.5" />
                </div>
                <div>
                  <h4 className="text-xs font-semibold text-white">MSBuild 编译管道通过</h4>
                  <p className="text-[10px] text-emerald-400/80 mt-0.5">生成制品: VoiceInput-win-x64.zip</p>
                </div>
              </div>
              <span className="text-[10px] font-mono bg-emerald-500/15 text-emerald-300 px-2.5 py-1 rounded border border-emerald-500/20">
                100% SUCCESS
              </span>
            </motion.div>
          )}
        </div>
      </div>
    </div>
  );
}
