import React from 'react';

interface LogoProps {
  size?: 'small' | 'medium' | 'large';
  className?: string;
}

const Logo: React.FC<LogoProps> = ({ size = 'medium', className = '' }) => {
  const dimensions = {
    small: { width: 180, height: 32 },
    medium: { width: 220, height: 38 },
    large: { width: 260, height: 45 }
  };

  const currentSize = dimensions[size];

  return (
    <div className={`flex items-center ${className}`}>
      <svg 
        width={currentSize.width} 
        height={currentSize.height} 
        viewBox="0 0 320 55" 
        xmlns="http://www.w3.org/2000/svg"
        className="transition-all duration-300 hover:scale-[1.02] filter"
        style={{filter: 'drop-shadow(0 4px 12px rgba(0,0,0,0.15))'}}
      >
        <defs>
          <linearGradient id={`brandGradient-${size}`} x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" style={{stopColor:'#3b82f6', stopOpacity:1}} />
            <stop offset="50%" style={{stopColor:'#8b5cf6', stopOpacity:1}} />
            <stop offset="100%" style={{stopColor:'#06b6d4', stopOpacity:1}} />
          </linearGradient>
          <linearGradient id={`accentGradient-${size}`} x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" style={{stopColor:'#10b981', stopOpacity:1}} />
            <stop offset="100%" style={{stopColor:'#3b82f6', stopOpacity:1}} />
          </linearGradient>
          <linearGradient id={`docGradient-${size}`} x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" style={{stopColor:'#f8fafc', stopOpacity:1}} />
            <stop offset="100%" style={{stopColor:'#e2e8f0', stopOpacity:1}} />
          </linearGradient>
          <filter id={`innerShadow-${size}`} x="-50%" y="-50%" width="200%" height="200%">
            <feOffset dx="0" dy="1"/>
            <feGaussianBlur stdDeviation="1" result="offset-blur"/>
            <feFlood floodColor="#000000" floodOpacity="0.2"/>
            <feComposite in2="offset-blur" operator="in"/>
          </filter>
          <filter id={`glow-${size}`} x="-50%" y="-50%" width="200%" height="200%">
            <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
            <feMerge>
              <feMergeNode in="coloredBlur"/>
              <feMergeNode in="SourceGraphic"/>
            </feMerge>
          </filter>
        </defs>
        
        {/* Logo mark container */}
        <g>
          {/* Subtle background circle */}
          <circle cx="27" cy="27" r="24" fill="white" opacity="0.1" />
          
          {/* Modern geometric document representation */}
          <g transform="translate(12, 12)">
            {/* Primary document */}
            <rect x="4" y="6" width="16" height="20" rx="2.5" 
                  fill="white" 
                  stroke="white" 
                  strokeWidth="1.5" 
                  transform="rotate(-8 12 16)" 
                  opacity="0.9"/>
            
            {/* Document content lines */}
            <g transform="rotate(-8 12 16)" opacity="0.5">
              <rect x="6.5" y="9" width="11" height="1" rx="0.5" fill="#e5e7eb" opacity="0.8"/>
              <rect x="6.5" y="11.5" width="9" height="1" rx="0.5" fill="#e5e7eb" opacity="0.7"/>
              <rect x="6.5" y="14" width="7" height="1" rx="0.5" fill="#e5e7eb" opacity="0.6"/>
              <rect x="6.5" y="16.5" width="10" height="1" rx="0.5" fill="#e5e7eb" opacity="0.6"/>
              <rect x="6.5" y="19" width="8" height="1" rx="0.5" fill="#e5e7eb" opacity="0.5"/>
            </g>
            
            {/* Secondary document */}
            <rect x="10" y="4" width="16" height="20" rx="2.5" 
                  fill="white" 
                  stroke="white" 
                  strokeWidth="1.5" 
                  transform="rotate(12 18 14)" 
                  opacity="0.8"/>
            
            {/* Secondary document content */}
            <g transform="rotate(12 18 14)" opacity="0.4">
              <rect x="12.5" y="7" width="11" height="1" rx="0.5" fill="#e5e7eb" opacity="0.8"/>
              <rect x="12.5" y="9.5" width="9" height="1" rx="0.5" fill="#e5e7eb" opacity="0.7"/>
              <rect x="12.5" y="12" width="7" height="1" rx="0.5" fill="#e5e7eb" opacity="0.6"/>
              <rect x="12.5" y="14.5" width="10" height="1" rx="0.5" fill="#e5e7eb" opacity="0.6"/>
            </g>
            
            {/* Analysis/comparison symbol */}
            <circle cx="35" cy="27" r="7" 
                    fill="none" 
                    stroke="white" 
                    strokeWidth="2.5" 
                    opacity="0.9" 
                    filter={`url(#glow-${size})`}/>
            <circle cx="35" cy="27" r="3" fill="white" opacity="0.5"/>
            
            {/* Analysis arrows */}
            <path d="M 40 22 L 44 18 M 42 20 L 46 16" 
                  stroke="white" 
                  strokeWidth="2" 
                  strokeLinecap="round" 
                  opacity="0.8"/>
          </g>
        </g>
        
        {/* Brand name with premium typography */}
        <g>
          <text x="75" y="25" 
                fontFamily="system-ui, -apple-system, 'Segoe UI', sans-serif" 
                fontSize="28" 
                fontWeight="800" 
                fill="white" 
                letterSpacing="-0.5px">
            Docu
          </text>
          <text x="75" y="42" 
                fontFamily="system-ui, -apple-system, 'Segoe UI', sans-serif" 
                fontSize="28" 
                fontWeight="300" 
                fill="white" 
                letterSpacing="0.5px">
            Vision
          </text>
          
          {/* Refined accent line */}
          <rect x="75" y="47" width="140" height="1.5" fill="white" opacity="0.6" rx="0.75"/>
          
          {/* Modern accent dots */}
          <circle cx="220" cy="47.5" r="2.5" fill="white" opacity="0.8"/>
          <circle cx="228" cy="47.5" r="1.8" fill="white" opacity="0.7"/>
          <circle cx="235" cy="47.5" r="1.2" fill="white" opacity="0.6"/>
        </g>
        
        {/* Enhanced tagline */}
        <text x="75" y="53" 
              fontFamily="system-ui, -apple-system, 'Segoe UI', sans-serif" 
              fontSize="10" 
              fontWeight="700" 
              fill="white" 
              letterSpacing="1.5px" 
              opacity="0.9">
          INTELLIGENT DOCUMENT ANALYSIS
        </text>
      </svg>
    </div>
  );
};

export default Logo;