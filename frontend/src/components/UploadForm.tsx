import React, { useState } from 'react';

interface Props {
  onCompare: (fileA: File, fileB: File) => void;
  disabled?: boolean;
}

const UploadForm: React.FC<Props> = ({ onCompare, disabled }) => {
  const [fileA, setFileA] = useState<File | null>(null);
  const [fileB, setFileB] = useState<File | null>(null);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (fileA && fileB) onCompare(fileA, fileB);
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Document A:</label>
        <input type="file" accept="application/pdf" onChange={e => setFileA(e.target.files?.[0] ?? null)} />
      </div>
      <div>
        <label>Document B:</label>
        <input type="file" accept="application/pdf" onChange={e => setFileB(e.target.files?.[0] ?? null)} />
      </div>
      <button type="submit" disabled={disabled || !fileA || !fileB}>Compare</button>
    </form>
  );
};

export default UploadForm;
