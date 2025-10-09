using System.Text.RegularExpressions;

namespace DocumentComparer.Utils;

public static class TextNormalizer
{
    // Use arrays instead of dictionary to avoid any key conflicts
    private static readonly (string From, string To)[] LigatureReplacements = new[]
    {
        // Standard Unicode ligatures
        ("\uFB01", "fi"),  // ﬁ ligature
        ("\uFB02", "fl"),  // ﬂ ligature
        ("\uFB03", "ffi"), // ﬃ ligature
        ("\uFB04", "ffl"), // ﬄ ligature
        ("\uFB00", "ff"),  // ﬀ ligature
        ("\uFB05", "ft"),  // ﬅ ligature
        ("\uFB06", "st"),  // ﬆ ligature
        
        // Common replacement character issues
        ("\uFFFD", ""),    // Unicode replacement character
        
        // OCR/extraction specific issues
        ("Ɵ", "ti"),       // Sometimes ti appears as this character
        ("ɵ", "ti"),       // Alternative ti replacement
        ("ʄ", "ft"),       // Sometimes ft appears as this character
    };

    private static readonly Dictionary<string, string> CommonWordFixes = new()
    {
        // Most common "ti" missing patterns (direct word matches)
        { "specifica on", "specification" },
        { "Specifica on", "Specification" },
        { "SPECIFICA ON", "SPECIFICATION" },
        { "specificaon", "specification" },
        { "Specificaon", "Specification" },
        { "SPECIFICAON", "SPECIFICATION" },
        
        { "authen ca on", "authentication" },
        { "Authen ca on", "Authentication" },
        { "AUTHEN CA ON", "AUTHENTICATION" },
        { "authencaon", "authentication" },
        { "Authencaon", "Authentication" },
        { "AUTHENCAON", "AUTHENTICATION" },
        
        { "authoriza on", "authorization" },
        { "Authoriza on", "Authorization" },
        { "AUTHORIZA ON", "AUTHORIZATION" },
        { "authorizaon", "authorization" },
        { "Authorizaon", "Authorization" },
        { "AUTHORIZAON", "AUTHORIZATION" },
        
        { "introduc on", "introduction" },
        { "Introduc on", "Introduction" },
        { "INTRODUC ON", "INTRODUCTION" },
        { "introducon", "introduction" },
        { "Introducon", "Introduction" },
        { "INTRODUCON", "INTRODUCTION" },
        
        { "informa on", "information" },
        { "Informa on", "Information" },
        { "INFORMA ON", "INFORMATION" },
        { "informaon", "information" },
        { "Informaon", "Information" },
        { "INFORMAON", "INFORMATION" },
        
        { "applica on", "application" },
        { "Applica on", "Application" },
        { "APPLICA ON", "APPLICATION" },
        { "applicaon", "application" },
        { "Applicaon", "Application" },
        { "APPLICAON", "APPLICATION" },
        
        { "configura on", "configuration" },
        { "Configura on", "Configuration" },
        { "CONFIGURA ON", "CONFIGURATION" },
        { "configuraon", "configuration" },
        { "Configuraon", "Configuration" },
        { "CONFIGURAON", "CONFIGURATION" },
        
        { "administra on", "administration" },
        { "Administra on", "Administration" },
        { "ADMINISTRA ON", "ADMINISTRATION" },
        { "administraon", "administration" },
        { "Administraon", "Administration" },
        { "ADMINISTRAON", "ADMINISTRATION" },
        
        { "registra on", "registration" },
        { "Registra on", "Registration" },
        { "REGISTRA ON", "REGISTRATION" },
        { "registraon", "registration" },
        { "Registraon", "Registration" },
        { "REGISTRAON", "REGISTRATION" },
        
        { "verifica on", "verification" },
        { "Verifica on", "Verification" },
        { "VERIFICA ON", "VERIFICATION" },
        { "verificaon", "verification" },
        { "Verificaon", "Verification" },
        { "VERIFICAON", "VERIFICATION" },
        
        { "cer fica on", "certification" },
        { "Cer fica on", "Certification" },
        { "CER FICA ON", "CERTIFICATION" },
        { "cerificaon", "certification" },
        { "Cerificaon", "Certification" },
        { "CERIFICAON", "CERTIFICATION" },
        
        { "no fica on", "notification" },
        { "No fica on", "Notification" },
        { "NO FICA ON", "NOTIFICATION" },
        { "noificaon", "notification" },
        { "Noificaon", "Notification" },
        { "NOIFICAON", "NOTIFICATION" },
        
        { "modifica on", "modification" },
        { "Modifica on", "Modification" },
        { "MODIFICA ON", "MODIFICATION" },
        { "modificaon", "modification" },
        { "Modificaon", "Modification" },
        { "MODIFICAON", "MODIFICATION" },
        
        { "classifica on", "classification" },
        { "Classifica on", "Classification" },
        { "CLASSIFICA ON", "CLASSIFICATION" },
        { "classificaon", "classification" },
        { "Classificaon", "Classification" },
        { "CLASSIFICAON", "CLASSIFICATION" },
        
        { "iden fica on", "identification" },
        { "Iden fica on", "Identification" },
        { "IDEN FICA ON", "IDENTIFICATION" },
        { "idenificaon", "identification" },
        { "Idenificaon", "Identification" },
        { "IDENIFICAON", "IDENTIFICATION" },
        
        { "organiza on", "organization" },
        { "Organiza on", "Organization" },
        { "ORGANIZA ON", "ORGANIZATION" },
        { "organizaon", "organization" },
        { "Organizaon", "Organization" },
        { "ORGANIZAON", "ORGANIZATION" },
        
        { "evalua on", "evaluation" },
        { "Evalua on", "Evaluation" },
        { "EVALUA ON", "EVALUATION" },
        { "evaluaon", "evaluation" },
        { "Evaluaon", "Evaluation" },
        { "EVALUAON", "EVALUATION" },
        
        { "implementa on", "implementation" },
        { "Implementa on", "Implementation" },
        { "IMPLEMENTA ON", "IMPLEMENTATION" },
        { "implementaon", "implementation" },
        { "Implementaon", "Implementation" },
        { "IMPLEMENTAON", "IMPLEMENTATION" },
        
        { "documenta on", "documentation" },
        { "Documenta on", "Documentation" },
        { "DOCUMENTA ON", "DOCUMENTATION" },
        { "documentaon", "documentation" },
        { "Documentaon", "Documentation" },
        { "DOCUMENTAON", "DOCUMENTATION" },
        
        { "op miza on", "optimization" },
        { "Op miza on", "Optimization" },
        { "OP MIZA ON", "OPTIMIZATION" },
        { "opmizaon", "optimization" },
        { "Opmizaon", "Optimization" },
        { "OPMIZAON", "OPTIMIZATION" },
        
        { "categoriza on", "categorization" },
        { "Categoriza on", "Categorization" },
        { "CATEGORIZA ON", "CATEGORIZATION" },
        { "categorizaon", "categorization" },
        { "Categorizaon", "Categorization" },
        { "CATEGORIZAON", "CATEGORIZATION" },
        
        // Function/operation words
        { "func onal", "functional" },
        { "tradi onal", "traditional" },
        { "op onal", "optional" },
        { "addi onal", "additional" },
        { "profess onal", "professional" },
        { "condi onal", "conditional" },
        { "opera onal", "operational" },
        { "educa onal", "educational" },
        { "interna onal", "international" },
        
        // Reporting/action words
        { "repor ng", "reporting" },
        { "suppor ng", "supporting" },
        { "impor ng", "importing" },
        { "expor ng", "exporting" },
        { "genera on", "generation" }
    };

