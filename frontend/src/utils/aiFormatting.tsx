// AI Text Formatting Utilities

export const formatAIAnalysis = (text: string): string => {
  if (!text) return '';
  
  // Remove markdown bold formatting (**text**)
  let formatted = text.replace(/\*\*(.*?)\*\*/g, '$1');
  
  // Convert section headers that start with **Section:** to proper sections
  formatted = formatted.replace(/\*\*(.*?):\*\*/g, '$1:');
  
  // Add proper line breaks for sections
  formatted = formatted.replace(/(\w+:)\s*/g, '\n\n$1\n');
  
  // Clean up multiple line breaks
  formatted = formatted.replace(/\n{3,}/g, '\n\n');
  
  // Trim leading/trailing whitespace
  formatted = formatted.trim();
  
  return formatted;
};

export const createStructuredAnalysis = (text: string): JSX.Element => {
  if (!text) return <div></div>;
  
  // Remove markdown formatting
  const cleanText = formatAIAnalysis(text);
  
  // Split into sections based on patterns
  const sections = cleanText.split(/\n\n(?=[A-Z][^:]*:)/);
  
  if (sections.length <= 1) {
    // If no clear sections, just return formatted text
    return (
      <div className="analysis-content">
        {cleanText.split('\n').map((paragraph, index) => (
          paragraph.trim() && (
            <div key={index} className="analysis-section">
              {paragraph.trim()}
            </div>
          )
        ))}
      </div>
    );
  }
  
  // Process sections with titles
  return (
    <div className="analysis-content">
      {sections.map((section, index) => {
        const lines = section.trim().split('\n');
        const titleLine = lines[0];
        const content = lines.slice(1).join(' ').trim();
        
        if (titleLine.includes(':')) {
          return (
            <div key={index} className="analysis-section">
              <span className="section-title">{titleLine}</span>
              <div className="section-content">{content}</div>
            </div>
          );
        } else {
          return (
            <div key={index} className="analysis-section">
              {section.trim()}
            </div>
          );
        }
      })}
    </div>
  );
};