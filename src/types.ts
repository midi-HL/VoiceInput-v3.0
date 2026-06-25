export interface SourceFile {
  path: string;
  name: string;
  language: 'xml' | 'yaml' | 'csharp';
  content: string;
  description: string;
}

export interface BuildLogLine {
  timestamp: string;
  type: 'info' | 'warning' | 'error' | 'success';
  message: string;
}

export interface SimulationState {
  isRecording: boolean;
  audioLevel: number;
  transcript: string;
  isProcessing: boolean;
  isCorrected: boolean;
  correctedText: string;
  statusText: string;
}