    /// <summary>
    /// Normalizes ligatures and common PDF extraction issues in text
    /// </summary>
    /// <param name="input">The text to normalize</param>
    /// <returns>Normalized text with ligatures resolved</returns>
    public static string NormalizeLigatures(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var text = input;

        // Step 1: Replace direct ligature mappings
        foreach (var (from, to) in LigatureReplacements)
        {
            text = text.Replace(from, to);
        }

        // Step 2: Fix common word patterns
        foreach (var kv in CommonWordFixes)
        {
            text = text.Replace(kv.Key, kv.Value, StringComparison.OrdinalIgnoreCase);
        }

        // Step 3: Advanced pattern matching for missed cases
        text = FixAdvancedPatterns(text);

        return text;
    }

    /// <summary>
    /// Advanced pattern-based fixing for complex ligature issues
    /// </summary>
    private static string FixAdvancedPatterns(string text)
    {
        // PRIORITY: Fix "ti" missing patterns first (most common issue)
        text = FixTiPatterns(text);
        
        // Pattern: word + space + 1-3 chars + space + word (likely broken ligature)
        text = Regex.Replace(text, @"\b(\w+) ([a-z]{1,3}) (\w+)\b", match => {
            var part1 = match.Groups[1].Value;
            var middle = match.Groups[2].Value;
            var part3 = match.Groups[3].Value;
            
            // Reconstruct potential word
            var reconstructed = part1 + middle + part3;
            
            // Check against common English word patterns
            if (IsLikelyValidWord(reconstructed))
            {
                return reconstructed;
            }
            
            // If middle part looks like it could be "ti", try that
            if (middle.Length <= 2)
            {
                var withTi = part1 + "ti" + part3;
                if (IsLikelyValidWord(withTi))
                {
                    return withTi;
                }
            }
            
            return match.Value; // Return original if no match
        }, RegexOptions.IgnoreCase);

        // Pattern: Fix isolated "ca on" -> "cation" patterns
        text = Regex.Replace(text, @"\b(\w+) ca on\b", "$1cation", RegexOptions.IgnoreCase);
        
        // Pattern: Fix isolated "  on" -> "tion" patterns  
        text = Regex.Replace(text, @"\b(\w+) on\b", match => {
            var word = match.Groups[1].Value;
            if (word.EndsWith("ta") || word.EndsWith("ca") || word.EndsWith("sa") || word.EndsWith("na"))
            {
                return word + "tion";
            }
            return match.Value;
        }, RegexOptions.IgnoreCase);

        return text;
    }

