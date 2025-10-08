import { ComparisonResult } from './types';

export const compareDocuments = async (pdfA: File, pdfB: File): Promise<ComparisonResult> => {
  const formData = new FormData();
  formData.append('pdf1', pdfA);
  formData.append('pdf2', pdfB);

  const res = await fetch('http://localhost:5000/compare', { method: 'POST', body: formData });
  if (!res.ok) throw new Error('Comparison failed');
  const result = await res.json();
  console.log('API Response:', result);
  return result;
};

export const exportReport = async (result: ComparisonResult) => {
  const res = await fetch('http://localhost:5000/export', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(result)
  });
  if (!res.ok) throw new Error('Export failed');

  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'comparison_report.pdf';
  a.click();
  window.URL.revokeObjectURL(url);
};
