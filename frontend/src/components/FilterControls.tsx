import React from 'react';

interface Props {
  filter: string;
  setFilter: (value: string) => void;
}

const FilterControls: React.FC<Props> = ({ filter, setFilter }) => {
  return (
    <div className="filter-controls">
      <label>Show: </label>
      {['All', 'Minor', 'Moderate', 'Major'].map(f => (
        <button
          key={f}
          onClick={() => setFilter(f)}
          className={`filter-button ${filter === f ? 'active' : 'inactive'}`}
        >
          {f}
        </button>
      ))}
    </div>
  );
};

export default FilterControls;
