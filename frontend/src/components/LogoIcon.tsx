import React from 'react';

interface LogoIconProps {
  size?: number;
  className?: string;
  animated?: boolean;
}

const LogoIcon: React.FC<LogoIconProps> = ({ size = 24, className = '', animated = false }) => {
  return (
    <div className={`logo-icon ${className} ${animated ? 'animate-pulse' : ''}`}>
      <svg 
        width={size} 
        height={size} 
        viewBox="0 0 32 32" 
        xmlns="http://www.w3.org/2000/svg"
        className="transition-all duration-300"
      >
        <defs>
          <linearGradient id={`iconGradient-${size}`} x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" style={{stopColor:'#2563eb', stopOpacity:1}} />
            <stop offset="100%" style={{stopColor:'#1e40af', stopOpacity:1}} />
          </linearGradient>
        </defs>
        
        <rect width="32" height="32" fill={`url(#iconGradient-${size})`} rx="6"/>
        
        {/* Document icons */}
        <rect x="4" y="6" width="10" height="14" rx="1" fill="white" opacity="0.9"/>
        <rect x="6" y="8" width="6" height="1" fill={`url(#iconGradient-${size})`} opacity="0.7"/>
        <rect x="6" y="10" width="5" height="1" fill={`url(#iconGradient-${size})`} opacity="0.7"/>
        <rect x="6" y="12" width="4" height="1" fill={`url(#iconGradient-${size})`} opacity="0.7"/>
        <rect x="6" y="14" width="6" height="1" fill={`url(#iconGradient-${size})`} opacity="0.7"/>
        
        <rect x="8" y="10" width="10" height="14" rx="1" fill="white" opacity="0.8"/>
        <rect x="10" y="12" width="6" height="1" fill={`url(#iconGradient-${size})`} opacity="0.6"/>
        <rect x="10" y="14" width="5" height="1" fill={`url(#iconGradient-${size})`} opacity="0.6"/>
        <rect x="10" y="16" width="4" height="1" fill={`url(#iconGradient-${size})`} opacity="0.6"/>
        <rect x="10" y="18" width="6" height="1" fill={`url(#iconGradient-${size})`} opacity="0.6"/>
        
        {/* Analysis symbol */}
        <circle cx="22" cy="16" r="4" fill="none" stroke="white" strokeWidth="1.5"/>
        <circle cx="22" cy="16" r="2" fill="white" opacity="0.3"/>
      </svg>
    </div>
  );
};

export default LogoIcon;