import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { SectionComparison, AISectionInsights } from '../utils/types';
import './SectionChunkView.css';

interface SectionChunkViewProps {
  sectionComparisons: SectionComparison[];
  aiInsights: AISectionInsights;
  documentAName: string;
  documentBName: string;
  overallSimilarity: number;
  isLoading: boolean;
}

const SectionChunkView: React.FC<SectionChunkViewProps> = ({
  sectionComparisons,
  aiInsights,
  documentAName,
  documentBName,
  overallSimilarity,
  isLoading
}) => {

  const [filterType, setFilterType] = useState<'all' | 'changed' | 'critical'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const sectionsPerPage = 5;

  const filteredSections = sectionComparisons.filter(section => {
    if (filterType === 'all') return true;
    if (filterType === 'changed') return section.changeType !== 'Unchanged';
    if (filterType === 'critical') return section.severity === 'High' || section.severity === 'Critical';
    return true;
  });

  const totalPages = Math.ceil(filteredSections.length / sectionsPerPage);
  const currentSections = filteredSections.slice(
    (currentPage - 1) * sectionsPerPage,
    currentPage * sectionsPerPage
  );

  const getChangeTypeIcon = (changeType: string) => {
    switch (changeType) {
      case 'Added': return '+';
      case 'Deleted': return '−';
      case 'Modified': return '△';
      case 'Unchanged': return '=';
      default: return '?';
    }
  };

  const getChangeTypeColor = (changeType: string) => {
    switch (changeType) {
      case 'Added': return '#059669';
      case 'Deleted': return '#dc2626';
      case 'Modified': return '#d97706';
      case 'Unchanged': return '#6b7280';
      default: return '#9ca3af';
    }
  };

  if (isLoading) {
    return (
      <div className="section-chunk-view">
        <div className="loading-container">
          <div className="loading-spinner"></div>
          <p>Analyzing document sections...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="section-chunk-view">
      {/* Header with Statistics */}
      <div className="section-header">
        <div className="document-info">
          <h2>Section-by-Section Comparison</h2>
          <div className="document-names">
            <span className="doc-name doc-a">{documentAName}</span>
            <span className="vs">vs</span>
            <span className="doc-name doc-b">{documentBName}</span>
          </div>
        </div>
        
        <div className="similarity-badge">
          <div className="similarity-score" style={{ 
            backgroundColor: overallSimilarity > 0.8 ? '#10b981' : 
                           overallSimilarity > 0.6 ? '#f59e0b' : '#ef4444'
          }}>
            {Math.round(overallSimilarity * 100)}%
          </div>
          <span>Overall Similarity</span>
        </div>
      </div>

      {/* AI Insights Panel */}
      <div className="ai-insights-panel">
        <h3>AI Analysis Summary</h3>
        <p className="ai-summary">{aiInsights.overallSummary}</p>
        
        <div className="change-statistics">
          <div className="stat-item added">
            <span className="stat-number">{aiInsights.changeStatistics.addedSections}</span>
            <span className="stat-label">Added</span>
          </div>
          <div className="stat-item deleted">
            <span className="stat-number">{aiInsights.changeStatistics.deletedSections}</span>
            <span className="stat-label">Deleted</span>
          </div>
          <div className="stat-item modified">
            <span className="stat-number">{aiInsights.changeStatistics.modifiedSections}</span>
            <span className="stat-label">Modified</span>
          </div>
          <div className="stat-item unchanged">
            <span className="stat-number">{aiInsights.changeStatistics.unchangedSections}</span>
            <span className="stat-label">Unchanged</span>
          </div>
        </div>
      </div>

      {/* Filters and Controls */}
      <div className="controls-panel">
        <div className="filters">
          <button 
            className={`filter-btn ${filterType === 'all' ? 'active' : ''}`}
            onClick={() => { setFilterType('all'); setCurrentPage(1); }}
          >
            All Sections ({sectionComparisons.length})
          </button>
          <button 
            className={`filter-btn ${filterType === 'changed' ? 'active' : ''}`}
            onClick={() => { setFilterType('changed'); setCurrentPage(1); }}
          >
            Changed Only ({sectionComparisons.filter(s => s.changeType !== 'Unchanged').length})
          </button>
          <button 
            className={`filter-btn ${filterType === 'critical' ? 'active' : ''}`}
            onClick={() => { setFilterType('critical'); setCurrentPage(1); }}
          >
            Critical ({sectionComparisons.filter(s => s.severity === 'High' || s.severity === 'Critical').length})
          </button>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="pagination">
            <button 
              className="page-btn"
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
            >
              Previous
            </button>
            <span className="page-info">
              Page {currentPage} of {totalPages}
            </span>
            <button 
              className="page-btn"
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
            >
              Next
            </button>
          </div>
        )}
      </div>

      {/* Section Comparisons */}
      <div className="sections-container">
        <AnimatePresence>
          {currentSections.map((comparison, index) => (
            <motion.div
              key={comparison.comparisonId}
              className={`section-comparison ${comparison.changeType.toLowerCase()}`}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              transition={{ delay: index * 0.1 }}
            >
              {/* Section Header */}
              <div className="section-comparison-header">
                <div className="section-meta">
                  <span className="change-type-icon">
                    {getChangeTypeIcon(comparison.changeType)}
                  </span>
                  <span 
                    className="change-type-label"
                    style={{ color: getChangeTypeColor(comparison.changeType) }}
                  >
                    {comparison.changeType}
                  </span>
                  {comparison.similarityScore && (
                    <span className="similarity-score-small">
                      {Math.round(comparison.similarityScore * 100)}% similar
                    </span>
                  )}
                  <span 
                    className={`severity-badge ${comparison.severity.toLowerCase()}`}
                    style={{ 
                      backgroundColor: comparison.severity === 'High' || comparison.severity === 'Critical' ? '#ef4444' :
                                     comparison.severity === 'Medium' ? '#f59e0b' : '#10b981'
                    }}
                  >
                    {comparison.severity}
                  </span>
                </div>
                
                <div className="page-numbers">
                  {comparison.pageNumberA && (
                    <span className="page-number">Page {comparison.pageNumberA}</span>
                  )}
                  {comparison.pageNumberB && comparison.pageNumberA !== comparison.pageNumberB && (
                    <span className="page-number">→ Page {comparison.pageNumberB}</span>
                  )}
                </div>
              </div>

              {/* AI Summary */}
              {comparison.aiSummary && (
                <div className="ai-summary-section">
                  <span className="ai-label">AI Analysis:</span>
                  <span className="ai-text">{comparison.aiSummary}</span>
                </div>
              )}

              {/* Side-by-Side Content */}
              <div className="section-content-comparison">
                {/* Original Version (Document A) */}
                <div className="section-content original">
                  <div className="content-header">
                    <h4>Original ({documentAName})</h4>
                    {comparison.sectionA && (
                      <span className="word-count">
                        {comparison.sectionA.wordCount} words
                      </span>
                    )}
                  </div>
                  <div className="content-body">
                    {comparison.sectionA ? (
                      <>
                        {comparison.sectionA.title && (
                          <div className="section-title">{comparison.sectionA.title}</div>
                        )}
                        <div className="section-text">
                          {comparison.sectionA.content.split('\n').map((line, i) => (
                            <p key={i} className="content-line">{line}</p>
                          ))}
                        </div>
                      </>
                    ) : (
                      <div className="empty-content">Section not present in original document</div>
                    )}
                  </div>
                </div>

                {/* New Version (Document B) */}
                <div className="section-content new">
                  <div className="content-header">
                    <h4>New ({documentBName})</h4>
                    {comparison.sectionB && (
                      <span className="word-count">
                        {comparison.sectionB.wordCount} words
                      </span>
                    )}
                  </div>
                  <div className="content-body">
                    {comparison.sectionB ? (
                      <>
                        {comparison.sectionB.title && (
                          <div className="section-title">{comparison.sectionB.title}</div>
                        )}
                        <div className="section-text">
                          {comparison.sectionB.content.split('\n').map((line, i) => (
                            <p key={i} className="content-line">{line}</p>
                          ))}
                        </div>
                      </>
                    ) : (
                      <div className="empty-content">Section not present in new document</div>
                    )}
                  </div>
                </div>
              </div>
            </motion.div>
          ))}
        </AnimatePresence>
      </div>

      {/* Key Changes Summary */}
      {aiInsights.keyChanges.length > 0 && (
        <div className="key-changes-panel">
          <h3>Key Changes Highlight</h3>
          <div className="key-changes-list">
            {aiInsights.keyChanges.slice(0, 3).map((change, index) => (
              <div key={change.comparisonId} className="key-change-item">
                <span className="change-icon">{getChangeTypeIcon(change.changeType)}</span>
                <div className="change-info">
                  <div className="change-title">
                    {change.sectionA?.title || change.sectionB?.title || 'Untitled Section'}
                  </div>
                  <div className="change-description">{change.aiSummary}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default SectionChunkView;