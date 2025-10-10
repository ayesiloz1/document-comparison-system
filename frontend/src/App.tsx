import React, { useState, useRef, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { exportReport, compareSections } from './utils/api';
import { ComparisonResult, SectionComparisonResult } from './utils/types';
import { createStructuredAnalysis } from './utils/aiFormatting';
import SectionChunkView from './components/SectionChunkView';
import Logo from './components/Logo';
import LogoIcon from './components/LogoIcon';
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
      content: "Welcome to DocuVision- Advanced AI-powered document analysis. Upload two PDF documents to discover intelligent insights and comprehensive comparisons.",
      timestamp: new Date()
    }
  ]);
  const [files, setFiles] = useState<{ file1: File | null; file2: File | null }>({
    file1: null,
    file2: null
  });
  const [isLoading, setIsLoading] = useState(false);
  const [dragOver, setDragOver] = useState(false);
  const [viewMode, setViewMode] = useState<'chat' | 'sections'>('chat');

  const [sectionResult, setSectionResult] = useState<SectionComparisonResult | null>(null);
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
    addMessage('user', 'Compare these documents');
    
    setTimeout(() => {
      addMessage('bot', 'Analyzing your documents with AI-powered section comparison. This may take a moment.');
    }, 500);

    try {
      const result = await compareSections(files.file1, files.file2);
      console.log('Section comparison result:', result);
      setSectionResult(result);
      
      const resultComponent = (
        <div className="result-card">
          {result.aiSectionInsights && (
            <div className="ai-summary">
              <h4>AI Analysis Summary</h4>
              {createStructuredAnalysis(result.aiSectionInsights.overallSummary)}
            </div>
          )}
          
          <div className="action-buttons">
            <button 
              className="view-btn"
              onClick={() => setViewMode('sections')}
            >
              Section-by-Section View
            </button>
            <button 
              className="export-btn"
              onClick={() => handleSectionExport(result)}
            >
              Export Report
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
        addMessage('bot', `Error: There was an issue comparing your documents: ${error instanceof Error ? error.message : 'Unknown error'}. Please ensure both files are valid PDFs and try again.`);
      }, 1000);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSectionExport = async (result: SectionComparisonResult) => {
    try {
      // Convert SectionComparisonResult to ComparisonResult for export
      const exportResult: ComparisonResult = {
        summary: result.aiSectionInsights.overallSummary,
        similarityScore: result.overallSimilarity,
        diffSegments: [], // Empty for now as we're using section-based comparison
        aiInsights: {
          summary: result.aiSectionInsights.overallSummary,
          keyChanges: result.aiSectionInsights.keyChanges.map(change => change.aiSummary),
          recommendations: [],
          impact: 'See section-by-section analysis for detailed impact assessment'
        }
      };
      await exportReport(exportResult);
      addMessage('bot', 'Report exported successfully. Check your downloads folder.');
    } catch (error) {
      addMessage('bot', 'Failed to export report. Please try again.');
    }
  };

  const removeFile = (fileNumber: 1 | 2) => {
    setFiles(prev => ({
      ...prev,
      [`file${fileNumber}`]: null
    }));
    addMessage('user', `Removed ${fileNumber === 1 ? 'first' : 'second'} document`);
  };

  const TypingIndicator = () => (
    <motion.div 
      className="message bot"
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
    >
      <div className="avatar bot">
        <LogoIcon size={20} animated={true} />
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



  if (viewMode === 'sections' && sectionResult) {
    return (
      <div className="app">
        <div className="header">
          <div className="header-brand">
            <Logo size="medium" />
          </div>
          <div className="view-mode-toggle">
            <p>AI-powered section-by-section document analysis</p>
            <button 
              className="back-btn toggle-button"
              onClick={() => setViewMode('chat')}
            >
              ← Back to Overview
            </button>
          </div>
        </div>
        <div className="main-content">
          <SectionChunkView 
            sectionComparisons={sectionResult.sectionComparisons}
            aiInsights={sectionResult.aiSectionInsights}
            documentAName={sectionResult.documentAName}
            documentBName={sectionResult.documentBName}
            overallSimilarity={sectionResult.overallSimilarity}
            isLoading={false}
          />
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <div className="header">
        <div className="header-brand">
          <Logo size="large" />
        </div>
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
                  {message.type === 'bot' ? <LogoIcon size={20} /> : 'U'}
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
            <div className="upload-icon"></div>
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
                  <span className="file-icon">PDF</span>
                  <span>{files.file1.name}</span>
                  <span className="remove-btn remove-file-btn" onClick={() => removeFile(1)}>×</span>
                </div>
              )}
              {files.file2 && (
                <div className="file-item">
                  <span className="file-icon">PDF</span>
                  <span>{files.file2.name}</span>
                  <span className="remove-btn remove-file-btn" onClick={() => removeFile(2)}>×</span>
                </div>
              )}
            </div>
          )}

          <button
            className="compare-btn"
            onClick={handleCompare}
            disabled={!files.file1 || !files.file2 || isLoading}
          >
            {isLoading ? 'Analyzing...' :  'Compare Documents'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default App;