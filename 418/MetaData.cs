using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// CSV 기반 메타데이터 시스템의 베이스 클래스.
///
/// [CSV 포맷 규칙]
///   Row 0 : 컬럼 헤더 (필드명)
///   Row 1 : 설명 행  (읽기 시 건너뜀)
///   Row 2+: 실제 데이터
///
/// [헤더 표기 규칙]
///   일반 필드  : fieldName
///   배열 필드  : fieldName[0], fieldName[1], fieldName[2], ...
///   계층 필드  : parentField.childField
///
/// [파생 클래스 사용 예시]
/// <code>
/// public class DungeonLevelData : MetaData
/// {
///     public int    DungeonId   { get; private set; }
///     public string Name        { get; private set; }
///     public float  Difficulty  { get; private set; }
///     public List&lt;int&gt; RewardIds = new List&lt;int&gt;();
///
///     public DungeonLevelData()
///     {
///         Bind("dungeonId",  (int    v) => DungeonId  = v);
///         Bind("name",       (string v) => Name       = v);
///         Bind("difficulty", (float  v) => Difficulty = v);
///         Bind("rewardId",   RewardIds, int.Parse);
///     }
/// }
///
/// // 읽기
/// var reader = new MetaData.Reader&lt;DungeonLevelData&gt;();
/// reader.Read("DungeonLevel.csv");
/// foreach (var data in reader.All) { ... }
/// </code>
/// </summary>
public abstract class MetaData
{
    // =========================================================
    //  내부 타입
    // =========================================================

    private class Header
    {
        public int    Index { get; set; } = -1;
        public string Name  { get; set; } = string.Empty;
        public Header Child { get; set; } = null;
    }

    private class Cell
    {
        public Header Header { get; set; }
        public string Value  { get; set; } = string.Empty;
    }

    // =========================================================
    //  Reader<TMeta>
    // =========================================================

    public class Reader<TMeta> where TMeta : MetaData, new()
    {
        private readonly List<TMeta> _metaDatas = new List<TMeta>();

        /// <summary>파싱된 메타데이터 전체 목록</summary>
        public IReadOnlyList<TMeta> All => _metaDatas;

        /// <summary>CSV 파일을 읽어 TMeta 목록을 구성합니다.</summary>
        public bool Read(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            var rows = ParseCsv(filePath);
            if (rows.Count == 0)
                return false;

            // Row 0: 헤더
            var headerRow = rows[0];
            var headers = new List<Header>(headerRow.Count);
            foreach (var h in headerRow)
                headers.Add(ReadHeader(h));

            // Row 1: 건너뜀 (설명 행)
            // Row 2+: 데이터
            for (int rowNum = 2; rowNum < rows.Count; rowNum++)
            {
                var row = rows[rowNum];

                // 빈 줄 건너뜀
                if (row.Count == 0 || (row.Count == 1 && string.IsNullOrWhiteSpace(row[0])))
                    continue;

                var cells = new List<Cell>(headers.Count);
                for (int colNum = 0; colNum < headers.Count; colNum++)
                {
                    cells.Add(new Cell
                    {
                        Header = headers[colNum],
                        Value  = colNum < row.Count ? row[colNum] : string.Empty
                    });
                }

                var meta = new TMeta();
                meta.Init(cells);
                _metaDatas.Add(meta);
            }

            return true;
        }

        // ---------------------------------------------------------
        //  CSV 파싱
        // ---------------------------------------------------------

        private static List<List<string>> ParseCsv(string filePath)
        {
            var result = new List<List<string>>();
            using var sr = new StreamReader(filePath, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
                result.Add(ParseCsvLine(line));
            return result;
        }

        private static List<string> ParseCsvLine(string line)
        {
            var cells   = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    // escaped quote: ""
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    cells.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            cells.Add(current.ToString());
            return cells;
        }

        // ---------------------------------------------------------
        //  헤더 파싱  (재귀)
        // ---------------------------------------------------------

        private static Header ReadHeader(string cellValue)
        {
            var root = new Header();

            // 계층 구분자 '.' 처리
            int dotPos = cellValue.IndexOf('.');
            string column = dotPos >= 0 ? cellValue.Substring(0, dotPos) : cellValue;

            // 배열 인덱스 '[n]' 처리
            int braceStart = column.IndexOf('[');
            int braceEnd   = column.IndexOf(']');

            bool hasBraceStart = braceStart >= 0;
            bool hasBraceEnd   = braceEnd   >= 0;

            if (hasBraceStart != hasBraceEnd)
                throw new FormatException($"컬럼명 오류: '{cellValue}', 대괄호 짝이 맞지 않습니다.");

            if (hasBraceStart)
                root.Index = int.Parse(column.Substring(braceStart + 1, braceEnd - braceStart - 1));

            // 순수 이름 부분만 추출
            int nameEnd = int.MaxValue;
            if (dotPos    >= 0) nameEnd = Math.Min(nameEnd, dotPos);
            if (braceStart >= 0) nameEnd = Math.Min(nameEnd, braceStart);
            root.Name = nameEnd == int.MaxValue ? column : column.Substring(0, nameEnd);

            // 자식 헤더 재귀 처리
            if (dotPos >= 0)
                root.Child = ReadHeader(cellValue.Substring(dotPos + 1));

            return root;
        }
    }

