using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VS_RustAnalyzer.Cargo.Json
{
    /**
     * See https://doc.rust-lang.org/rustc/json.html
     */
    [Serializable]
    public class RustcMessage
    {
        [DataMember(Name = "message")]
        string Message { get; set; }

        [DataMember(Name = "code")]
        DiagnosticCode CodeData;

        /** The severity of the diagnostic.
           Values may be:
           - "error": A fatal error that prevents compilation.
           - "warning": A possible error or concern.
           - "note": Additional information or context about the diagnostic.
           - "help": A suggestion on how to resolve the diagnostic.
           - "failure-note": A note attached to the message for further information.
           - "error: internal compiler error": Indicates a bug within the compiler.
        **/
        [DataMember(Name = "level")]
        string Level { get; set; }

        [DataMember(Name = "spans")]
        List<Span> Spans { get; set; }

        [DataMember(Name = "rendered")]
        string Rendered { get; set; }

        [Serializable]
        public class DiagnosticCode
        {
            [DataMember(Name = "code")]
            string Code { get; set; }

            [DataMember(Name = "explanation")]
            string Explanation { get; set; }
        }

        [Serializable]
        public class Span
        {
            /**The file where the span is located.
               Note that this path may not exist. For example, if the path
               points to the standard library, and the rust src is not
               available in the sysroot, then it may point to a non-existent
               file. Beware that this may also point to the source of an
               external crate.
            */
            [DataMember(Name = "file_name")]
            string FileName { get; set; }
            /** The byte offset where the span starts (0-based, inclusive). */
            [DataMember(Name = "byte_start")]
            int ByteStart { get; set; }
            /** The byte offset where the span ends (0-based, exclusive). */
            [DataMember(Name = "byte_end")]
            int ByteEnd { get; set; }
            /** The first line number of the span (1-based, inclusive). */
            [DataMember(Name = "line_start")]
            int LineStart { get; set; }
            /** The last line number of the span (1-based, inclusive). */
            [DataMember(Name = "line_end")]
            int LineEnd { get; set; }
            /** The first character offset of the line_start (1-based, inclusive). */
            [DataMember(Name = "column_start")]
            int ColumnStart { get; set; }
            /** The last character offset of the line_end (1-based, exclusive). */
            [DataMember(Name = "column_end")]
            int ColumnEnd { get; set; }
            /** Whether or not this is the "primary" span.

               This indicates that this span is the focal point of the
               diagnostic.

               There are rare cases where multiple spans may be marked as
               primary. For example, "immutable borrow occurs here" and
               "mutable borrow ends here" can be two separate primary spans.

               The top (parent) message should always have at least one
               primary span, unless it has zero spans. Child messages may have
               zero or more primary spans.
            */
            [DataMember(Name = "is_primary")]
            bool IsPrimary { get; set; }

            [DataMember(Name = "text")]
            List<TextHighlight> Text { get; set; }

            /**An optional message to display at this span location.
               This is typically null for primary spans.
            */
            [DataMember(Name = "label")]
            string Label { get; set; }

            /**An optional string of a suggested replacement for this span to
               solve the issue. Tools may try to replace the contents of the
               span with this text.
            */
            [DataMember(Name ="suggested_replacement")]
            string SuggestedReplacement { get; set; }
            /**An optional string that indicates the confidence of the
               "suggested_replacement". Tools may use this value to determine
               whether or not suggestions should be automatically applied.

               Possible values may be:
               - "MachineApplicable": The suggestion is definitely what the
                 user intended. This suggestion should be automatically
                 applied.
               - "MaybeIncorrect": The suggestion may be what the user
                 intended, but it is uncertain. The suggestion should result
                 in valid Rust code if it is applied.
               - "HasPlaceholders": The suggestion contains placeholders like
                 `(...)`. The suggestion cannot be applied automatically
                 because it will not result in valid Rust code. The user will
                 need to fill in the placeholders.
               - "Unspecified": The applicability of the suggestion is unknown.
            */
            [DataMember(Name = "suggestion_applicability")]
            string SuggestedApplicability { get; set; }

            [Serializable]
            public class TextHighlight
            {
                /** The entire line of the original source code. */
                [DataMember(Name = "text")]
                string NearbyText { get; set; }
                /**The first character offset of the line of
                   where the span covers this line (1-based, inclusive). */
                [DataMember(Name = "highlight_start")]
                int HighlightStart { get; set; }
                /**The last character offset of the line of
                   where the span covers this line (1-based, exclusive). */
                [DataMember(Name = "highlight_end")]
                int HighlightEnd { get; set; }
            }

            [Serializable]
            public class Expansion
            {
                [DataMember(Name = "span")]
                Span Span { get; set; }
                /**Name of the macro, such as "foo!" or "#[derive(Eq)]". */
                [DataMember(Name ="macro_decl_name")]
                string MacroDeclarationName { get; set; }

                /* Optional span where the relevant part of the macro is
                  defined. */
                [DataMember(Name ="def_site_span")]
                Span DefinitionSiteSpan { get; set; }
            }
        }
    }

    /**
     */
    [Serializable]
    public class RustcMessageWithChildren : RustcMessage
    {
        [DataMember(Name = "children")]
        List<RustcMessage> Children { get; set; }
    }
}
