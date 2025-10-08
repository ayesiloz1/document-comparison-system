import React, { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { compareDocuments, exportReport } from './utils/api';
import { ComparisonResult } from './utils/types';
import SegmentBySideView from './components/SegmentBySideView';
import './index.css';

interface Message {
  id: string;
  type: 'bot' | 'user';
  content: string | JSX.Element;
  timestamp: Date;
}

function App() {
  const [messages, setMessages] = useState<Message[]>([
    {
      id: '1',
      type: 'bot',
      content: "Welcome to the Document Comparison System. Upload two PDF documents and I'll analyze their differences with intelligent insights powered by Azure OpenAI.",
      timestamp: new Date()
    }
  ]);
  const [files, setFiles] = useState<{ file1: File | null; file2: File | null }>({
    file1: null,
    file2: null
  });
  const [isLoading, setIsLoading] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const [viewMode, setViewMode] = useState<'chat' | 'sidebyside'>('chat');
  const [currentResult, setCurrentResult] = useState<ComparisonResult | null>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const file1InputRef = useRef<HTMLInputElement>(null);
  const file2InputRef = useRef<HTMLInputElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const addMessage = (type: 'bot' | 'user', content: string | JSX.Element) => {
    const newMessage: Message = {
      id: Date.now().toString(),
      type,
      content,
      timestamp: new Date()
    };
    setMessages(prev => [...prev, newMessage]);
  };

  const handleFileSelect = (fileNumber: 1 | 2, file: File) => {
    setFiles(prev => ({
      ...prev,
      [`file${fileNumber}`]: file
    }));
    addMessage('user', `Uploaded: ${file.name}`);
    
    if (fileNumber === 1 && !files.file2) {
      setTimeout(() => {
        addMessage('bot', 'Great! Now please upload the second PDF document to compare.');
      }, 500);
    } else if (fileNumber === 2 && !files.file1) {
      setTimeout(() => {
        addMessage('bot', 'Perfect! Now please upload the first PDF document to compare.');
      }, 500);
    } else if ((fileNumber === 1 && files.file2) || (fileNumber === 2 && files.file1)) {
      setTimeout(() => {
        addMessage('bot', 'Both documents uploaded! Click "Compare Documents" to start the analysis.');
      }, 500);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setDragOver(false);
    
    const droppedFiles = Array.from(e.dataTransfer.files);
    const pdfFiles = droppedFiles.filter(file => file.type === 'application/pdf');
    
    if (pdfFiles.length > 0) {
      if (!files.file1) {
        handleFileSelect(1, pdfFiles[0]);
      } else if (!files.file2) {
        handleFileSelect(2, pdfFiles[0]);
      }
    }
  };

  const handleCompare = async () => {
    if (!files.file1 || !files.file2) return;

    setIsLoading(true);
    addMessage('user', '🔍 Compare these documents');
    
    setTimeout(() => {
      addMessage('bot', 'Analyzing your documents... This may take a moment.');
    }, 500);

    try {
      const result = await compareDocuments(files.file1, files.file2);
      console.log('Comparison result:', result);
      setCurrentResult(result);
      
      const resultComponent = (
        <div className="result-card">
          <div className="similarity-score">
            <div className={`score-circle ${
              result.similarityScore > 0.8 ? 'score-high' : 
              result.similarityScore > 0.5 ? 'score-medium' : 'score-low'
            }`}>
              {Math.round(result.similarityScore * 100)}%
            </div>
            <div>
              <h3>Similarity Score</h3>
              <p>Documents are {Math.round(result.similarityScore * 100)}% similar</p>
            </div>
          </div>
          
          {result.aiInsights && (
            <div className="ai-summary">
              <h4>AI Insights</h4>
              <p><strong>Summary:</strong> {result.aiInsights.summary}</p>
              <p><strong>Impact:</strong> {result.aiInsights.impact}</p>
            </div>
          )}
          
          <div className="diff-section">
            <h4>Key Differences ({result.diffSegments.length} changes found)</h4>
            {result.diffSegments.slice(0, 5).map((diff: any, index: number) => {
              console.log('Diff segment:', diff);
              const typeStr = typeof diff.type === 'string' ? diff.type : 
                             typeof diff.type === 'number' ? ['Unchanged', 'Inserted', 'Deleted', 'Modified'][diff.type] || 'Unknown' :
                             String(diff.type);
              return (
                <div key={index} className={`diff-item diff-${typeStr.toLowerCase()}`}>
                  <strong>{typeStr}:</strong> {diff.text.substring(0, 100)}
                  {diff.text.length > 100 && '...'}
                </div>
              );
            })}
            {result.diffSegments.length > 5 && (
              <p>... and {result.diffSegments.length - 5} more differences</p>
            )}
          </div>
          
          <div className="action-buttons">
            <button 
              className="view-btn"
              onClick={() => setViewMode('sidebyside')}
            >
              � Segment-by-Segment View
            </button>
            <button 
              className="export-btn"
              onClick={() => handleExport(result)}
            >
              📥 Export Report
            </button>
          </div>
        </div>
      );

      setTimeout(() => {
        addMessage('bot', resultComponent);
      }, 1500);

    } catch (error) {
      console.error('Comparison error:', error);
      setTimeout(() => {
        addMessage('bot', `❌ Sorry, there was an error comparing your documents: ${error instanceof Error ? error.message : 'Unknown error'}. Please make sure both files are valid PDFs and try again.`);
      }, 1000);
    } finally {
      setIsLoading(false);
    }
  };

  const handleExport = async (result: ComparisonResult) => {
    try {
      await exportReport(result);
      addMessage('bot', '📥 Report exported successfully! Check your downloads folder.');
    } catch (error) {
      addMessage('bot', '❌ Failed to export report. Please try again.');
    }
  };

  const removeFile = (fileNumber: 1 | 2) => {
    setFiles(prev => ({
      ...prev,
      [`file${fileNumber}`]: null
    }));
    addMessage('user', `🗑️ Removed ${fileNumber === 1 ? 'first' : 'second'} document`);
  };

  const TypingIndicator = () => (
    <motion.div 
      className="message bot"
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
    >
      <div className="avatar bot">
        🤖
      </div>
      <div className="message-content">
        <div className="typing-indicator">
          <div className="typing-dots">
            <div className="typing-dot"></div>
            <div className="typing-dot"></div>
            <div className="typing-dot"></div>
          </div>
        </div>
      </div>
    </motion.div>
  );

  // Transform diffSegments for SegmentBySideView component
  const transformSegments = (result: ComparisonResult) => {
    return result.diffSegments.map(segment => ({
      type: segment.type === 'Inserted' ? 'added' as const : 
            segment.type === 'Deleted' ? 'removed' as const : 
            segment.type === 'Modified' ? 'modified' as const : 'unchanged' as const,
      text: segment.text,
      pageA: segment.pageNumberA,
      pageB: segment.pageNumberB,
      severity: segment.severity === 'Minor' ? 'Low' as const : 
               segment.severity === 'Moderate' ? 'Medium' as const : 'High' as const
    }));
  };

  if (viewMode === 'sidebyside' && currentResult) {
    return (
      <div className="app">
        <div className="header">
          <h1>Document Comparison System</h1>
          <div className="view-mode-toggle">
            <p>Professional PDF document analysis with AI insights</p>
            <button 
              className="back-btn toggle-button"
              onClick={() => setViewMode('chat')}
            >
              ← Back to Overview
            </button>
          </div>
        </div>
        <div className="main-content">
          <SegmentBySideView 
            segments={transformSegments(currentResult)}
            aiInsights={currentResult.aiInsights}
            isLoading={false}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <div className="header">
        <h1>Document Comparison System</h1>
        <p>Professional PDF document analysis with AI insights</p>
      </div>

      <div className="chat-container">
        <div className="messages-container">
          <AnimatePresence>
            {messages.map((message) => (
              <motion.div
                key={message.id}
                className={`message ${message.type}`}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
              >
                <div className={`avatar ${message.type}`}>
                  {message.type === 'bot' ? 'AI' : 'U'}
                </div>
                <div className="message-content">
                  {typeof message.content === 'string' ? (
                    <p>{message.content}</p>
                  ) : (
                    message.content
                  )}
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
          
          {isLoading && <TypingIndicator />}
          <div ref={messagesEndRef} />
        </div>

        <div className="input-area">
          <div 
            className={`upload-area ${dragOver ? 'dragging' : ''}`}
            onDrop={handleDrop}
            onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
            onDragLeave={() => setDragOver(false)}
            onClick={() => {
              if (!files.file1) file1InputRef.current?.click();
              else if (!files.file2) file2InputRef.current?.click();
            }}
          >
            <div className="upload-icon">📄</div>
            <p>
              {!files.file1 && !files.file2 
                ? 'Drop your first PDF here or click to browse'
                : !files.file1 
                ? 'Drop your first PDF here or click to browse'
                : !files.file2 
                ? 'Drop your second PDF here or click to browse'
                : 'Both documents uploaded! Ready to compare.'
              }
            </p>
          </div>

          <input
            ref={file1InputRef}
            type="file"
            accept=".pdf"
            onChange={(e) => e.target.files?.[0] && handleFileSelect(1, e.target.files[0])}
            className="file-input"
          />
          <input
            ref={file2InputRef}
            type="file"
            accept=".pdf"
            onChange={(e) => e.target.files?.[0] && handleFileSelect(2, e.target.files[0])}
            className="file-input"
          />

          {(files.file1 || files.file2) && (
            <div className="uploaded-files">
              {files.file1 && (
                <div className="file-item">
                  <span className="file-icon">📄</span>
                  <span>{files.file1.name}</span>
                  <span className="remove-btn remove-file-btn" onClick={() => removeFile(1)}>❌</span>
                </div>
              )}
              {files.file2 && (
                <div className="file-item">
                  <span className="file-icon">📄</span>
                  <span>{files.file2.name}</span>
                  <span className="remove-btn remove-file-btn" onClick={() => removeFile(2)}>❌</span>
                </div>
              )}
            </div>
          )}

          <button
            className="compare-btn"
            onClick={handleCompare}
            disabled={!files.file1 || !files.file2 || isLoading}
          >
            {isLoading ? '🔄 Analyzing...' : '🔍 Compare Documents'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default App;
