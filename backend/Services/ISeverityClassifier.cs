using DocumentComparer.Models;

namespace DocumentComparer.Services;

public interface ISeverityClassifier
{
    List<DiffSegment> Classify(List<DiffSegment> diffSegments);
}
