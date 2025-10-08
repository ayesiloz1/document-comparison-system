import React from 'react';
import { Document, Page } from 'react-pdf';
import 'react-pdf/dist/esm/Page/AnnotationLayer.css';

interface Props {
  file: File | null;
  scale?: number;
}

const PdfPreview: React.FC<Props> = ({ file, scale = 1.2 }) => {
  if (!file) return null;

  return (
    <div style={{ border: '1px solid #ccc', padding: 10, margin: 10 }}>
      <Document file={file}>
        {[...Array(10).keys()].map(i => (
          <Page key={i} pageNumber={i + 1} scale={scale} />
        ))}
      </Document>
    </div>
  );
};

export default PdfPreview;
