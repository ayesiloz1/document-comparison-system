import React from 'react';
import { DiffSegment } from '../utils/types';
import { severityColors } from '../utils/severityColors';

interface Props {
  diffSegments: DiffSegment[];
  filter?: 'All' | 'Minor' | 'Moderate' | 'Major';
}

const DiffViewer: React.FC<Props> = ({ diffSegments, filter = 'All' }) => {
  // Filter segments by severity
  const filtered = diffSegments.filter(seg => filter === 'All' || seg.severity === filter);

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'Inserted': return '➕';
      case 'Deleted': return '➖';
      case 'Modified': return '🔄';
      default: return '📝';
    }
  };



  return (
    <div className="diff-viewer">
      <h4>Document Differences ({filtered.length} items)</h4>
      <div className="diff-viewer-container">
        {filtered.length === 0 ? (
          <div className="diff-empty">
            No differences found for the selected filter.
          </div>
        ) : (
          filtered.map((segment, index) => (
            <div
              key={index}
              className={`diff-segment ${segment.type.toLowerCase()}`}
            >
              <span className="diff-icon">
                {getTypeIcon(segment.type)}
              </span>
              <div className="diff-content">
                <div className="diff-meta">
                  <span className={`diff-type ${segment.type.toLowerCase()}`}>
                    {segment.type.toUpperCase()}
                  </span>
                  <span>•</span>
                  <span 
                    className="diff-severity-badge"
                    style={{ backgroundColor: severityColors[segment.severity] }}
                  >
                    {segment.severity}
                  </span>
                  {segment.pageNumberA && (
                    <>
                      <span>•</span>
                      <span>Page {segment.pageNumberA}</span>
                    </>
                  )}
                </div>
                <div className="diff-text">
                  {segment.text.length > 200 
                    ? `${segment.text.substring(0, 200)}...` 
                    : segment.text
                  }
                </div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default DiffViewer;
