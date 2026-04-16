using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Common.UI.Text
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPInlineGradient : MonoBehaviour
    {
        [Tooltip("Смотреть изменения tmp.text и перепарсивать автоматически")] [SerializeField]
        private bool watchTextChanges = true;

        private TMP_Text _tmp;
        private string _lastTagged = "";

        private struct Span
        {
            public int start;
            public int length;
            public TMP_ColorGradient gradient;
        }

        private readonly List<Span> _spans = new();
        private readonly Dictionary<string, TMP_ColorGradient> _cache = new();

        // поддерживаем <g=Name>...</g> и <gradient="Name">...</gradient>
        private static readonly Regex StartTag =
            new(@"<(?:g|gradient)\s*=\s*(""|')?(?<name>[^""'>]+)\1?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex EndTag =
            new(@"</(?:g|gradient)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
            _tmp.OnPreRenderText += HandlePreRender;
        }

        private void OnDestroy()
        {
            if (_tmp != null) _tmp.OnPreRenderText -= HandlePreRender;
        }

        private void LateUpdate()
        {
            if (!watchTextChanges) return;
            if (!string.Equals(_tmp.text, _lastTagged))
                SetTaggedText(_tmp.text);
        }

        /// <summary>Ручной парсинг строки с тегами. Можно звать при необходимости.</summary>
        public void SetTaggedText(string taggedText)
        {
            _lastTagged = taggedText ?? string.Empty;
            _spans.Clear();

            var sb = new StringBuilder(_lastTagged.Length);
            int i = 0;
            var stack = new Stack<TMP_ColorGradient>();

            while (i < _lastTagged.Length)
            {
                var mStart = StartTag.Match(_lastTagged, i);
                if (mStart.Success && mStart.Index == i)
                {
                    string name = mStart.Groups["name"].Value.Trim();
                    stack.Push(LoadGradient(name));
                    i += mStart.Length;
                    continue;
                }

                var mEnd = EndTag.Match(_lastTagged, i);
                if (mEnd.Success && mEnd.Index == i)
                {
                    if (stack.Count > 0) stack.Pop();
                    i += mEnd.Length;
                    continue;
                }

                int writeIndex = sb.Length;
                sb.Append(_lastTagged[i]);

                if (stack.Count > 0)
                {
                    bool extend = false;
                    if (_spans.Count > 0)
                    {
                        var last = _spans[^1];
                        if (last.start + last.length == writeIndex && last.gradient == stack.Peek())
                        {
                            last.length += 1;
                            _spans[^1] = last;
                            extend = true;
                        }
                    }

                    if (!extend)
                    {
                        _spans.Add(new Span
                        {
                            start = writeIndex,
                            length = 1,
                            gradient = stack.Peek()
                        });
                    }
                }

                i++;
            }

            _tmp.text = sb.ToString();
            _tmp.SetAllDirty();
        }

        private TMP_ColorGradient LoadGradient(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (_cache.TryGetValue(name, out var g) && g != null) return g;

            // Важно: пресет должен быть TMP_ColorGradient в Resources/
            var loaded = Resources.Load<TMP_ColorGradient>(name);
            _cache[name] = loaded;
            if (loaded == null)
                Debug.LogError($"[TMPInlineGradient] Gradient preset '{name}' not found in Resources/");
            return loaded;
        }

        private void HandlePreRender(TMP_TextInfo textInfo)
        {
            if (_spans.Count == 0) return;

            // какие mesh-слои мы изменили (на случай эмодзи/спрайтов и др. материалов)
            var touched = System.Buffers.ArrayPool<int>.Shared.Rent(textInfo.meshInfo.Length);
            int touchedCount = 0;

            foreach (var span in _spans)
            {
                int from = Mathf.Clamp(span.start, 0, textInfo.characterCount);
                int to   = Mathf.Clamp(span.start + span.length, 0, textInfo.characterCount);
                var grad = span.gradient;
                if (grad == null) continue;

                var bl = (Color32)grad.bottomLeft;
                var tl = (Color32)grad.topLeft;
                var tr = (Color32)grad.topRight;
                var br = (Color32)grad.bottomRight;

                for (int ci = from; ci < to; ci++)
                {
                    var ch = textInfo.characterInfo[ci];
                    if (!ch.isVisible) continue;

                    int vi = ch.vertexIndex;
                    int mi = ch.materialReferenceIndex;
                    var colors = textInfo.meshInfo[mi].colors32;

                    colors[vi + 0] = bl; // BL
                    colors[vi + 1] = tl; // TL
                    colors[vi + 2] = tr; // TR
                    colors[vi + 3] = br; // BR

                    // пометим слой как изменённый
                    bool known = false;
                    for (int k = 0; k < touchedCount; k++)
                        if (touched[k] == mi) { known = true; break; }
                    if (!known) touched[touchedCount++] = mi;
                }
            }

            // КЛЮЧЕВОЕ: записать цвета обратно в mesh
            for (int k = 0; k < touchedCount; k++)
            {
                int mi = touched[k];
                var meshInfo = textInfo.meshInfo[mi];
                if (meshInfo.mesh != null)
                    meshInfo.mesh.colors32 = meshInfo.colors32;
            }

            System.Buffers.ArrayPool<int>.Shared.Return(touched);
        }

        // Удобная обёртка для генерации тега
        public static string Wrap(string gradientName, string text)
            => $@"<g=""{gradientName}"">{text}</g>";
    }
}