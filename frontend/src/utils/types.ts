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
