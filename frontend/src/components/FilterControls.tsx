import React from 'react';

interface Props {
  filter: string;
  setFilter: (value: string) => void;
}

const FilterControls: React.FC<Props> = ({ filter, setFilter }) => {
  return (
    <div style={{ margin: '10px 0' }}>
      <label>Show: </label>
      {['All', 'Minor', 'Moderate', 'Major'].map(f => (
        <button
          key={f}
          onClick={() => setFilter(f)}
          style={{ fontWeight: filter === f ? 'bold' : 'normal', margin: '0 5px' }}
        >
          {f}
        </button>
      ))}
    </div>
  );
};

export default FilterControls;
