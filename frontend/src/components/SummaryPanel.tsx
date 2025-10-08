import React from 'react';

interface Props {
  summary: string;
  similarityScore: number;
}

const SummaryPanel: React.FC<Props> = ({ summary, similarityScore }) => (
  <div style={{ border: '1px solid #ddd', padding: 10, margin: 10 }}>
    <h3>Summary</h3>
    <p>{summary}</p>
    <p>Similarity: {(similarityScore * 100).toFixed(2)}%</p>
  </div>
);

export default SummaryPanel;
