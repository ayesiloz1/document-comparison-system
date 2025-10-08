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

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'Inserted': return '#dcfce7';
      case 'Deleted': return '#fee2e2';
      case 'Modified': return '#fef3c7';
      default: return '#f8fafc';
    }
  };

  return (
    <div style={{ margin: '20px 0' }}>
      <h4>Document Differences ({filtered.length} items)</h4>
      <div style={{ maxHeight: '400px', overflowY: 'auto', border: '1px solid #e5e7eb', borderRadius: '8px' }}>
        {filtered.length === 0 ? (
          <div style={{ padding: '20px', textAlign: 'center', color: '#6b7280' }}>
            No differences found for the selected filter.
          </div>
        ) : (
          filtered.map((segment, index) => (
            <div
              key={index}
              style={{
                padding: '12px 16px',
                borderBottom: index < filtered.length - 1 ? '1px solid #f3f4f6' : 'none',
                backgroundColor: getTypeColor(segment.type),
                display: 'flex',
                alignItems: 'flex-start',
                gap: '12px'
              }}
            >
              <span style={{ fontSize: '16px', flexShrink: 0 }}>
                {getTypeIcon(segment.type)}
              </span>
              <div style={{ flex: 1 }}>
                <div style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  gap: '8px', 
                  marginBottom: '4px',
                  fontSize: '12px',
                  color: '#6b7280'
                }}>
                  <span style={{ 
                    fontWeight: 'bold',
                    color: segment.type === 'Inserted' ? '#059669' : 
                           segment.type === 'Deleted' ? '#dc2626' : 
                           segment.type === 'Modified' ? '#d97706' : '#374151'
                  }}>
                    {segment.type.toUpperCase()}
                  </span>
                  <span>•</span>
                  <span style={{
                    backgroundColor: severityColors[segment.severity],
                    padding: '2px 6px',
                    borderRadius: '4px',
                    fontWeight: '500'
                  }}>
                    {segment.severity}
                  </span>
                  {segment.pageNumberA && (
                    <>
                      <span>•</span>
                      <span>Page {segment.pageNumberA}</span>
                    </>
                  )}
                </div>
                <div style={{
                  fontFamily: 'Monaco, Menlo, monospace',
                  fontSize: '13px',
                  lineHeight: '1.4',
                  color: '#374151',
                  whiteSpace: 'pre-wrap',
                  wordBreak: 'break-word'
                }}>
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
