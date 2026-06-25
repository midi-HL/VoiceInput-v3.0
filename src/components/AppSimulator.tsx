import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Mic, Settings, Volume2, Globe, Sliders, Keyboard, Play, Pause, Save, UploadCloud, RefreshCw, Sparkles, Check, CheckCircle2 } from 'lucide-react';
import { SimulationState } from '../types';

export default function AppSimulator() {
  const [activeTab, setActiveTab] = useState<'home' | 'lyrics' | 'subtitle' | 'settings'>('home');
  const [state, setState] = useState<SimulationState>({
    isRecording: false,
    audioLevel: 0,
    transcript: '',
    isProcessing: false,
    isCorrected: false,
    correctedText: '',
    statusText: '闲置'
  });

  // Settings state
  const [apiKey, setApiKey] = useState<string>('mimo_sk_82f1a94bb2e841f4a6');
  const [asrLanguage, setAsrLanguage] = useState<string>('auto');
  const [hotkey, setHotkey] = useState<string>('RightAlt');
  const [enableLlm, setEnableLlm] = useState<boolean>(true);
  const [showSavedNotification, setShowSavedNotification] = useState<boolean>(false);

  // Lyrics simulation state
  const [lyricFile, setLyricFile] = useState<string | null>(null);
  const [lyricResult, setLyricResult] = useState<string[]>([]);
  const [lyricsProcessing, setLyricsProcessing] = useState<boolean>(false);

  // Audio bars level animation during recording
  useEffect(() => {
    let interval: NodeJS.Timeout;
    if (state.isRecording) {
      interval = setInterval(() => {
        setState(prev => ({
          ...prev,
          audioLevel: Math.floor(Math.random() * 80) + 20
        }));
      }, 100);
    } else {
      setState(prev => ({ ...prev, audioLevel: 0 }));
    }
    return () => clearInterval(interval);
  }, [state.isRecording]);

  const toggleRecording = () => {
    if (state.isRecording) {
      // Stop recording, start processing
      setState(prev => ({
        ...prev,
        isRecording: false,
        isProcessing: true,
        statusText: '正在调用 MiMo-V2.5-ASR 识别...'
      }));

      setTimeout(() => {
        setState(prev => ({
          ...prev,
          transcript: '今天我们来解决 XamlCompiler 进程在 Release 增量编译时的崩溃问题。',
          statusText: enableLlm ? '正在调用 LLM 进行智能纠错和标点完善...' : '识别完成'
        }));

        if (enableLlm) {
          setTimeout(() => {
            setState(prev => ({
              ...prev,
              isProcessing: false,
              isCorrected: true,
              correctedText: '今天，我们来解决 XamlCompiler 进程在 Release 增量编译时的崩溃问题。',
              statusText: '就绪'
            }));
          }, 1500);
        } else {
          setState(prev => ({
            ...prev,
            isProcessing: false,
            isCorrected: false,
            correctedText: '',
            statusText: '就绪'
          }));
        }
      }, 1500);

    } else {
      // Start recording
      setState({
        isRecording: true,
        audioLevel: 30,
        transcript: '',
        isProcessing: false,
        isCorrected: false,
        correctedText: '',
        statusText: '录音中，请说话...'
      });
    }
  };

  const handleUploadFile = () => {
    setLyricsProcessing(true);
    setLyricFile('meeting_audio_recording.wav');
    setTimeout(() => {
      setLyricsProcessing(false);
      setLyricResult([
        '[00:01.20] 大家好，今天项目例会开始。',
        '[00:04.50] 我们重点核对一下 Windows 语音输入法的打包发布进度。',
        '[00:09.15] 目前 GitHub Actions 上的打包编译报错已经被我们彻底解决。',
        '[00:14.30] 我们成功将 MSBuild.exe 替换为主要编译内核。'
      ]);
    }, 2000);
  };

  const saveSettings = () => {
    setShowSavedNotification(true);
    setTimeout(() => setShowSavedNotification(false), 2000);
  };

  return (
    <div id="app-simulator-root" className="bg-[#1C1C1E] border border-[#2D2D30] rounded-xl shadow-xl overflow-hidden flex flex-col h-[550px] relative">
      
      {/* Top Windows Chrome Bar */}
      <div className="bg-[#18181A] px-4 py-2 border-b border-[#2D2D30] flex items-center justify-between">
        <div className="flex items-center gap-1.5">
          <div className="w-2.5 h-2.5 rounded-full bg-red-500/85"></div>
          <div className="w-2.5 h-2.5 rounded-full bg-yellow-500/85"></div>
          <div className="w-2.5 h-2.5 rounded-full bg-green-500/85"></div>
          <span className="text-xs font-medium text-gray-400 ml-2 font-mono">WinUI 3 交互式虚拟机沙盒</span>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-[10px] font-mono bg-blue-500/10 text-blue-400 border border-blue-500/20 px-2 py-0.5 rounded">
            API: Connected
          </span>
          {enableLlm && (
            <span className="text-[10px] font-mono bg-purple-500/10 text-purple-400 border border-purple-500/20 px-2 py-0.5 rounded flex items-center gap-0.5">
              <Sparkles className="w-3 h-3" /> LLM Active
            </span>
          )}
        </div>
      </div>

      <div className="flex-1 flex overflow-hidden">
        {/* Sidebar Nav */}
        <div className="w-48 bg-[#141416] border-r border-[#2D2D30] p-3 flex flex-col gap-1.5">
          <div className="flex items-center gap-2 px-3 py-2.5 mb-2 bg-[#2D2D30]/20 rounded-lg">
            <Mic className="w-5 h-5 text-blue-500" />
            <span className="text-sm font-semibold text-white">语音输入 HUD</span>
          </div>

          <button
            onClick={() => setActiveTab('home')}
            className={`flex items-center gap-2.5 px-3 py-2 text-xs rounded-md transition text-left ${
              activeTab === 'home' ? 'bg-blue-600/10 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200 hover:bg-[#202024]/30'
            }`}
          >
            <Volume2 className="w-4 h-4" />
            主页控制台
          </button>

          <button
            onClick={() => setActiveTab('subtitle')}
            className={`flex items-center gap-2.5 px-3 py-2 text-xs rounded-md transition text-left ${
              activeTab === 'subtitle' ? 'bg-blue-600/10 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200 hover:bg-[#202024]/30'
            }`}
          >
            <Globe className="w-4 h-4" />
            实时字幕
          </button>

          <button
            onClick={() => setActiveTab('lyrics')}
            className={`flex items-center gap-2.5 px-3 py-2 text-xs rounded-md transition text-left ${
              activeTab === 'lyrics' ? 'bg-blue-600/10 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200 hover:bg-[#202024]/30'
            }`}
          >
            <Play className="w-4 h-4" />
            歌词识别
          </button>

          <button
            onClick={() => setActiveTab('settings')}
            className={`flex items-center gap-2.5 px-3 py-2 text-xs rounded-md transition text-left ${
              activeTab === 'settings' ? 'bg-blue-600/10 text-blue-400 font-medium' : 'text-gray-400 hover:text-gray-200 hover:bg-[#202024]/30'
            }`}
          >
            <Settings className="w-4 h-4" />
            系统设置
          </button>

          <div className="mt-auto bg-[#1C1C1E]/50 border border-[#2D2D30] rounded-lg p-3 text-[10px] text-gray-500 leading-normal">
            <Keyboard className="w-3.5 h-3.5 mb-1 text-gray-400" />
            热键触发：<br />
            按住 <code className="text-blue-400 font-mono text-[10px]">{hotkey}</code> 即可直接拉起悬浮录音。
          </div>
        </div>

        {/* Dynamic Display Panel */}
        <div className="flex-1 bg-[#121214] p-5 overflow-y-auto relative flex flex-col">
          
          {/* TAB: Home */}
          {activeTab === 'home' && (
            <div className="space-y-5">
              <div>
                <h3 className="text-base font-bold text-white">语音输入法仪表盘</h3>
                <p className="text-xs text-gray-500 mt-1">
                  基于 .NET 8 / WinUI 3 打造的高效轻量级系统级语音输入助手。
                </p>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div 
                  onClick={() => setActiveTab('subtitle')}
                  className="bg-[#1C1C1E] border border-[#2D2D30] hover:border-blue-500/40 rounded-lg p-4 cursor-pointer transition-all hover:translate-y-[-2px]"
                >
                  <div className="w-8 h-8 rounded-full bg-blue-500/10 flex items-center justify-center text-blue-400 mb-3">
                    <Globe className="w-4.5 h-4.5" />
                  </div>
                  <h4 className="text-xs font-semibold text-white">实时字幕 HUD</h4>
                  <p className="text-[11px] text-gray-500 mt-1">调用 MiMo-V2.5 极速识别并转换实时语音为文字，直接键入光标处。</p>
                </div>

                <div 
                  onClick={() => setActiveTab('lyrics')}
                  className="bg-[#1C1C1E] border border-[#2D2D30] hover:border-blue-500/40 rounded-lg p-4 cursor-pointer transition-all hover:translate-y-[-2px]"
                >
                  <div className="w-8 h-8 rounded-full bg-teal-500/10 flex items-center justify-center text-teal-400 mb-3">
                    <Play className="w-4.5 h-4.5" />
                  </div>
                  <h4 className="text-xs font-semibold text-white">歌词音频识别</h4>
                  <p className="text-[11px] text-gray-500 mt-1">支持本地音频一键上传，并以高精度时间戳生成对应的 LRC 文本文件。</p>
                </div>
              </div>

              {/* Status card */}
              <div className="bg-[#1C1C1E] border border-[#2D2D30] rounded-lg p-4 flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className={`w-3.5 h-3.5 rounded-full flex items-center justify-center ${state.isRecording ? 'bg-red-500 animate-pulse' : 'bg-emerald-500'}`}>
                    {state.isRecording && <div className="w-1.5 h-1.5 rounded-full bg-white"></div>}
                  </div>
                  <div>
                    <h5 className="text-xs font-semibold text-white">语音核心引擎</h5>
                    <p className="text-[10px] text-gray-400 mt-0.5">当前状态: {state.statusText}</p>
                  </div>
                </div>
                <button
                  onClick={toggleRecording}
                  className={`px-4 py-1.5 text-xs font-semibold rounded-lg transition-all ${
                    state.isRecording 
                      ? 'bg-red-600 hover:bg-red-500 text-white' 
                      : 'bg-blue-600 hover:bg-blue-500 text-white'
                  }`}
                >
                  {state.isRecording ? '停止说话' : '模拟触发热键'}
                </button>
              </div>
            </div>
          )}

          {/* TAB: Subtitle (Live Recording) */}
          {activeTab === 'subtitle' && (
            <div className="flex-1 flex flex-col gap-4 justify-between">
              <div>
                <h3 className="text-base font-bold text-white">实时字幕识别</h3>
                <p className="text-xs text-gray-500 mt-1">
                  按下下方按钮或模拟快捷键开始音频采集。
                </p>
              </div>

              {/* Central Transcript Box */}
              <div className="flex-1 bg-[#18181A] border border-[#2D2D30] rounded-xl p-4 flex flex-col justify-between min-h-[160px] overflow-y-auto">
                <div className="space-y-4">
                  {state.transcript && (
                    <div>
                      <span className="text-[10px] font-bold text-blue-400 uppercase tracking-wider block mb-1">ASR 原始文本</span>
                      <p className="text-xs text-gray-300 bg-black/20 p-2.5 rounded border border-[#2D2D30] leading-relaxed">
                        {state.transcript}
                      </p>
                    </div>
                  )}

                  {state.isCorrected && (
                    <motion.div
                      initial={{ opacity: 0, y: 5 }}
                      animate={{ opacity: 1, y: 0 }}
                    >
                      <span className="text-[10px] font-bold text-purple-400 uppercase tracking-wider flex items-center gap-1 mb-1">
                        <Sparkles className="w-3.5 h-3.5" /> LLM 纠错润色后文本
                      </span>
                      <p className="text-xs text-purple-300 bg-purple-500/5 p-2.5 rounded border border-purple-500/20 leading-relaxed font-semibold">
                        {state.correctedText}
                      </p>
                    </motion.div>
                  )}

                  {!state.transcript && !state.isProcessing && !state.isRecording && (
                    <div className="flex flex-col items-center justify-center py-8 text-gray-500 text-center gap-2">
                      <Mic className="w-8 h-8 text-gray-700" />
                      <p className="text-xs">等待录音输入...</p>
                    </div>
                  )}

                  {state.isProcessing && (
                    <div className="flex items-center justify-center py-8 gap-2.5 text-blue-400 text-xs font-medium">
                      <RefreshCw className="w-4 h-4 animate-spin" />
                      <span>{state.statusText}</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="flex items-center justify-center">
                <button
                  onClick={toggleRecording}
                  disabled={state.isProcessing}
                  className={`w-14 h-14 rounded-full flex items-center justify-center transition-all shadow-lg ${
                    state.isRecording
                      ? 'bg-red-600 hover:bg-red-500 text-white animate-pulse'
                      : 'bg-blue-600 hover:bg-blue-500 text-white'
                  }`}
                >
                  <Mic className="w-6 h-6" />
                </button>
              </div>
            </div>
          )}

          {/* TAB: Lyrics */}
          {activeTab === 'lyrics' && (
            <div className="flex-1 flex flex-col gap-4">
              <div>
                <h3 className="text-base font-bold text-white">歌词识别 / 转录</h3>
                <p className="text-xs text-gray-500 mt-1">
                  上传完整的录音文件，高精度转换为带有时间戳标志的 LRC 文件。
                </p>
              </div>

              <div className="border border-dashed border-[#2D2D30] bg-[#1C1C1E] rounded-xl p-6 flex flex-col items-center justify-center text-center gap-3">
                <UploadCloud className="w-10 h-10 text-gray-500" />
                <div>
                  <p className="text-xs font-semibold text-white">拖拽或点击模拟上传文件</p>
                  <p className="text-[10px] text-gray-500 mt-1">支持 *.wav, *.mp3, *.m4a 格式音频</p>
                </div>
                <button
                  onClick={handleUploadFile}
                  disabled={lyricsProcessing}
                  className="bg-teal-600 hover:bg-teal-500 text-white text-xs font-semibold py-1.5 px-4 rounded-lg shadow-md transition"
                >
                  {lyricsProcessing ? '识别中...' : '模拟导入 wav 文件'}
                </button>
              </div>

              {/* Lyrics list */}
              {lyricResult.length > 0 && (
                <div className="bg-[#18181A] border border-[#2D2D30] rounded-lg p-3 flex-1 overflow-y-auto font-mono text-[11px] text-gray-400 space-y-1">
                  {lyricResult.map((line, idx) => (
                    <div key={idx} className="flex gap-4 hover:bg-white/5 py-0.5 px-1 rounded">
                      <span className="text-teal-500 select-none shrink-0">{line.substring(0, 10)}</span>
                      <span className="text-white">{line.substring(10)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* TAB: Settings */}
          {activeTab === 'settings' && (
            <div className="space-y-4 flex flex-col justify-between flex-1">
              <div className="space-y-3.5">
                <div>
                  <h3 className="text-base font-bold text-white">系统运行设置</h3>
                  <p className="text-xs text-gray-500 mt-1">
                    调整本地 Windows 客户端的识别属性。
                  </p>
                </div>

                <div className="space-y-3">
                  <div>
                    <label className="text-xs text-gray-400 block mb-1">MiMo API 密钥</label>
                    <input
                      type="password"
                      value={apiKey}
                      onChange={(e) => setApiKey(e.target.value)}
                      className="w-full bg-[#18181A] border border-[#2D2D30] rounded px-3 py-1.5 text-xs text-white font-mono focus:outline-none focus:border-blue-500"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="text-xs text-gray-400 block mb-1">识别目标语言</label>
                      <select
                        value={asrLanguage}
                        onChange={(e) => setAsrLanguage(e.target.value)}
                        className="w-full bg-[#18181A] border border-[#2D2D30] rounded px-2.5 py-1.5 text-xs text-white focus:outline-none"
                      >
                        <option value="auto">自动识别 (Auto)</option>
                        <option value="zh">仅中文 (zh)</option>
                        <option value="en">仅英文 (en)</option>
                      </select>
                    </div>

                    <div>
                      <label className="text-xs text-gray-400 block mb-1">唤醒快捷键</label>
                      <select
                        value={hotkey}
                        onChange={(e) => setHotkey(e.target.value)}
                        className="w-full bg-[#18181A] border border-[#2D2D30] rounded px-2.5 py-1.5 text-xs text-white focus:outline-none"
                      >
                        <option value="RightAlt">右 Alt 键</option>
                        <option value="LeftAlt">左 Alt 键</option>
                        <option value="Space">空格 键</option>
                      </select>
                    </div>
                  </div>

                  <div className="flex items-center justify-between border-t border-[#2D2D30] pt-3.5">
                    <div>
                      <h4 className="text-xs font-semibold text-white">智能大语言模型（LLM）文字纠错</h4>
                      <p className="text-[10px] text-gray-500 mt-0.5">对识别结果进行标点修饰、语气词删除与逻辑润色。</p>
                    </div>
                    <input
                      type="checkbox"
                      checked={enableLlm}
                      onChange={(e) => setEnableLlm(e.target.checked)}
                      className="w-4 h-4 accent-blue-600 rounded cursor-pointer"
                    />
                  </div>
                </div>
              </div>

              <div className="flex justify-end gap-2 border-t border-[#2D2D30] pt-3 shrink-0">
                <button
                  onClick={saveSettings}
                  className="bg-blue-600 hover:bg-blue-500 text-white text-xs font-semibold px-4 py-1.5 rounded flex items-center gap-1 transition"
                >
                  <Save className="w-3.5 h-3.5" />
                  保存设置
                </button>
              </div>
            </div>
          )}

          {/* Floating Glass Capsule HUD (Visible during simulated recording) */}
          <AnimatePresence>
            {state.isRecording && (
              <motion.div
                initial={{ opacity: 0, y: 30, scale: 0.95 }}
                animate={{ opacity: 1, y: 0, scale: 1 }}
                exit={{ opacity: 0, y: 20, scale: 0.95 }}
                className="absolute bottom-4 left-1/2 -translate-x-1/2 w-72 bg-black/85 backdrop-blur border border-white/10 rounded-full px-4 py-2 flex items-center gap-3.5 shadow-2xl z-20"
              >
                {/* 5 columns of leaping voice bars */}
                <div className="flex items-center gap-0.5 h-4 w-10 shrink-0">
                  <div className="w-0.5 bg-blue-400 rounded-full transition-all duration-75" style={{ height: `${Math.min(100, Math.max(20, state.audioLevel * 0.4))}%%` }} />
                  <div className="w-0.5 bg-blue-400 rounded-full transition-all duration-75" style={{ height: `${Math.min(100, Math.max(20, state.audioLevel * 0.8))}%%` }} />
                  <div className="w-0.5 bg-blue-400 rounded-full transition-all duration-75" style={{ height: `${Math.min(100, Math.max(20, state.audioLevel * 0.9))}%%` }} />
                  <div className="w-0.5 bg-blue-400 rounded-full transition-all duration-75" style={{ height: `${Math.min(100, Math.max(20, state.audioLevel * 0.6))}%%` }} />
                  <div className="w-0.5 bg-blue-400 rounded-full transition-all duration-75" style={{ height: `${Math.min(100, Math.max(20, state.audioLevel * 0.3))}%%` }} />
                </div>
                <span className="text-[11px] font-medium text-white truncate">正在说话并听写中...</span>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Saved Settings Toast Notification */}
          <AnimatePresence>
            {showSavedNotification && (
              <motion.div
                initial={{ opacity: 0, y: -20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -20 }}
                className="absolute top-16 left-1/2 -translate-x-1/2 bg-emerald-500 text-white text-xs font-semibold px-4 py-2 rounded-lg shadow-xl flex items-center gap-2 z-30"
              >
                <CheckCircle2 className="w-4 h-4" />
                配置保存成功！
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>
    </div>
  );
}
