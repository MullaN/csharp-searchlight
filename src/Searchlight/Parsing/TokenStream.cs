using System.Collections.Generic;

namespace Searchlight.Parsing
{
    /// <summary>
    /// Represents a parsed token stream pre-AST
    /// </summary>
    public class TokenStream
    {
        /// <summary>
        /// Original text
        /// </summary>
        public string OriginalText { get; set; }
        
        /// <summary>
        /// The list of tokens from the stream
        /// </summary>
        public Queue<Token> TokenQueue { get; set; }
        
        /// <summary>
        /// Set to true if there is an open quote but no close quote
        /// </summary>
        public bool HasUnterminatedLiteral { get; set; }
        
        /// <summary>
        /// Set to true if there is an open double quote but no close double quote
        /// </summary>
        public bool HasUnterminatedJsonKeyName { get; set; }
        
        /// <summary>
        /// Used to determine where unterminated literal begins
        /// </summary>
        public int LastStringLiteralBegin { get; set; }
        
        /// <summary>
        /// Used to determine where JSON Key begins
        /// </summary>
        public int LastJsonKeyBegin { get; set; }
    }
}