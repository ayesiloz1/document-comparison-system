import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

interface Segment {
  type: 'added' | 'removed' | 'modified' | 'unchanged';
  text: string;
  pageA?: number;
  pageB?: number;
  severity: 'Low' | 'Medium' | 'High';
}

interface AIInsight {
  summary: string;
  keyChanges: string[];
  recommendations: string[];
  impact: string;
}

interface PageSegment {
  pageNumber: number;
  leftSegments: Segment[];
  rightSegments: Segment[];
}

interface SegmentBySideViewProps {
  segments: Segment[];
  aiInsights?: AIInsight;
  isLoading?: boolean;
}

const SegmentBySideView: React.FC<SegmentBySideViewProps> = ({ segments, aiInsights, isLoading }) => {
  const [selectedPage, setSelectedPage] = useState<number | null>(null);
  const [showAIPanel, setShowAIPanel] = useState(true);
  const [pageSegments, setPageSegments] = useState<PageSegment[]>([]);

  useEffect(() => {
    // Group segments by page for side-by-side display
    const pageMap = new Map<number, { left: Segment[], right: Segment[] }>();
    
    // Collect all page numbers
    const allPages = new Set<number>();
    segments.forEach(segment => {
      if (segment.pageA) allPages.add(segment.pageA);
      if (segment.pageB) allPages.add(segment.pageB);
    });

    // Initialize page map
    Array.from(allPages).forEach(page => {
      pageMap.set(page, { left: [], right: [] });
    });

    // Distribute segments to left/right based on their type
    segments.forEach(segment => {
      const pageNum = segment.pageA || segment.pageB || 1;
      
      if (!pageMap.has(pageNum)) {
        pageMap.set(pageNum, { left: [], right: [] });
      }

      const pageData = pageMap.get(pageNum)!;

      if (segment.type === 'removed') {
        // Show only on left side (original document)
        pageData.left.push(segment);
      } else if (segment.type === 'added') {
        // Show only on right side (new document)
        pageData.right.push(segment);
      } else {
        // Show on both sides for unchanged/modified
        pageData.left.push(segment);
        pageData.right.push(segment);
      }
    });

    // Convert to array and sort by page number
    const pageSegmentsArray = Array.from(pageMap.entries())
      .map(([pageNumber, data]) => ({
        pageNumber,
        leftSegments: data.left,
        rightSegments: data.right
      }))
      .sort((a, b) => a.pageNumber - b.pageNumber);

    setPageSegments(pageSegmentsArray);
  }, [segments]);

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
      case 'added': return '+';
      case 'removed': return '-';
      case 'modified': return '~';
      case 'unchanged': return '=';
      default: return '•';
    }
  };

  const getChangeColor = (type: string) => {
    switch (type) {
      case 'added': return '#e8f5e8';
      case 'removed': return '#ffebee';
      case 'modified': return '#fff3e0';
      case 'unchanged': return '#f8f9fa';
      default: return '#ffffff';
    }
  };

  const getTextColor = (type: string) => {
    switch (type) {
      case 'added': return '#2e7d32';
      case 'removed': return '#c62828';
      case 'modified': return '#f57c00';
      case 'unchanged': return '#424242';
      default: return '#000000';
    }
  };

  return (
    <div className="segment-view">
      <style>{`
        .segment-view {
          display: flex;
          flex-direction: column;
          height: 100%;
          background: #f8f9fa;
          border-radius: 12px;
          overflow: hidden;
        }

        .segment-header {
          background: #1e293b;
          color: white;
          padding: 20px;
          display: flex;
          justify-content: space-between;
          align-items: center;
          border-bottom: 3px solid #3b82f6;
        }

        .segment-title {
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

        .segment-container {
          display: flex;
          flex: 1;
          overflow: hidden;
        }

        .segment-content {
          flex: 1;
          display: flex;
          flex-direction: column;
          overflow: hidden;
        }

        .page-navigator {
          background: white;
          border-bottom: 2px solid #e9ecef;
          padding: 15px 20px;
          display: flex;
          align-items: center;
          gap: 15px;
          overflow-x: auto;
        }

        .page-tab {
          padding: 8px 16px;
          border-radius: 20px;
          background: #f8f9fa;
          border: 2px solid transparent;
          cursor: pointer;
          transition: all 0.3s ease;
          white-space: nowrap;
          font-weight: 500;
        }

        .page-tab.active {
          background: #e3f2fd;
          border-color: #2196f3;
          color: #1976d2;
        }

        .page-tab:hover {
          background: #e9ecef;
        }

        .segments-grid {
          flex: 1;
          display: flex;
          overflow: hidden;
        }

        .segment-side {
          flex: 1;
          overflow-y: auto;
          padding: 20px;
          background: white;
        }

        .segment-side.left {
          border-right: 2px solid #e9ecef;
          background: #fafafa;
        }

        .segment-side.right {
          background: #f0fff4;
        }

        .side-title {
          font-size: 1.2rem;
          font-weight: 600;
          margin-bottom: 20px;
          color: #495057;
          display: flex;
          align-items: center;
          gap: 10px;
          position: sticky;
          top: 0;
          background: inherit;
          padding: 10px 0;
          border-bottom: 1px solid #e9ecef;
        }

        .segment-item {
          margin-bottom: 10px;
          padding: 12px 15px;
          border-radius: 8px;
          border-left: 4px solid transparent;
          transition: all 0.3s ease;
          position: relative;
          line-height: 1.5;
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }

        .segment-item:hover {
          transform: translateX(3px);
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .segment-meta {
          display: flex;
          align-items: center;
          gap: 8px;
          margin-bottom: 8px;
          font-size: 0.85rem;
        }

        .segment-text {
          font-size: 0.95rem;
          word-wrap: break-word;
          white-space: pre-wrap;
        }

        .severity-badge {
          padding: 2px 8px;
          border-radius: 10px;
          font-size: 0.75rem;
          font-weight: 600;
          color: white;
        }

        .page-info {
          background: #f8f9fa;
          padding: 4px 8px;
          border-radius: 8px;
          font-size: 0.8rem;
          color: #6c757d;
        }

        .ai-insights-panel {
          width: 400px;
          background: white;
          border-left: 2px solid #e9ecef;
          display: flex;
          flex-direction: column;
        }

        .ai-insights-header {
          background: #374151;
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

        .stats-bar {
          background: white;
          padding: 15px 20px;
          border-bottom: 1px solid #e9ecef;
          display: flex;
          justify-content: space-between;
          align-items: center;
          flex-wrap: wrap;
          gap: 15px;
        }

        .stat-item {
          display: flex;
          align-items: center;
          gap: 8px;
          font-size: 0.9rem;
          color: #495057;
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
      `}</style>

      <div className="segment-header">
        <h2 className="segment-title">Segment-by-Segment Comparison</h2>
        <button 
          className="ai-toggle"
          onClick={() => setShowAIPanel(!showAIPanel)}
        >
          {showAIPanel ? 'Hide AI Analysis' : 'Show AI Analysis'}
        </button>
      </div>

      {segments.length === 0 ? (
        <div className="empty-state">
          <div style={{ fontSize: '2rem', marginBottom: '20px', fontWeight: 'bold' }}>No Changes</div>
          <div>No differences detected between documents</div>
        </div>
      ) : (
        <>
          <div className="stats-bar">
            <div className="stat-item">
              <span>Total Segments:</span>
              <strong>{segments.length}</strong>
            </div>
            <div className="stat-item">
              <span>Pages:</span>
              <strong>{pageSegments.length}</strong>
            </div>
            <div className="stat-item">
              <span>Added:</span>
              <strong style={{ color: '#059669' }}>
                {segments.filter(s => s.type === 'added').length}
              </strong>
            </div>
            <div className="stat-item">
              <span>Removed:</span>
              <strong style={{ color: '#dc2626' }}>
                {segments.filter(s => s.type === 'removed').length}
              </strong>
            </div>
            <div className="stat-item">
              <span>Modified:</span>
              <strong style={{ color: '#d97706' }}>
                {segments.filter(s => s.type === 'modified').length}
              </strong>
            </div>
          </div>

          <div className="segment-container">
            <div className="segment-content">
              <div className="page-navigator">
                <span style={{ fontWeight: 600, color: '#495057' }}>Pages:</span>
                <button
                  className={`page-tab ${selectedPage === null ? 'active' : ''}`}
                  onClick={() => setSelectedPage(null)}
                >
                  All Pages
                </button>
                {pageSegments.map(page => (
                  <button
                    key={page.pageNumber}
                    className={`page-tab ${selectedPage === page.pageNumber ? 'active' : ''}`}
                    onClick={() => setSelectedPage(page.pageNumber)}
                  >
                    Page {page.pageNumber}
                  </button>
                ))}
              </div>

              <div className="segments-grid">
                <div className="segment-side left">
                  <div className="side-title">
                    Original Document
                  </div>
                  {pageSegments
                    .filter(page => selectedPage === null || page.pageNumber === selectedPage)
                    .map(page => (
                      <div key={`left-${page.pageNumber}`}>
                        {selectedPage === null && (
                          <div style={{ 
                            fontSize: '1.1rem', 
                            fontWeight: 600, 
                            margin: '20px 0 15px 0', 
                            color: '#495057',
                            borderBottom: '2px solid #e9ecef',
                            paddingBottom: '8px'
                          }}>
                            Page {page.pageNumber}
                          </div>
                        )}
                        {page.leftSegments.map((segment, index) => (
                          <motion.div
                            key={`left-${page.pageNumber}-${index}`}
                            className="segment-item"
                            style={{
                              backgroundColor: getChangeColor(segment.type),
                              borderLeftColor: getSeverityColor(segment.severity),
                              color: getTextColor(segment.type)
                            }}
                            whileHover={{ scale: 1.01 }}
                          >
                            <div className="segment-meta">
                              <span>{getChangeIcon(segment.type)}</span>
                              <span style={{ textTransform: 'uppercase', fontSize: '0.8rem', fontWeight: 600 }}>
                                {segment.type}
                              </span>
                              <div 
                                className="severity-badge"
                                style={{ backgroundColor: getSeverityColor(segment.severity) }}
                              >
                                {segment.severity}
                              </div>
                              {segment.pageA && (
                                <div className="page-info">
                                  Page {segment.pageA}
                                </div>
                              )}
                            </div>
                            <div className="segment-text">
                              {segment.text}
                            </div>
                          </motion.div>
                        ))}
                      </div>
                    ))}
                </div>

                <div className="segment-side right">
                  <div className="side-title">
                    Updated Document
                  </div>
                  {pageSegments
                    .filter(page => selectedPage === null || page.pageNumber === selectedPage)
                    .map(page => (
                      <div key={`right-${page.pageNumber}`}>
                        {selectedPage === null && (
                          <div style={{ 
                            fontSize: '1.1rem', 
                            fontWeight: 600, 
                            margin: '20px 0 15px 0', 
                            color: '#495057',
                            borderBottom: '2px solid #e9ecef',
                            paddingBottom: '8px'
                          }}>
                            Page {page.pageNumber}
                          </div>
                        )}
                        {page.rightSegments.map((segment, index) => (
                          <motion.div
                            key={`right-${page.pageNumber}-${index}`}
                            className="segment-item"
                            style={{
                              backgroundColor: getChangeColor(segment.type),
                              borderLeftColor: getSeverityColor(segment.severity),
                              color: getTextColor(segment.type)
                            }}
                            whileHover={{ scale: 1.01 }}
                          >
                            <div className="segment-meta">
                              <span>{getChangeIcon(segment.type)}</span>
                              <span style={{ textTransform: 'uppercase', fontSize: '0.8rem', fontWeight: 600 }}>
                                {segment.type}
                              </span>
                              <div 
                                className="severity-badge"
                                style={{ backgroundColor: getSeverityColor(segment.severity) }}
                              >
                                {segment.severity}
                              </div>
                              {segment.pageB && (
                                <div className="page-info">
                                  Page {segment.pageB}
                                </div>
                              )}
                            </div>
                            <div className="segment-text">
                              {segment.text}
                            </div>
                          </motion.div>
                        ))}
                      </div>
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
                    <h3 style={{ margin: 0 }}>AI Analysis</h3>
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
                            Summary
                          </div>
                          <div className="insight-content">
                            {aiInsights.summary}
                          </div>
                        </div>

                        <div className="insight-section">
                          <div className="insight-title">
                            Key Changes
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
                            Recommendations
                          </div>
                          <ul className="insight-list">
                            {aiInsights.recommendations.map((rec, index) => (
                              <li key={index}>
                                <span>•</span>
                                <span>{rec}</span>
                              </li>
                            ))}
                          </ul>
                        </div>

                        <div className="insight-section">
                          <div className="insight-title">
                            Impact Assessment
                          </div>
                          <div className="insight-content">
                            {aiInsights.impact}
                          </div>
                        </div>
                      </>
                    ) : (
                      <div className="empty-state">
                        <div style={{ fontSize: '1.2rem', marginBottom: '15px', fontWeight: 600 }}>AI Analysis</div>
                        <div>AI insights will appear here after document comparison</div>
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

export default SegmentBySideView;