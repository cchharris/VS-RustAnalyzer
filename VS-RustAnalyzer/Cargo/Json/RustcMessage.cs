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
        #region RustcMessage Properties
        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "code")]
        public DiagnosticCode CodeData;

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
        public string Level { get; set; }

        [DataMember(Name = "spans")]
        public List<Span> Spans { get; set; }

        [DataMember(Name = "rendered")]
        public string Rendered { get; set; }

        #endregion

        #region RustcMessage InnerData

        [Serializable]
        public class DiagnosticCode
        {
            #region DiagnosticCode Properties
            [DataMember(Name = "code")]
            public string Code { get; set; }

            [DataMember(Name = "explanation")]
            public string Explanation { get; set; }
            #endregion
        }

        [Serializable]
        public class Span
        {
            #region Span Properties
            /**The file where the span is located.
               Note that this path may not exist. For example, if the path
               points to the standard library, and the rust src is not
               available in the sysroot, then it may point to a non-existent
               file. Beware that this may also point to the source of an
               external crate.
            */
            [DataMember(Name = "file_name")]
            public string FileName { get; set; }
            /** The byte offset where the span starts (0-based, inclusive). */
            [DataMember(Name = "byte_start")]
            public int ByteStart { get; set; }
            /** The byte offset where the span ends (0-based, exclusive). */
            [DataMember(Name = "byte_end")]
            public int ByteEnd { get; set; }
            /** The first line number of the span (1-based, inclusive). */
            [DataMember(Name = "line_start")]
            public int LineStart { get; set; }
            /** The last line number of the span (1-based, inclusive). */
            [DataMember(Name = "line_end")]
            public int LineEnd { get; set; }
            /** The first character offset of the line_start (1-based, inclusive). */
            [DataMember(Name = "column_start")]
            public int ColumnStart { get; set; }
            /** The last character offset of the line_end (1-based, exclusive). */
            [DataMember(Name = "column_end")]
            public int ColumnEnd { get; set; }
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
            public bool IsPrimary { get; set; }

            [DataMember(Name = "text")]
            public List<TextHighlight> Text { get; set; }

            /**An optional message to display at this span location.
               This is typically null for primary spans.
            */
            [DataMember(Name = "label")]
            public string Label { get; set; }

            /**An optional string of a suggested replacement for this span to
               solve the issue. Tools may try to replace the contents of the
               span with this text.
            */
            [DataMember(Name ="suggested_replacement")]
            public string SuggestedReplacement { get; set; }
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
            public string SuggestedApplicability { get; set; }

            #endregion

            #region Span InnerData

            [Serializable]
            public class TextHighlight
            {
                #region TextHighlight Properties
                /** The entire line of the original source code. */
                [DataMember(Name = "text")]
                public string NearbyText { get; set; }
                /**The first character offset of the line of
                   where the span covers this line (1-based, inclusive). */
                [DataMember(Name = "highlight_start")]
                public int HighlightStart { get; set; }
                /**The last character offset of the line of
                   where the span covers this line (1-based, exclusive). */
                [DataMember(Name = "highlight_end")]
                public int HighlightEnd { get; set; }
                #endregion
            }

            [Serializable]
            public class Expansion
            {
                #region Expansion Properties
                [DataMember(Name = "span")]
                public Span Span { get; set; }
                /**Name of the macro, such as "foo!" or "#[derive(Eq)]". */
                [DataMember(Name ="macro_decl_name")]
                public string MacroDeclarationName { get; set; }

                /* Optional span where the relevant part of the macro is
                  defined. */
                [DataMember(Name ="def_site_span")]
                public Span DefinitionSiteSpan { get; set; }
                #endregion
            }
            #endregion
        }
        #endregion
    }

    /**
     */
    [Serializable]
    public class RustcMessageWithChildren : RustcMessage
    {
        #region RustcMessageWithChildrenProperties
        [DataMember(Name = "children")]
        public List<RustcMessage> Children { get; set; }
        #endregion
    }
}
