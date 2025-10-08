import React, { useState, useEffect } from 'react';
import { Document, Page, pdfjs } from 'react-pdf';
import { motion, AnimatePresence } from 'framer-motion';
import './PdfSectionViewer.css';

// Set up PDF.js worker
pdfjs.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.min.js`;

interface Section {
  title: string;
  pageNumber: number;
  lineNumber: number;
  sectionType: string;
  content: string;
}

interface PdfPageData {
  pageNumber: number;
  text: string;
  sections: Section[];
  textBlocks: TextBlock[];
}

interface TextBlock {
  text: string;
  lineNumber: number;
  x: number;
  y: number;
  width: number;
  height: number;
}

interface PdfDocumentData {
  fileName: string;
  fullText: string;
  pages: PdfPageData[];
  sections: Section[];
}

interface DiffHighlight {
  type: 'added' | 'removed' | 'modified' | 'unchanged';
  pageNumber: number;
  x: number;
  y: number;
  width: number;
  height: number;
  text: string;
}

interface PdfSectionViewerProps {
  documentA: File | null;
  documentB: File | null;
  comparisonResult?: any;
  isLoading?: boolean;
}

const PdfSectionViewer: React.FC<PdfSectionViewerProps> = ({
  documentA,
  documentB,
  comparisonResult,
  isLoading
}) => {
  const [currentPage, setCurrentPage] = useState(1);
  const [pdfDataA, setPdfDataA] = useState<PdfDocumentData | null>(null);
  const [pdfDataB, setPdfDataB] = useState<PdfDocumentData | null>(null);
  const [diffHighlights, setDiffHighlights] = useState<DiffHighlight[]>([]);
  const [selectedSection, setSelectedSection] = useState<string | null>(null);
  const [scale, setScale] = useState(1.2);
  const [showDifferences, setShowDifferences] = useState(true);

  useEffect(() => {
    if (documentA && documentB) {
      extractPdfData();
    }
  }, [documentA, documentB]);

  useEffect(() => {
    if (comparisonResult && pdfDataA && pdfDataB) {
      processDiffHighlights();
    }
  }, [comparisonResult, pdfDataA, pdfDataB]);

  const extractPdfData = async () => {
    if (!documentA || !documentB) return;

    try {
      // Call enhanced PDF extraction API
      const formData = new FormData();
      formData.append('documentA', documentA);
      formData.append('documentB', documentB);

      const response = await fetch('/api/extract-sections', {
        method: 'POST',
        body: formData
      });

      if (response.ok) {
        const result = await response.json();
        setPdfDataA(result.documentA);
        setPdfDataB(result.documentB);
      }
    } catch (error) {
      console.error('Error extracting PDF data:', error);
    }
  };

  const processDiffHighlights = () => {
    if (!comparisonResult?.diffSegments) return;

    const highlights: DiffHighlight[] = [];
    
    comparisonResult.diffSegments.forEach((segment: any) => {
      if (segment.pageA || segment.pageB) {
        highlights.push({
          type: segment.type,
          pageNumber: segment.pageA || segment.pageB || 1,
          x: 0, // Would be calculated based on text position
          y: segment.lineNumber * 20 || 0,
          width: 500, // Approximate width
          height: 20,
          text: segment.text
        });
      }
    });

    setDiffHighlights(highlights);
  };

  const getHighlightColor = (type: string) => {
    switch (type) {
      case 'added': return 'rgba(34, 197, 94, 0.3)';
      case 'removed': return 'rgba(239, 68, 68, 0.3)';
      case 'modified': return 'rgba(245, 158, 11, 0.3)';
      default: return 'transparent';
    }
  };

  const getSections = () => {
    const allSections = [
      ...(pdfDataA?.sections || []),
      ...(pdfDataB?.sections || [])
    ];
    
    // Remove duplicates and sort by page number
    const uniqueSections = allSections.filter((section, index, self) =>
      index === self.findIndex(s => s.title === section.title)
    );
    
    return uniqueSections.sort((a, b) => a.pageNumber - b.pageNumber);
  };

  const jumpToSection = (section: Section) => {
    setCurrentPage(section.pageNumber);
    setSelectedSection(section.title);
  };

  const getMaxPages = () => {
    const pagesA = pdfDataA?.pages?.length || 0;
    const pagesB = pdfDataB?.pages?.length || 0;
    return Math.max(pagesA, pagesB);
  };

  if (isLoading) {
    return (
      <div className="pdf-section-viewer">
        <div className="loading-state">
          <div className="loading-spinner"></div>
          <p>Analyzing PDF sections and differences...</p>
        </div>
      </div>
    );
  }

  if (!documentA || !documentB) {
    return (
      <div className="pdf-section-viewer">
        <div className="empty-state">
          <h3>PDF Section Viewer</h3>
          <p>Upload two PDF documents to see section-by-section comparison</p>
        </div>
      </div>
    );
  }

  const maxPages = getMaxPages();
  const sections = getSections();

  return (
    <div className="pdf-section-viewer">
      <div className="viewer-header">
        <h2>PDF Section-by-Section Comparison</h2>
        <div className="viewer-controls">
          <div className="scale-controls">
            <button onClick={() => setScale(Math.max(0.5, scale - 0.1))}>−</button>
            <span>{Math.round(scale * 100)}%</span>
            <button onClick={() => setScale(Math.min(2.0, scale + 0.1))}>+</button>
          </div>
          <button 
            className={`diff-toggle ${showDifferences ? 'active' : ''}`}
            onClick={() => setShowDifferences(!showDifferences)}
          >
            {showDifferences ? 'Hide' : 'Show'} Differences
          </button>
        </div>
      </div>

      <div className="viewer-content">
        <div className="sections-sidebar">
          <h3>Document Sections</h3>
          <div className="sections-list">
            {sections.map((section, index) => (
              <motion.div
                key={index}
                className={`section-item ${selectedSection === section.title ? 'selected' : ''}`}
                onClick={() => jumpToSection(section)}
                whileHover={{ x: 5 }}
              >
                <div className="section-header">
                  <span className="section-title">{section.title}</span>
                  <span className="section-page">Page {section.pageNumber}</span>
                </div>
                <div className="section-type">{section.sectionType}</div>
              </motion.div>
            ))}
          </div>
        </div>

        <div className="pdf-comparison">
          <div className="page-navigation">
            <button 
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
            >
              ← Previous
            </button>
            <span>Page {currentPage} of {maxPages}</span>
            <button 
              onClick={() => setCurrentPage(Math.min(maxPages, currentPage + 1))}
              disabled={currentPage === maxPages}
            >
              Next →
            </button>
          </div>

          <div className="pdf-panels">
            <div className="pdf-panel">
              <h4>Original Document</h4>
              <div className="pdf-container">
                {documentA && (
                  <Document
                    file={documentA}
                    loading={<div className="pdf-loading">Loading PDF...</div>}
                  >
                    <div className="pdf-page-wrapper">
                      <Page
                        pageNumber={currentPage}
                        scale={scale}
                        loading={<div className="page-loading">Loading page...</div>}
                      />
                      {showDifferences && (
                        <div className="diff-overlay">
                          {diffHighlights
                            .filter(h => h.pageNumber === currentPage && (h.type === 'removed' || h.type === 'modified'))
                            .map((highlight, index) => (
                              <div
                                key={index}
                                className="diff-highlight"
                                style={{
                                  position: 'absolute',
                                  left: highlight.x,
                                  top: highlight.y,
                                  width: highlight.width,
                                  height: highlight.height,
                                  backgroundColor: getHighlightColor(highlight.type),
                                  border: `2px solid ${getHighlightColor(highlight.type).replace('0.3', '0.8')}`,
                                  pointerEvents: 'none'
                                }}
                                title={`${highlight.type}: ${highlight.text.substring(0, 100)}...`}
                              />
                            ))}
                        </div>
                      )}
                    </div>
                  </Document>
                )}
              </div>
            </div>

            <div className="pdf-panel">
              <h4>Updated Document</h4>
              <div className="pdf-container">
                {documentB && (
                  <Document
                    file={documentB}
                    loading={<div className="pdf-loading">Loading PDF...</div>}
                  >
                    <div className="pdf-page-wrapper">
                      <Page
                        pageNumber={currentPage}
                        scale={scale}
                        loading={<div className="page-loading">Loading page...</div>}
                      />
                      {showDifferences && (
                        <div className="diff-overlay">
                          {diffHighlights
                            .filter(h => h.pageNumber === currentPage && (h.type === 'added' || h.type === 'modified'))
                            .map((highlight, index) => (
                              <div
                                key={index}
                                className="diff-highlight"
                                style={{
                                  position: 'absolute',
                                  left: highlight.x,
                                  top: highlight.y,
                                  width: highlight.width,
                                  height: highlight.height,
                                  backgroundColor: getHighlightColor(highlight.type),
                                  border: `2px solid ${getHighlightColor(highlight.type).replace('0.3', '0.8')}`,
                                  pointerEvents: 'none'
                                }}
                                title={`${highlight.type}: ${highlight.text.substring(0, 100)}...`}
                              />
                            ))}
                        </div>
                      )}
                    </div>
                  </Document>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PdfSectionViewer;