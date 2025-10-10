import React from 'react';
import './SummaryPanel.css';

interface Props {
  summary: string;
  similarityScore: number;
}

const SummaryPanel: React.FC<Props> = ({ summary, similarityScore }) => {
  const getSimilarityLevel = (score: number) => {
    if (score >= 0.9) return { level: 'Excellent', color: '#059669', bgColor: '#d1fae5' };
    if (score >= 0.8) return { level: 'Very High', color: '#059669', bgColor: '#d1fae5' };
    if (score >= 0.7) return { level: 'High', color: '#d97706', bgColor: '#fef3c7' };
    if (score >= 0.5) return { level: 'Moderate', color: '#d97706', bgColor: '#fef3c7' };
    if (score >= 0.3) return { level: 'Low', color: '#dc2626', bgColor: '#fee2e2' };
    return { level: 'Very Low', color: '#dc2626', bgColor: '#fee2e2' };
  };

  const similarityInfo = getSimilarityLevel(similarityScore);

  return (
    <div className="professional-summary-panel">
      <div className="summary-header">
        <div className="summary-title">
          <h3>Document Analysis Summary</h3>
          <div className="summary-timestamp">
            Generated on {new Date().toLocaleDateString('en-US', { 
              year: 'numeric', 
              month: 'long', 
              day: 'numeric',
              hour: '2-digit',
              minute: '2-digit'
            })}
          </div>
        </div>
        
        <div className="similarity-indicator">
          <div className="similarity-score-large">
            <div 
              className="score-ring" 
              style={{ 
                background: `conic-gradient(${similarityInfo.color} ${similarityScore * 360}deg, #e5e7eb 0deg)` 
              }}
            >
              <div className="score-inner">
                <span className="score-percentage">{(similarityScore * 100).toFixed(1)}%</span>
                <span className="score-label">Similarity</span>
              </div>
            </div>
          </div>
          <div className="similarity-badge" style={{ 
            backgroundColor: similarityInfo.bgColor, 
            color: similarityInfo.color 
          }}>
            {similarityInfo.level} Match
          </div>
        </div>
      </div>
      
      <div className="summary-content">
        <div className="content-section">
          <h4>Executive Summary</h4>
          <p className="summary-text">{summary}</p>
        </div>
        
        <div className="key-metrics">
          <div className="metric-card">
            <div className="metric-icon">📊</div>
            <div className="metric-details">
              <span className="metric-value">{(similarityScore * 100).toFixed(1)}%</span>
              <span className="metric-label">Overall Similarity</span>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-icon">🔍</div>
            <div className="metric-details">
              <span className="metric-value">{similarityInfo.level}</span>
              <span className="metric-label">Match Level</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SummaryPanel;
