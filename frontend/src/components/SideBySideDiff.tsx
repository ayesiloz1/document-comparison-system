import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

interface Change {
  type: 'added' | 'removed' | 'modified';
  oldText?: string;
  newText?: string;
  context?: string;
  page?: number;
  severity: 'Low' | 'Medium' | 'High';
}

interface AIInsight {
  summary: string;
  keyChanges: string[];
  recommendations: string[];
  impact: string;
}

interface SideBySideDiffProps {
  changes: Change[];
  aiInsights?: AIInsight;
  isLoading?: boolean;
}

const SideBySideDiff: React.FC<SideBySideDiffProps> = ({ changes, aiInsights, isLoading }) => {
  const [selectedChange, setSelectedChange] = useState<number | null>(null);
  const [showAIPanel, setShowAIPanel] = useState(true);

  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'High': return '#ff4757';
      case 'Medium': return '#ffa502';
      case 'Low': return '#2ed573';
      default: return '#747d8c';
    }
  };

  const getChangeIcon = (type: string) => {
    switch (type) {
      case 'added': return '✅';
      case 'removed': return '❌';
      case 'modified': return '🔄';
      default: return '📝';
    }
  };

  return (
    <div className="side-by-side-diff">
      <style>{`
        .side-by-side-diff {
          display: flex;
          flex-direction: column;
          height: 100%;
          background: #f8f9fa;
          border-radius: 12px;
          overflow: hidden;
        }

        .diff-header {
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          color: white;
          padding: 20px;
          display: flex;
          justify-content: space-between;
          align-items: center;
        }

        .diff-title {
          font-size: 1.5rem;
          font-weight: 600;
          margin: 0;
        }

        .ai-toggle {
          background: rgba(255, 255, 255, 0.2);
          border: none;
          color: white;
          padding: 8px 16px;
          border-radius: 20px;
          cursor: pointer;
          transition: all 0.3s ease;
        }

        .ai-toggle:hover {
          background: rgba(255, 255, 255, 0.3);
        }

        .diff-container {
          display: flex;
          flex: 1;
          overflow: hidden;
        }

        .changes-panel {
          flex: 1;
          display: flex;
          flex-direction: column;
          border-right: 2px solid #e9ecef;
        }

        .diff-content {
          display: flex;
          flex: 1;
          overflow: hidden;
        }

        .diff-side {
          flex: 1;
          overflow-y: auto;
          padding: 20px;
          background: #fff;
        }

        .diff-side.old {
          border-right: 1px solid #e9ecef;
          background: #fff5f5;
        }

        .diff-side.new {
          background: #f0fff4;
        }

        .diff-side h3 {
          margin: 0 0 20px 0;
          color: #495057;
          font-size: 1.2rem;
          font-weight: 600;
          display: flex;
          align-items: center;
          gap: 10px;
        }

        .change-item {
          margin-bottom: 20px;
          padding: 15px;
          border-radius: 8px;
          border-left: 4px solid transparent;
          cursor: pointer;
          transition: all 0.3s ease;
          background: white;
          box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .change-item:hover {
          transform: translateX(5px);
          box-shadow: 0 4px 8px rgba(0,0,0,0.15);
        }

        .change-item.selected {
          background: #e3f2fd;
          border-left-color: #2196f3;
        }

        .change-header {
          display: flex;
          justify-content: between;
          align-items: center;
          margin-bottom: 10px;
        }

        .change-type {
          display: flex;
          align-items: center;
          gap: 8px;
          font-weight: 600;
          font-size: 0.9rem;
        }

        .severity-badge {
          padding: 4px 8px;
          border-radius: 12px;
          font-size: 0.8rem;
          font-weight: 600;
          color: white;
        }

        .change-text {
          font-family: 'Courier New', monospace;
          font-size: 0.9rem;
          line-height: 1.5;
          white-space: pre-wrap;
          background: #f8f9fa;
          padding: 10px;
          border-radius: 4px;
          margin: 10px 0;
        }

        .change-text.removed {
          background: #ffebee;
          color: #c62828;
        }

        .change-text.added {
          background: #e8f5e8;
          color: #2e7d32;
        }

        .ai-insights-panel {
          width: 400px;
          background: white;
          border-left: 2px solid #e9ecef;
          display: flex;
          flex-direction: column;
        }

        .ai-insights-header {
          background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
          color: white;
          padding: 20px;
          display: flex;
          align-items: center;
          gap: 10px;
        }

        .ai-insights-content {
          flex: 1;
          overflow-y: auto;
          padding: 20px;
        }

        .insight-section {
          margin-bottom: 25px;
        }

        .insight-title {
          font-size: 1.1rem;
          font-weight: 600;
          color: #495057;
          margin-bottom: 10px;
          display: flex;
          align-items: center;
          gap: 8px;
        }

        .insight-content {
          color: #6c757d;
          line-height: 1.6;
        }

        .insight-list {
          list-style: none;
          padding: 0;
        }

        .insight-list li {
          padding: 8px 0;
          border-bottom: 1px solid #f1f3f4;
          display: flex;
          align-items: flex-start;
          gap: 10px;
        }

        .insight-list li:last-child {
          border-bottom: none;
        }

        .loading-spinner {
          display: flex;
          justify-content: center;
          align-items: center;
          height: 200px;
          flex-direction: column;
          gap: 15px;
        }

        .spinner {
          width: 40px;
          height: 40px;
          border: 4px solid #f3f3f3;
          border-top: 4px solid #667eea;
          border-radius: 50%;
          animation: spin 1s linear infinite;
        }

        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }

        .empty-state {
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          height: 300px;
          color: #6c757d;
          font-size: 1.1rem;
        }

        .stats-bar {
          background: #f8f9fa;
          padding: 15px 20px;
          border-bottom: 1px solid #e9ecef;
          display: flex;
          justify-content: space-between;
          align-items: center;
        }

        .stat-item {
          display: flex;
          align-items: center;
          gap: 8px;
          font-size: 0.9rem;
          color: #495057;
        }
      `}</style>

      <div className="diff-header">
        <h2 className="diff-title">🔍 Side-by-Side Comparison</h2>
        <button 
          className="ai-toggle"
          onClick={() => setShowAIPanel(!showAIPanel)}
        >
          {showAIPanel ? '🧠 Hide AI' : '🧠 Show AI'}
        </button>
      </div>

      {changes.length === 0 ? (
        <div className="empty-state">
          <div style={{ fontSize: '3rem', marginBottom: '20px' }}>📄</div>
          <div>No changes detected between documents</div>
        </div>
      ) : (
        <>
          <div className="stats-bar">
            <div className="stat-item">
              <span>📊 Total Changes:</span>
              <strong>{changes.length}</strong>
            </div>
            <div className="stat-item">
              <span>➕ Added:</span>
              <strong style={{ color: '#2ed573' }}>
                {changes.filter(c => c.type === 'added').length}
              </strong>
            </div>
            <div className="stat-item">
              <span>➖ Removed:</span>
              <strong style={{ color: '#ff4757' }}>
                {changes.filter(c => c.type === 'removed').length}
              </strong>
            </div>
            <div className="stat-item">
              <span>🔄 Modified:</span>
              <strong style={{ color: '#ffa502' }}>
                {changes.filter(c => c.type === 'modified').length}
              </strong>
            </div>
          </div>

          <div className="diff-container">
            <div className="changes-panel">
              <div className="diff-content">
                <div className="diff-side old">
                  <h3>📄 Original Document</h3>
                  {changes.map((change, index) => (
                    <motion.div
                      key={index}
                      className={`change-item ${selectedChange === index ? 'selected' : ''}`}
                      onClick={() => setSelectedChange(selectedChange === index ? null : index)}
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                    >
                      <div className="change-header">
                        <div className="change-type">
                          {getChangeIcon(change.type)}
                          {change.type.toUpperCase()}
                          {change.page && <span>• Page {change.page}</span>}
                        </div>
                        <div 
                          className="severity-badge"
                          style={{ backgroundColor: getSeverityColor(change.severity) }}
                        >
                          {change.severity}
                        </div>
                      </div>
                      {change.oldText && (
                        <div className="change-text removed">
                          {change.oldText.substring(0, 200)}
                          {change.oldText.length > 200 && '...'}
                        </div>
                      )}
                      {change.context && (
                        <div style={{ fontSize: '0.8rem', color: '#6c757d', marginTop: '10px' }}>
                          Context: {change.context}
                        </div>
                      )}
                    </motion.div>
                  ))}
                </div>

                <div className="diff-side new">
                  <h3>📄 Updated Document</h3>
                  {changes.map((change, index) => (
                    <motion.div
                      key={index}
                      className={`change-item ${selectedChange === index ? 'selected' : ''}`}
                      onClick={() => setSelectedChange(selectedChange === index ? null : index)}
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                    >
                      <div className="change-header">
                        <div className="change-type">
                          {getChangeIcon(change.type)}
                          {change.type.toUpperCase()}
                          {change.page && <span>• Page {change.page}</span>}
                        </div>
                        <div 
                          className="severity-badge"
                          style={{ backgroundColor: getSeverityColor(change.severity) }}
                        >
                          {change.severity}
                        </div>
                      </div>
                      {change.newText && (
                        <div className="change-text added">
                          {change.newText.substring(0, 200)}
                          {change.newText.length > 200 && '...'}
                        </div>
                      )}
                      {change.context && (
                        <div style={{ fontSize: '0.8rem', color: '#6c757d', marginTop: '10px' }}>
                          Context: {change.context}
                        </div>
                      )}
                    </motion.div>
                  ))}
                </div>
              </div>
            </div>

            <AnimatePresence>
              {showAIPanel && (
                <motion.div
                  className="ai-insights-panel"
                  initial={{ width: 0, opacity: 0 }}
                  animate={{ width: 400, opacity: 1 }}
                  exit={{ width: 0, opacity: 0 }}
                  transition={{ duration: 0.3 }}
                >
                  <div className="ai-insights-header">
                    <span>🧠</span>
                    <h3 style={{ margin: 0 }}>AI Insights</h3>
                  </div>
                  
                  <div className="ai-insights-content">
                    {isLoading ? (
                      <div className="loading-spinner">
                        <div className="spinner"></div>
                        <div>Analyzing with GPT...</div>
                      </div>
                    ) : aiInsights ? (
                      <>
                        <div className="insight-section">
                          <div className="insight-title">
                            📋 Summary
                          </div>
                          <div className="insight-content">
                            {aiInsights.summary}
                          </div>
                        </div>

                        <div className="insight-section">
                          <div className="insight-title">
                            🔑 Key Changes
                          </div>
                          <ul className="insight-list">
                            {aiInsights.keyChanges.map((change, index) => (
                              <li key={index}>
                                <span>•</span>
                                <span>{change}</span>
                              </li>
                            ))}
                          </ul>
                        </div>

                        <div className="insight-section">
                          <div className="insight-title">
                            💡 Recommendations
                          </div>
                          <ul className="insight-list">
                            {aiInsights.recommendations.map((rec, index) => (
                              <li key={index}>
                                <span>💡</span>
                                <span>{rec}</span>
                              </li>
                            ))}
                          </ul>
                        </div>

                        <div className="insight-section">
                          <div className="insight-title">
                            📊 Impact Assessment
                          </div>
                          <div className="insight-content">
                            {aiInsights.impact}
                          </div>
                        </div>
                      </>
                    ) : (
                      <div className="empty-state">
                        <div style={{ fontSize: '2rem', marginBottom: '15px' }}>🤖</div>
                        <div>AI insights will appear here</div>
                        <div style={{ fontSize: '0.9rem', marginTop: '10px', color: '#adb5bd' }}>
                          Upload documents to get AI-powered analysis
                        </div>
                      </div>
                    )}
                  </div>
                </motion.div>
              )}
            </AnimatePresence>
          </div>
        </>
      )}
    </div>
  );
};

export default SideBySideDiff;