    /// <summary>
    /// Specifically fix "ti" missing patterns in words
    /// </summary>
    private static string FixTiPatterns(string text)
    {
        // Pattern for words ending in "ca on" -> "cation" (most common case)
        text = Regex.Replace(text, @"\b(\w+)ca\s+on\b", "$1cation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)caon\b", "$1cation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "za on" -> "zation"
        text = Regex.Replace(text, @"\b(\w+)za\s+on\b", "$1zation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)zaon\b", "$1zation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "sa on" -> "sation"
        text = Regex.Replace(text, @"\b(\w+)sa\s+on\b", "$1sation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)saon\b", "$1sation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "ra on" -> "ration"
        text = Regex.Replace(text, @"\b(\w+)ra\s+on\b", "$1ration", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)raon\b", "$1ration", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "ta on" -> "tation"
        text = Regex.Replace(text, @"\b(\w+)ta\s+on\b", "$1tation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)taon\b", "$1tation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "ma on" -> "mation"
        text = Regex.Replace(text, @"\b(\w+)ma\s+on\b", "$1mation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)maon\b", "$1mation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "na on" -> "nation"
        text = Regex.Replace(text, @"\b(\w+)na\s+on\b", "$1nation", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)naon\b", "$1nation", RegexOptions.IgnoreCase);
        
        // Pattern for words ending in "c on" -> "ction"
        text = Regex.Replace(text, @"\b(\w+)c\s+on\b", "$1ction", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\b(\w+)con\b", "$1ction", RegexOptions.IgnoreCase);
        
        // Specific problem cases from the user's example
        text = Regex.Replace(text, @"\bSpecificaon\b", "Specification", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bIntroducon\b", "Introduction", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bAuthencaon\b", "Authentication", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bAuthorizaon\b", "Authorization", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\binformaon\b", "information", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bReporng\b", "Reporting", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bexisng\b", "existing", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bso\s+delete\b", "soft delete", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bMul-factor\b", "Multi-factor", RegexOptions.IgnoreCase);
        
        // Fix specific compound issues
        text = Regex.Replace(text, @"\bcategorizaonandtagging\b", "categorization and tagging", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\bcategorizaon\s*and\s*tagging\b", "categorization and tagging", RegexOptions.IgnoreCase);
        
        return text;
    }

    /// <summary>
    /// Simple heuristic to check if a reconstructed word looks valid
    /// </summary>
    private static bool IsLikelyValidWord(string word)
    {
        if (word.Length < 6) return false; // Too short to be a meaningful reconstruction
        
        // Check for common valid word endings that suggest this is a real word
        var validEndings = new[] { 
            "tion", "sion", "ation", "ication", "ization", "ational", 
            "tional", "ing", "ment", "ness", "able", "ible", "ful",
            "ive", "ory", "ary", "ery", "ual", "ial", "ous", "ious"
        };
        
        return validEndings.Any(ending => word.EndsWith(ending, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normalize text specifically for document section titles
    /// </summary>
    public static string NormalizeSectionTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return title;

        var normalized = NormalizeLigatures(title);
        
        // Additional title-specific fixes
        normalized = normalized.Replace("SPECIFICA ION", "SPECIFICATION", StringComparison.OrdinalIgnoreCase);
        normalized = normalized.Replace("Specifica ion", "Specification", StringComparison.OrdinalIgnoreCase);
        normalized = normalized.Replace("specifica ion", "specification", StringComparison.OrdinalIgnoreCase);
        
        return normalized.Trim();
    }
}