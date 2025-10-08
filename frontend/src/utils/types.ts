export type ChangeType = 'Unchanged' | 'Inserted' | 'Deleted' | 'Modified';
export type Severity = 'Minor' | 'Moderate' | 'Major';

export interface DiffSegment {
  type: ChangeType;
  text: string;
  severity: Severity;
  pageNumberA?: number;
  pageNumberB?: number;
}

export interface AIInsight {
  summary: string;
  keyChanges: string[];
  recommendations: string[];
  impact: string;
}

export interface ComparisonResult {
  summary: string;
  similarityScore: number;
  diffSegments: DiffSegment[];
  aiInsights?: AIInsight;
}

// New Section-based comparison types
export interface SectionComparisonResult {
  documentAName: string;
  documentBName: string;
  documentASections: DocumentSection[];
  documentBSections: DocumentSection[];
  sectionComparisons: SectionComparison[];
  overallSimilarity: number;
  aiSectionInsights: AISectionInsights;
  comparisonTimestamp: string;
}

export interface DocumentSection {
  sectionId: string;
  title: string;
  pageNumber: number;
  lineNumber: number;
  sectionType: string;
  content: string;
  documentName: string;
  wordCount: number;
  extractedAt: string;
}

export interface SectionComparison {
  comparisonId: string;
  sectionA?: DocumentSection;
  sectionB?: DocumentSection;
  changeType: 'Unchanged' | 'Added' | 'Deleted' | 'Modified';
  similarityScore?: number;
  pageNumberA?: number;
  pageNumberB?: number;
  aiSummary: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
}

export interface AISectionInsights {
  overallSummary: string;
  keyChanges: SectionComparison[];
  changeStatistics: ChangeStatistics;
}

export interface ChangeStatistics {
  addedSections: number;
  deletedSections: number;
  modifiedSections: number;
  unchangedSections: number;
  totalSections: number;
  changePercentage: number;
}
