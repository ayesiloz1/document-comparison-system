import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import './SideBySideDiff.css';

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