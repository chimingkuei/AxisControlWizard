using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DeepWise.Expression
{
    [DebuggerDisplay("{DebugText}")]
    public class Token
    {
        string DebugText
        {
            get
            {
                switch(TokenType)
                {
                    case TokenType.Identifier:return Identifier;
                    case TokenType.Object:return Value.ToString();
                    case TokenType.EOF: return "EOF";
                    default: return TokenType.TryGetSymbol(out var symbol) ? symbol.ToString() : TokenType.ToString();
                }
            }
        }

        public Token(TokenType type)
        {
            TokenType = type;
        }
        public Token(TokenType type, string identifier)
        {
            TokenType = type;
            Identifier = identifier;
        }
        public Token(TokenType type, object obj)
        {
            TokenType = type;
            if (obj is Token)
                throw new Exception("obj can't be a token");
            if (obj is int | obj is float)
                Value = Convert.ToDouble(obj);
            else
                Value = obj;
        }

        public bool IsPunctuationMark => !new TokenType[] { TokenType.Object , TokenType.Number, TokenType.Object, TokenType.String }.Contains(TokenType);

        public TokenType TokenType { get; }
        public object Value { get; }
        public string Identifier { get; }
    }
    public class Tokenizer
    {
        public Tokenizer(string str)
        {
            reader = new StringReader(str);
            NextChar();
            NextToken();
        }
        public Tokenizer(TextReader reader)
        {
            this.reader = reader;
            NextChar();
            NextToken();
        }

        TextReader reader;
        char currentChar;

        public TokenType Token { get; private set; }
        public double Number { get; private set; }
        public string Identifier { get; private set; }
        public string String { get; private set; }
        void NextChar()
        {
            int ch = reader.Read();
            currentChar = ch < 0 ? '\0' : (char)ch;
        }

        // Read the next token from the input stream
        public void NextToken()
        {
            // Skip whitespace
            while (char.IsWhiteSpace(currentChar) || currentChar == '\n')
            {
                NextChar();
            }

            //String
            if(currentChar == '"')
            {
                var sb = new StringBuilder();
                NextChar();
                //[New]
                bool end = false;
                while (true)
                {
                    switch(currentChar)
                    {
                        case '\\':
                            NextChar();
                            switch (currentChar)
                            {
                                case '\\':
                                    sb.Append('\\');
                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case '"':
                                    sb.Append('"');
                                    break;
                                case '\0':
                                    throw new Exception("找不到對應的引號字元(\")");
                                default:throw new Exception("無法辨識符號\\"+currentChar);
                            }
                            break;
                        case '"':
                            end = true;
                            break;
                        case '\0':
                            throw new Exception("找不到對應的引號字元(\")");
                        default:
                            {
                                sb.Append(currentChar);
                                break;
                            }
                    }
                    if (end) break;
                    NextChar();
                }
                NextChar();
                Token = TokenType.String;
                String = sb.ToString();
                return;
            }

            //Number(Or Guid)
            if (char.IsDigit(currentChar) || currentChar == '.' && char.IsDigit((char)reader.Peek()))
            {
                // Capture digits/decimal point
                var sb = new StringBuilder();
                while (currentChar == '.' || char.IsLetterOrDigit(currentChar))
                {
                    if(sb.Length ==32 && currentChar =='.')
                    {
                        string uuid = sb.ToString();
                        if (Guid.TryParse(uuid, out _))
                        {
                            Token = TokenType.Identifier;
                            Identifier = uuid;
                            return;
                        }
                    }
                    sb.Append(currentChar);
                    NextChar();
                }

                var s = sb.ToString();
                // Parse it

                if(Guid.TryParse(s, out _))
                {
                    Token = TokenType.Identifier;
                    Identifier = s;
                    return;
                }
                else if(double.TryParse(s,out var number))
                {
                    Token = TokenType.Number;
                    Number = number;
                    return;
                }

                if (s.All(x => char.IsDigit(x) || x == '.'))
                    throw new Exception("數字格式錯誤，小數點數量不可多於1位 : " + sb.ToString());
                else
                    throw new Exception("無法識別項目" + sb.ToString());
            }

            //KeyWords
            if (currentChar.TryGetToken(out var tokenType))
            {
                Token = tokenType;
                if (tokenType != TokenType.EOF) NextChar();
                return;
            }


            //if(currentChar == '.')
            //{
            //    NextChar();
            //    Token = TokenType.Dot;
            //    return;
            //}
            
            // Identifier - starts with letter or underscore
            var sb2 = new StringBuilder();
            while (!TokenHelper.EscapeChar.Contains(currentChar))
            {
                if(!char.IsWhiteSpace(currentChar))
                sb2.Append(currentChar);
                NextChar();
            }
            // Setup token
            var str = sb2.ToString();
            Identifier = sb2.ToString();
            Token = TokenType.Identifier;
            return;
        }
    }
    public enum TokenType
    {
        //Arithmetic===========================
        [Symbol('+'),Display(Name = "+")]
        Add,
        [Symbol('-'),Display(Name = "-")]
        Subtract,
        [Symbol('*'),Display(Name = "*")]
        Multiply,
        [Symbol('/'),Display(Name = "/")]
        Divide,
        [Symbol('^'),Display(Name = "^")]
        Pow,
        [Symbol('%'),Display(Name = "%")]
        Mod,
        //Equvalent============================
        [Symbol('='),Display(Name = "=")]
        Equal,
        [Symbol('≠'),Display(Name = "≠")]
        NotEqual,
        [Symbol('>'),Display(Name = ">")]
        Greater,
        [Symbol('≥'),Display(Name = "≥")]
        GreaterOrEqual,
        [Symbol('<'),Display(Name = "<")]
        Less,
        [Symbol('≤'),Display(Name = "≤")]
        LessOrEqual,
        //Logic Operation======================
        [Symbol('&'),Display(Name = "&")]
        And,
        [Symbol('|'),Display(Name = "|")]
        Or,
        //Parentheses==========================
        [Symbol('('),Display(Name = "(")]
        OpenParenthesis,
        [Symbol(')'),Display(Name = ")")]
        CloseParenthesis,
        [Symbol('['),Display(Name = "[")]
        OpenSquareBracket,
        [Symbol(']'),Display(Name = "]")]
        CloseSquareBracket,
        //==================================
        [Symbol('.'),Display(Name = ".")]
        Dot,
        [Symbol(','),Display(Name = ",")]
        Comma,

        //[Symbol('\"')]
        //[Display(Name = "\"")]
        //InvertedComma,
        //還沒使用==========================================

        [Symbol('\0'),Display(Name = "EOF")]
        EOF,
        Identifier,
        Number,
        String,
        Object,
    }
    public static class TokenHelper
    {
        static char[] _keywords;
        static Dictionary<char, TokenType> dic;
        static void Ini()
        {
            var symbols = new List<char>();
            dic = new Dictionary<char, TokenType>();
            foreach (TokenType token in Enum.GetValues(typeof(TokenType)))
            {
                if (token.TryGetSymbol(out char symbol))
                {
                    symbols.Add(symbol);
                    dic.Add(symbol, token);
                }
            }
            _keywords = symbols.ToArray();
        }
        public static char[] EscapeChar
        {
            get
            {
                if (_keywords == null) Ini();
                return _keywords;
       
            }
        }

        public static bool TryGetSymbol(this TokenType value, out char symbol)
        {
            var field = typeof(TokenType).GetField(value.ToString());
            if (field != null)
            {
                var attr = field.GetCustomAttributes(typeof(SymbolAttribute), true).SingleOrDefault() as SymbolAttribute;
                if (attr != null)
                {
                    symbol = attr.Symbol;
                    return true;
                }
            }
            symbol = '\0';
            return false;
        }
        public static bool TryGetToken(this char symbol,out TokenType token)
        {
            if (dic == null) Ini();
            if(dic.ContainsKey(symbol))
            {
                token = dic[symbol];
                return true;
            }
            else
            {
                token = default;
                return false;
            }
        }
    }

    public class SymbolAttribute : Attribute
    { 
        public SymbolAttribute(char symbol)
        {
            Symbol = symbol;
        }
        public char Symbol { get; }
    }
}