    // =========================================================
    //  Init
    // =========================================================

    /// <summary>파싱된 Cell 목록을 바탕으로 멤버를 초기화합니다.</summary>
    public void Init(IList<Cell> row)
    {
        foreach (var cell in row)
        {
            string key = cell.Header?.Name ?? string.Empty;
            if (string.IsNullOrEmpty(key))              continue;
            if (string.IsNullOrEmpty(cell.Value))       continue;
            if (!_bindFunctions.TryGetValue(key, out var func)) continue;

            func(cell);
        }
    }

    // =========================================================
    //  Bind 함수 등록부
    // =========================================================

    private readonly Dictionary<string, Action<Cell>> _bindFunctions =
        new Dictionary<string, Action<Cell>>();

    // ---------------------------------------------------------
    //  스칼라 타입
    // ---------------------------------------------------------

    protected void Bind(string name, Action<bool> setter)
    {
        _bindFunctions[name] = cell =>
        {
            string lower = cell.Value.ToLowerInvariant();
            setter(lower != "false" && lower != "0");
        };
    }

    protected void Bind(string name, Action<short>  setter)
        => _bindFunctions[name] = cell => setter(short.Parse(cell.Value));

    protected void Bind(string name, Action<ushort> setter)
        => _bindFunctions[name] = cell => setter(ushort.Parse(cell.Value));

    protected void Bind(string name, Action<int>    setter)
        => _bindFunctions[name] = cell => setter(int.Parse(cell.Value));

    protected void Bind(string name, Action<uint>   setter)
        => _bindFunctions[name] = cell => setter(uint.Parse(cell.Value));

    protected void Bind(string name, Action<long>   setter)
        => _bindFunctions[name] = cell => setter(long.Parse(cell.Value));

    protected void Bind(string name, Action<ulong>  setter)
        => _bindFunctions[name] = cell => setter(ulong.Parse(cell.Value));

    protected void Bind(string name, Action<float>  setter)
        => _bindFunctions[name] = cell => setter(float.Parse(cell.Value));

    protected void Bind(string name, Action<double> setter)
        => _bindFunctions[name] = cell => setter(double.Parse(cell.Value));

    protected void Bind(string name, Action<string> setter)
        => _bindFunctions[name] = cell => setter(cell.Value);

    // ---------------------------------------------------------
    //  배열 타입  (헤더에 [n] 인덱스가 있어야 함)
    //  사용 예: Bind("rewardId", RewardIds, int.Parse);
    // ---------------------------------------------------------

    protected void Bind<T>(string name, List<T> list, Func<string, T> parser)
    {
        _bindFunctions[name] = cell =>
        {
            int idx = cell.Header.Index;
            if (idx < 0)
                throw new InvalidOperationException(
                    $"컬럼 '{name}' 은 배열 컬럼이 아닙니다. 헤더에 [n] 인덱스가 필요합니다.");

            while (list.Count <= idx)
                list.Add(default);

            list[idx] = parser(cell.Value);
        };
    }

    // ---------------------------------------------------------
    //  하위 MetaData (점 표기 계층 구조)
    //  사용 예: Bind("spawn", SpawnInfo);
    // ---------------------------------------------------------

    protected void Bind(string name, MetaData subMeta)
    {
        _bindFunctions[name] = cell =>
        {
            if (cell.Header?.Child == null) return;

            var childCell = new Cell
            {
                Header = cell.Header.Child,
                Value  = cell.Value
            };
            subMeta.Init(new List<Cell> { childCell });
        };
    }

    // ---------------------------------------------------------
    //  커스텀 함수 바인딩
    //  사용 예: BindFunc("flags", raw => ParseFlags(raw));
    // ---------------------------------------------------------

    protected void BindFunc(string name, Action<string> customFunction)
        => _bindFunctions[name] = cell => customFunction(cell.Value);
}
