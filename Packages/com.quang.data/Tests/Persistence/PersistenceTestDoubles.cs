using System.Collections.Generic;
using Quang.Base.Persistence;

namespace Quang.Data.Tests.Persistence
{
    internal sealed class DiagnosticsCollector : ISaveDiagnostics
    {
        public readonly List<SaveDiagnostic> Events = new List<SaveDiagnostic>();
        public void Report(SaveDiagnostic diagnostic) => Events.Add(diagnostic);
    }

    internal sealed class DictionaryLegacySaveSource : Quang.Data.Persistence.ILegacySaveSource
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();

        public void Set(string key, string json) => values[key] = json;

        public bool TryRead(string key, out string json) => values.TryGetValue(key, out json);
    }

    internal static class PersistenceTestDoubles
    {
        public static ISaveCodec Codec() => new Quang.Data.Persistence.PlainJsonSaveCodec();
        public static ISaveProtector Protector() => new Quang.Data.Persistence.Sha256SaveProtector();
    }
}
